using Aura.Application.DTOs.Project;
using Aura.Application.Interfaces;
using Aura.Domain.Entity;
using Aura.Domain.Enum;
using Aura.Domain.Interfaces;

namespace Aura.Infrastructure.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IPackageRepository _packageRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMailService _mailService;
        private readonly IPaymentRepository _paymentRepository;

        public ProjectService(
            IProjectRepository projectRepository, 
            IPackageRepository packageRepository,
            IUserRepository userRepository,
            IMailService mailService,
            IPaymentRepository paymentRepository)
        {
            _projectRepository = projectRepository;
            _packageRepository = packageRepository;
            _userRepository = userRepository;
            _mailService = mailService;
            _paymentRepository = paymentRepository;
        }

        public async Task<ProjectResponseDTO> CreateProjectAsync(CreateProjectRequestDTO request)
        {
            var package = await _packageRepository.GetByIdAsync(request.PackageId);
            if (package == null || !package.IsActive)
            {
                throw new Exception("Bản ghi Package không tồn tại hoặc đã ngừng cung cấp.");
            }

            var project = new Project
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                ClientId = request.ClientId,
                PackageId = request.PackageId,

                // Gán giá Revenue của Project dựa theo giá chuẩn Menu (Price) của Package
                Revenue = package.Price,

                // Tự động snapshot toàn bộ Benefits từ Package → không cần Customer nhập thủ công
                Benefits = new List<string>(package.Benefits),

                Status = ProjectStatus.Scheduled, // Cần để Scheduled để hiện mã QR thanh toán
                Deadline = DateTime.UtcNow.AddDays(7),
                Description = request.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdProject = await _projectRepository.AddAsync(project);

            // Email sẽ chỉ được gửi khi thanh toán thành công (HandlePaymentSuccessAsync)

            return MapToDTO(createdProject);
        }

        public async Task<ProjectResponseDTO?> GetProjectByIdAsync(Guid id)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            return project == null ? null : MapToDTO(project);
        }

        public async Task<IEnumerable<ProjectResponseDTO>> GetAllProjectsAsync()
        {
            var projects = await _projectRepository.GetAllAsync();
            return projects.Select(MapToDTO);
        }

        public async Task<ProjectResponseDTO?> UpdateProjectAsync(UpdateProjectRequestDTO request)
        {
            var project = await _projectRepository.GetByIdAsync(request.Id);
            if (project == null) return null;

            project.Name = request.Name;
            project.StaffId = request.StaffId;
            project.Status = request.Status;
            project.Revenue = request.Revenue; // Update lại Doanh thu thực tế nếu phát sinh
            project.Deadline = request.Deadline;
            project.Description = request.Description;
            project.ResultLink = request.ResultLink;
            project.UpdatedAt = DateTime.UtcNow;
            // Lưu ý: KHÔNG update Benefits ở đây vì đây là snapshot đã cam kết với customer

            var updatedProject = await _projectRepository.UpdateAsync(project);
            return MapToDTO(updatedProject);
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
            return projects.Select(MapToDTO);
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

            return updated == null ? null : MapToDTO(updated);
        }

        public async Task<bool> CancelProjectAsync(Guid projectId)
        {
            return await _projectRepository.CancelAsync(projectId);
        }

        public async Task<bool> HandlePaymentSuccessAsync(Guid projectId, decimal amount, string transactionId)
        {
            // 1. Tìm dự án
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null) return false;

            // 2. Kiểm tra nếu giao dịch đã tồn tại để tránh trùng lặp
            var existingPayment = await _paymentRepository.GetByTransactionIdAsync(transactionId);
            if (existingPayment != null) return true; // Đã xử lý rồi

            // 3. Cập nhật trạng thái dự án
            project.Status = ProjectStatus.InProduction;
            project.UpdatedAt = DateTime.UtcNow;
            await _projectRepository.UpdateAsync(project);

            // 4. Tạo bản ghi thanh toán
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                UserId = project.ClientId,
                ProjectId = project.Id,
                Amount = amount,
                Currency = "VND",
                TotalAmount = amount,
                OrderCode = $"AURA-{DateTime.Now:yyyyMMdd}-{transactionId.Substring(Math.Max(0, transactionId.Length - 4))}",
                PaymentMethod = Aura.Domain.Enum.PaymentMethod.VietQR,
                Gateway = "SePay",
                Status = Aura.Domain.Enum.PaymentStatus.Completed,
                TransactionId = transactionId,
                Note = $"Thanh toan cho du an {project.Name}",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _paymentRepository.AddAsync(payment);

            // 5. Gửi email thông báo (tận dụng logic có sẵn)
            _ = Task.Run(async () =>
            {
                try
                {
                    var user = await _userRepository.GetByIdAsync(project.ClientId);
                    var package = await _packageRepository.GetByIdAsync(project.PackageId);
                    if (user != null && package != null)
                    {
                        string subject = $"[AURA] Xác nhận thanh toán thành công: {package.Name}";
                        string body = $@"
                            <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; border: 1px solid #eee; padding: 30px;'>
                                <div style='border-bottom: 2px solid #ADFF00; padding-bottom: 10px; margin-bottom: 20px;'>
                                    <h2 style='margin: 0; color: #000;'>AURA PRODUCTION HOUSE</h2>
                                </div>
                                
                                <p>Kính gửi <strong>{user.FullName}</strong>,</p>
                                
                                <p>Hệ thống đã ghi nhận khoản thanh toán thành công cho dự án <strong>{project.Name}</strong>.</p>
                                <p>Dự án của bạn đã được chuyển sang trạng thái <strong>Đang thực hiện (In Production)</strong>. Đội ngũ AURA sẽ bắt đầu triển khai các bước tiếp theo ngay lập tức.</p>
                                
                                <div style='background-color: #f9f9f9; padding: 15px; border-radius: 4px; margin: 20px 0;'>
                                    <h4 style='margin-top: 0; border-bottom: 1px solid #ddd; padding-bottom: 5px;'>Chi tiết thanh toán</h4>
                                    <table style='width: 100%; border-collapse: collapse;'>
                                        <tr>
                                            <td style='padding: 8px 0; color: #666;'>Tên dự án:</td>
                                            <td style='padding: 8px 0;'><strong>{project.Name}</strong></td>
                                        </tr>
                                        <tr>
                                            <td style='padding: 8px 0; color: #666;'>Gói dịch vụ:</td>
                                            <td style='padding: 8px 0;'>{package.Name}</td>
                                        </tr>
                                        <tr>
                                            <td style='padding: 8px 0; color: #666;'>Số tiền đã nhận:</td>
                                            <td style='padding: 8px 0;'><span style='color: #071FD9; font-weight: 700;'>{amount:N0} VNĐ</span></td>
                                        </tr>
                                        <tr>
                                            <td style='padding: 8px 0; color: #666;'>Mã giao dịch:</td>
                                            <td style='padding: 8px 0; font-family: monospace;'>{transactionId}</td>
                                        </tr>
                                        <tr>
                                            <td style='padding: 8px 0; color: #666;'>Thời gian:</td>
                                            <td style='padding: 8px 0;'>{DateTime.UtcNow.AddHours(7):dd/MM/yyyy HH:mm}</td>
                                        </tr>
                                    </table>
                                </div>
                                
                                <p>Chúng tôi sẽ sớm liên hệ trực tiếp với quý khách để cập nhật tiến độ sản xuất.</p>
                                
                                <p>Cảm ơn quý khách đã tin tưởng và lựa chọn dịch vụ của AURA.</p>
                                
                                <p style='margin-top: 40px;'>Trân trọng,<br />
                                <strong>Ban quản trị AURA</strong></p>
                                
                                <div style='margin-top: 50px; padding-top: 15px; border-top: 1px solid #eee; font-size: 11px; color: #999; text-align: center;'>
                                    Đây là email thông báo tự động về giao dịch tài chính. Quý khách vui lòng lưu giữ email này để đối soát khi cần thiết.<br />
                                    © 2024 Aura Production House.
                                </div>
                            </div>";
                        await _mailService.SendEmailAsync(user.Email, subject, body);
                    }
                }
                catch { /* Ignore email failure */ }
            });

            return true;
        }

        private ProjectResponseDTO MapToDTO(Project project)
        {
            return new ProjectResponseDTO
            {
                Id = project.Id,
                Name = project.Name,
                ClientId = project.ClientId,
                ClientName = project.Client?.FullName ?? string.Empty,
                PackageId = project.PackageId,
                PackageName = project.Package?.Name ?? string.Empty,

                StaffId = project.StaffId == Guid.Empty ? null : project.StaffId,
                StaffName = project.Staff?.FullName,
                Status = project.Status,
                Revenue = project.Revenue,
                Benefits = project.Benefits,
                Deadline = project.Deadline,
                Description = project.Description,
                ResultLink = project.ResultLink,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt
            };
        }
    }
}
