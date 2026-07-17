using Aura.Application.Common;
using Aura.Application.DTOs.Statistics;
using Aura.Domain.Enum;
using Aura.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aura.API.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AnalyticsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AnalyticsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<AnalyticsDashboardDTO>>> GetDashboard(
        [FromQuery] int months = 12,
        [FromQuery] int recentTake = 20)
    {
        if (months < 1 || months > 60)
            return BadRequest(ApiResponse<AnalyticsDashboardDTO>.ErrorResponse("Số tháng phải từ 1 đến 60."));

        if (recentTake < 1 || recentTake > 100)
            return BadRequest(ApiResponse<AnalyticsDashboardDTO>.ErrorResponse("Số giao dịch gần đây phải từ 1 đến 100."));

        var now = DateTime.UtcNow;
        var firstDayThisMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var firstDayLastMonth = firstDayThisMonth.AddMonths(-1);
        var periodStart = firstDayThisMonth.AddMonths(-months + 1);
        var previousPeriodStart = periodStart.AddMonths(-months);

        var completedPayments = _context.Payments
            .AsNoTracking()
            .Where(payment => payment.Status == PaymentStatus.Completed);

        var totalRevenue = await completedPayments.SumAsync(payment => (decimal?)payment.Amount) ?? 0;
        var revenueThisMonth = await completedPayments
            .Where(payment => payment.CreatedAt >= firstDayThisMonth && payment.CreatedAt <= now)
            .SumAsync(payment => (decimal?)payment.Amount) ?? 0;
        var revenueLastMonth = await completedPayments
            .Where(payment => payment.CreatedAt >= firstDayLastMonth && payment.CreatedAt < firstDayThisMonth)
            .SumAsync(payment => (decimal?)payment.Amount) ?? 0;

        var operationalProjects = _context.Projects.AsNoTracking()
            .Where(project =>
                !EF.Functions.ILike(project.Name, "%membership%") &&
                !EF.Functions.ILike(project.Name, "%hội viên%") &&
                !EF.Functions.ILike(project.Package.Name, "%membership%") &&
                !EF.Functions.ILike(project.Package.Name, "%hội viên%"));

        var totalProjects = await operationalProjects.CountAsync();
        var projectsScheduled = await operationalProjects.CountAsync(project => project.Status == ProjectStatus.Scheduled);
        var projectsInProduction = await operationalProjects.CountAsync(project => project.Status == ProjectStatus.InProduction);
        var projectsCompleted = await operationalProjects.CountAsync(project => project.Status == ProjectStatus.Completed);
        var projectsCancelled = await operationalProjects.CountAsync(project => project.Status == ProjectStatus.Cancelled);
        var paidProjects = await completedPayments
            .Where(payment => operationalProjects.Any(project => project.Id == payment.ProjectId))
            .Select(payment => payment.ProjectId).Distinct().CountAsync();
        var awaitingPaymentProjects = await operationalProjects.CountAsync(project =>
            project.Status != ProjectStatus.Cancelled &&
            (project.Payments.Where(payment => payment.Status == PaymentStatus.Completed)
                .Sum(payment => (decimal?)payment.Amount) ?? 0) < project.Revenue);
        var outstandingAmount = await operationalProjects
            .Where(project => project.Status != ProjectStatus.Cancelled)
            .Select(project => project.Revenue - (project.Payments
                .Where(payment => payment.Status == PaymentStatus.Completed)
                .Sum(payment => (decimal?)payment.Amount) ?? 0))
            .Where(amount => amount > 0)
            .SumAsync(amount => (decimal?)amount) ?? 0;

        var totalCustomers = await _context.Users.AsNoTracking().CountAsync(user => user.Role.Name == "User");
        var totalStaff = await _context.Users.AsNoTracking().CountAsync(user => user.Role.Name == "Photographer");
        var newCustomersThisMonth = await _context.Users.AsNoTracking().CountAsync(user =>
            user.Role.Name == "User" && user.CreatedAt >= firstDayThisMonth && user.CreatedAt <= now);
        var unreadMessages = await _context.ContactMessages.AsNoTracking().CountAsync(message => !message.IsRead);

        var revenueGrowth = CalculateGrowth(revenueThisMonth, revenueLastMonth);
        var conversionRate = totalProjects > 0 ? Math.Round((double)paidProjects / totalProjects * 100, 1) : 0;

        var revenueByPackage = await completedPayments
            .GroupBy(payment => payment.Project.Package.Name)
            .ToDictionaryAsync(group => group.Key, group => group.Sum(payment => payment.Amount));
        var projectsByPackage = await _context.Projects.AsNoTracking()
            .GroupBy(project => project.Package.Name)
            .ToDictionaryAsync(group => group.Key, group => group.Count());

        var recentRows = await completedPayments
            .OrderByDescending(payment => payment.CreatedAt)
            .Take(recentTake)
            .Select(payment => new RecentPaymentDTO
            {
                PaymentId = payment.Id,
                TransactionId = payment.TransactionId ?? payment.OrderCode,
                ProjectId = payment.ProjectId,
                ProjectName = payment.Project.Name,
                CustomerName = payment.User.FullName,
                PackageName = payment.Project.Package.Name,
                Amount = payment.Amount,
                Status = payment.Status.ToString(),
                PaidAt = payment.CreatedAt
            })
            .ToListAsync();

        var unassignedProjects = await _context.Projects.AsNoTracking()
            .Where(project => project.StaffId == null &&
                project.Status != ProjectStatus.Completed && project.Status != ProjectStatus.Cancelled &&
                !EF.Functions.ILike(project.Name, "%membership%") &&
                !EF.Functions.ILike(project.Name, "%hội viên%") &&
                !EF.Functions.ILike(project.Package.Name, "%membership%") &&
                !EF.Functions.ILike(project.Package.Name, "%hội viên%"))
            .OrderBy(project => project.Deadline)
            .Take(50)
            .Select(project => new OverviewProjectDTO
            {
                ProjectId = project.Id,
                ProjectName = project.Name,
                CustomerName = project.Client.FullName,
                PackageName = project.Package.Name,
                PhotographerName = null,
                Deadline = project.Deadline
            })
            .ToListAsync();

        var upcomingProjects = await _context.Projects.AsNoTracking()
            .Where(project => project.Deadline >= now &&
                project.Status != ProjectStatus.Completed && project.Status != ProjectStatus.Cancelled &&
                !EF.Functions.ILike(project.Name, "%membership%") &&
                !EF.Functions.ILike(project.Name, "%hội viên%") &&
                !EF.Functions.ILike(project.Package.Name, "%membership%") &&
                !EF.Functions.ILike(project.Package.Name, "%hội viên%"))
            .OrderBy(project => project.Deadline)
            .Take(50)
            .Select(project => new OverviewProjectDTO
            {
                ProjectId = project.Id,
                ProjectName = project.Name,
                CustomerName = project.Client.FullName,
                PackageName = project.Package.Name,
                PhotographerName = project.Staff != null ? project.Staff.FullName : null,
                Deadline = project.Deadline
            })
            .ToListAsync();

        var monthlyRows = await completedPayments
            .Where(payment => payment.CreatedAt >= periodStart && payment.CreatedAt <= now)
            .GroupBy(payment => new { payment.CreatedAt.Year, payment.CreatedAt.Month })
            .Select(group => new MonthlyRevenueDTO
            {
                Year = group.Key.Year,
                Month = group.Key.Month,
                Revenue = group.Sum(payment => payment.Amount),
                ProjectCount = group.Select(payment => payment.ProjectId).Distinct().Count()
            })
            .ToListAsync();

        var monthlyLookup = monthlyRows.ToDictionary(row => (row.Year, row.Month));
        var filledMonthlyRevenue = Enumerable.Range(0, months)
            .Select(offset => periodStart.AddMonths(offset))
            .Select(month => monthlyLookup.TryGetValue((month.Year, month.Month), out var row)
                ? row
                : new MonthlyRevenueDTO { Year = month.Year, Month = month.Month, Revenue = 0, ProjectCount = 0 })
            .ToList();

        var packageRows = await _context.Packages.AsNoTracking()
            .Select(package => new
            {
                package.Id,
                package.Name,
                OrderCount = package.Projects.Count(project => project.Payments.Any(payment =>
                    payment.Status == PaymentStatus.Completed && payment.CreatedAt >= periodStart && payment.CreatedAt <= now)),
                Revenue = package.Projects.SelectMany(project => project.Payments)
                    .Where(payment => payment.Status == PaymentStatus.Completed && payment.CreatedAt >= periodStart && payment.CreatedAt <= now)
                    .Sum(payment => (decimal?)payment.Amount) ?? 0,
                PreviousRevenue = package.Projects.SelectMany(project => project.Payments)
                    .Where(payment => payment.Status == PaymentStatus.Completed && payment.CreatedAt >= previousPeriodStart && payment.CreatedAt < periodStart)
                    .Sum(payment => (decimal?)payment.Amount) ?? 0,
                LastPurchasedAt = package.Projects.SelectMany(project => project.Payments)
                    .Where(payment => payment.Status == PaymentStatus.Completed && payment.CreatedAt >= periodStart && payment.CreatedAt <= now)
                    .Max(payment => (DateTime?)payment.CreatedAt)
            })
            .ToListAsync();

        var periodRevenue = packageRows.Sum(row => row.Revenue);
        var packageRanking = packageRows
            .Select(row => new PackageRankingDTO
            {
                PackageId = row.Id,
                PackageName = row.Name,
                OrderCount = row.OrderCount,
                Revenue = row.Revenue,
                RevenueShare = periodRevenue > 0 ? Math.Round((double)(row.Revenue / periodRevenue * 100), 1) : 0,
                Growth = CalculateGrowth(row.Revenue, row.PreviousRevenue),
                LastPurchasedAt = row.LastPurchasedAt
            })
            .OrderByDescending(row => row.OrderCount)
            .ThenByDescending(row => row.Revenue)
            .ThenByDescending(row => row.LastPurchasedAt)
            .ToList();

        var photographerPerformance = await _context.Users.AsNoTracking()
            .Where(user => user.Role.Name == "Photographer")
            .Select(user => new PhotographerPerformanceDTO
            {
                PhotographerId = user.Id,
                PhotographerName = user.FullName,
                TotalAssigned = user.StaffProjects.Count,
                Completed = user.StaffProjects.Count(project => project.Status == ProjectStatus.Completed),
                InProgress = user.StaffProjects.Count(project =>
                    project.Status == ProjectStatus.InProduction || project.Status == ProjectStatus.Scheduled),
                Cancelled = user.StaffProjects.Count(project => project.Status == ProjectStatus.Cancelled),
                TotalRevenue = user.StaffProjects.SelectMany(project => project.Payments)
                    .Where(payment => payment.Status == PaymentStatus.Completed)
                    .Sum(payment => (decimal?)payment.Amount) ?? 0
            })
            .OrderByDescending(row => row.TotalRevenue)
            .ThenByDescending(row => row.Completed)
            .ToListAsync();

        var dashboard = new AnalyticsStatsDTO
        {
            TotalProjects = totalProjects,
            ProjectsInProduction = projectsInProduction,
            ProjectsScheduled = projectsScheduled,
            ProjectsCompleted = projectsCompleted,
            ProjectsCancelled = projectsCancelled,
            TotalRevenue = totalRevenue,
            RevenueThisMonth = revenueThisMonth,
            RevenueLastMonth = revenueLastMonth,
            RevenueGrowth = revenueGrowth,
            AverageOrderValue = paidProjects > 0 ? totalRevenue / paidProjects : 0,
            OutstandingAmount = outstandingAmount,
            PaidProjects = paidProjects,
            TotalCustomers = totalCustomers,
            TotalStaff = totalStaff,
            NewCustomersThisMonth = newCustomersThisMonth,
            ConversionRate = conversionRate,
            RevenueByPackage = revenueByPackage,
            ProjectsByCategory = projectsByPackage,
            TotalBookings = totalProjects - projectsCancelled,
            BookingsThisMonth = await operationalProjects.CountAsync(project => project.CreatedAt >= firstDayThisMonth && project.CreatedAt <= now),
            CancelledThisMonth = await operationalProjects.CountAsync(project => project.Status == ProjectStatus.Cancelled && project.UpdatedAt >= firstDayThisMonth && project.UpdatedAt <= now),
            TotalActivePackages = await _context.Packages.AsNoTracking().CountAsync(package => package.IsActive),
            GeneratedAt = now
        };

        var overview = new OverviewStatsDTO
        {
            RevenueThisMonth = revenueThisMonth,
            ProjectsInProduction = projectsInProduction,
            AwaitingPaymentProjects = awaitingPaymentProjects,
            UnreadMessages = unreadMessages,
            UnassignedProjects = unassignedProjects,
            UpcomingProjects = upcomingProjects,
            RecentPayments = recentRows,
            GeneratedAt = now
        };

        var response = new AnalyticsDashboardDTO
        {
            Dashboard = dashboard,
            Overview = overview,
            MonthlyRevenue = filledMonthlyRevenue,
            PackageRanking = packageRanking,
            RecentPayments = recentRows,
            PhotographerPerformance = photographerPerformance,
            Months = months,
            PeriodStart = periodStart,
            PeriodEnd = now,
            GeneratedAt = now
        };

        return Ok(ApiResponse<AnalyticsDashboardDTO>.SuccessResponse(response, "Lấy dữ liệu phân tích thành công."));
    }

    private static double CalculateGrowth(decimal current, decimal previous)
    {
        if (previous == 0) return current > 0 ? 100 : 0;
        return Math.Round((double)((current - previous) / previous * 100), 1);
    }
}
