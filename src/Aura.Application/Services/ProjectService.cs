using Aura.Application.Common;
using Aura.Application.DTOs.Project;
using Aura.Application.Interfaces;
using Aura.Application.Mappers;
using Aura.Domain.Enum;
using Aura.Domain.Interfaces;

namespace Aura.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IPackageRepository _packageRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMailService _mailService;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IEmailTemplateService _templateService;

    public ProjectService(
        IProjectRepository projectRepository,
        IPackageRepository packageRepository,
        IUserRepository userRepository,
        IMailService mailService,
        IPaymentRepository paymentRepository,
        IEmailTemplateService templateService)
    {
        _projectRepository = projectRepository;
        _packageRepository = packageRepository;
        _userRepository = userRepository;
        _mailService = mailService;
        _paymentRepository = paymentRepository;
        _templateService = templateService;
    }

    public async Task<ProjectResponseDTO> CreateProjectAsync(CreateProjectRequestDTO request)
    {
        var package = await _packageRepository.GetByIdAsync(request.PackageId);
        if (package == null || !package.IsActive)
        {
            throw new Exception(ErrorMessages.PackageNotFound);
        }

        var project = ProjectMapper.ToEntity(request, package);
        var createdProject = await _projectRepository.AddAsync(project);

        return ProjectMapper.ToDTO(createdProject);
    }

    public async Task<ProjectResponseDTO?> GetProjectByIdAsync(Guid id)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        return project == null ? null : ProjectMapper.ToDTO(project);
    }

    public async Task<IEnumerable<ProjectResponseDTO>> GetAllProjectsAsync()
    {
        var projects = await _projectRepository.GetAllAsync();
        return projects.Select(ProjectMapper.ToDTO);
    }

    public async Task<ProjectResponseDTO?> UpdateProjectAsync(UpdateProjectRequestDTO request)
    {
        var project = await _projectRepository.GetByIdAsync(request.Id);
        if (project == null) return null;

        ProjectMapper.UpdateProject(project, request);


        var updatedProject = await _projectRepository.UpdateAsync(project);
        return ProjectMapper.ToDTO(updatedProject);
    }

    public async Task<bool> UpdateProjectStatusAsync(Guid id, ProjectStatus status)
    {
        return await _projectRepository.UpdateStatusAsync(id, status);
    }

    public async Task<bool> UpdateProjectStaffAsync(Guid id, Guid staffId)
    {
        return await _projectRepository.UpdateStaffAsync(id, staffId);
    }

    public async Task<IEnumerable<ProjectResponseDTO>> GetSchedulesAsync(Guid? clientId, Guid? staffId, DateTime? from, DateTime? to)
    {
        var projects = await _projectRepository.GetSchedulesAsync(clientId, staffId, from, to);
        return projects.Select(ProjectMapper.ToDTO);
    }

    public async Task<SlotAvailabilityResponseDTO> CheckSlotAvailabilityAsync(DateTime date, int maxSlotsPerDay = 3)
    {
        var booked = (await _projectRepository.GetBookedOnDateAsync(date)).ToList();

        return new SlotAvailabilityResponseDTO
        {
            Date = date.Date,
            BookedCount = booked.Count,
            IsAvailable = booked.Count < maxSlotsPerDay,
            BookedProjectIds = booked.Select(p => p.Id)
        };
    }

    public async Task<ProjectResponseDTO?> RescheduleAsync(RescheduleRequestDTO request)
    {
        if (request.NewShootingDate.ToUniversalTime() < DateTime.UtcNow)
            throw new ArgumentException("Ngày chụp mới không được trong quá khứ.");

        var updated = await _projectRepository.RescheduleAsync(
            request.ProjectId, request.NewShootingDate.ToUniversalTime());

        return updated == null ? null : ProjectMapper.ToDTO(updated);
    }

    public async Task<bool> CancelProjectAsync(Guid projectId)
    {
        return await _projectRepository.CancelAsync(projectId);
    }

    public async Task<bool> HandlePaymentSuccessAsync(Guid projectId, decimal amount, string transactionId)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null) return false;

        if (project.Status == ProjectStatus.Completed || project.Status == ProjectStatus.Cancelled)
        {
            throw new ArgumentException($"Không thể thanh toán cho dự án đã ở trạng thái {project.Status}.");
        }

        var existingPayment = await _paymentRepository.GetByTransactionIdAsync(transactionId);
        if (existingPayment != null)
        {
            throw new ArgumentException($"Transaction '{transactionId}' has already been used for another payment.");
        }

        var projectPayments = await _paymentRepository.GetByProjectIdAsync(projectId);
        var paidAmount = projectPayments
            .Where(p => p.Status == PaymentStatus.Completed)
            .Sum(p => p.Amount);
        var remainingAmount = project.Revenue - paidAmount;

        if (amount <= 0)
        {
            throw new ArgumentException("So tien thanh toan phai lon hon 0 VND.");
        }

        if (remainingAmount <= 0)
        {
            throw new ArgumentException("Du an nay da duoc thanh toan du.");
        }

        if (amount > remainingAmount)
        {
            throw new ArgumentException($"So tien chuyen khoan ({amount:N0} VND) lon hon so tien con lai cua du an ({remainingAmount:N0} VND).");
        }

        var nextInstallment = PaymentPlanCalculator.GetNextInstallment(project.Revenue, projectPayments);
        if (nextInstallment == null)
        {
            throw new ArgumentException("Du an nay da duoc thanh toan du.");
        }

        var payment = PaymentMapper.ToEntity(project, nextInstallment, amount, transactionId);
        await _paymentRepository.AddAsync(payment);

        var updatedPayments = projectPayments.Append(payment);
        if (project.Status == ProjectStatus.Scheduled || PaymentPlanCalculator.IsFullyPaid(project.Revenue, updatedPayments))
        {
            project.Status = ProjectStatus.InProduction;
            project.UpdatedAt = DateTime.UtcNow;
            await _projectRepository.UpdateAsync(project);
        }

        try
        {
            var user = await _userRepository.GetByIdAsync(project.ClientId);
            var package = await _packageRepository.GetByIdAsync(project.PackageId);
            if (user != null && package != null)
            {
                var isMembershipPackage = package.Name.Trim()
                    .Equals("Membership", StringComparison.OrdinalIgnoreCase);
                if (isMembershipPackage &&
                    !projectPayments.Any(p => p.Status == PaymentStatus.Completed) &&
                    nextInstallment.InstallmentNumber == 1)
                {
                    user.IsVip = true;
                    if (user.VipExpireAt.HasValue && user.VipExpireAt.Value > DateTime.UtcNow)
                    {
                        user.VipExpireAt = user.VipExpireAt.Value.AddMonths(1);
                    }
                    else
                    {
                        user.VipExpireAt = DateTime.UtcNow.AddMonths(1);
                    }
                    await _userRepository.UpdateAsync(user);
                }
                // === EMAIL CHO KHÁCH HÀNG ===
                if (!string.IsNullOrEmpty(user.Email))
                {
                    string customerSubject = $"[AURA] Xác nhận thanh toán thành công: {package.Name}";
                    string customerBody = _templateService.GetPaymentSuccessCustomerTemplate(
                        user.FullName, project.Name, package.Name, amount, transactionId);

                    await _mailService.SendEmailAsync(user.Email, customerSubject, customerBody);
                }

                // === EMAIL CHO ADMIN ===
                const string adminEmail = "auraproduction2512@gmail.com";
                string adminSubject = $"[AURA] Đơn hàng mới - Thanh toán thành công: {project.Name}";
                string adminBody = _templateService.GetPaymentSuccessAdminTemplate(
                    user.FullName, user.Email, project.Name, package.Name, amount, transactionId);

                await _mailService.SendEmailAsync(adminEmail, adminSubject, adminBody);
            }
        }
        catch (Exception ex)
        {
            // Ghi log lỗi nhưng không fail toàn bộ thanh toán
            Console.WriteLine($"Failed to send payment confirmation emails: {ex.Message}");
        }

        return true;
    }
}
