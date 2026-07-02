using Aura.Domain.Entity;
using Aura.Domain.Enum;

namespace Aura.Application.Services;

public static class PaymentPlanCalculator
{
    public const decimal InstallmentThreshold = 10_000_000m;

    public static IReadOnlyList<PaymentInstallment> BuildPlan(decimal totalAmount)
    {
        if (totalAmount < InstallmentThreshold)
        {
            return new[]
            {
                new PaymentInstallment(1, 100m, totalAmount)
            };
        }

        var firstAmount = RoundVnd(totalAmount * 0.5m);
        var secondAmount = RoundVnd(totalAmount * 0.25m);
        var thirdAmount = totalAmount - firstAmount - secondAmount;

        return new[]
        {
            new PaymentInstallment(1, 50m, firstAmount),
            new PaymentInstallment(2, 25m, secondAmount),
            new PaymentInstallment(3, 25m, thirdAmount)
        };
    }

    public static PaymentInstallment? GetNextInstallment(decimal totalAmount, IEnumerable<Payment> payments)
    {
        var completedPayments = payments
            .Where(p => p.Status == PaymentStatus.Completed)
            .ToList();

        var totalPaid = completedPayments.Sum(p => p.Amount);
        if (totalPaid >= totalAmount)
        {
            return null;
        }

        var totalRemaining = totalAmount - totalPaid;
        foreach (var installment in BuildPlan(totalAmount))
        {
            var paidForInstallment = completedPayments
                .Where(p => p.InstallmentNumber == installment.InstallmentNumber)
                .Sum(p => p.Amount);

            var installmentRemaining = installment.Amount - paidForInstallment;
            if (installmentRemaining > 0)
            {
                return installment with { Amount = Math.Min(installmentRemaining, totalRemaining) };
            }
        }

        return null;
    }

    public static int CountCompletedInstallments(decimal totalAmount, IEnumerable<Payment> payments)
    {
        var completedPayments = payments
            .Where(p => p.Status == PaymentStatus.Completed)
            .ToList();

        return BuildPlan(totalAmount)
            .Count(installment => completedPayments
                .Where(p => p.InstallmentNumber == installment.InstallmentNumber)
                .Sum(p => p.Amount) >= installment.Amount);
    }

    public static bool IsFullyPaid(decimal totalAmount, IEnumerable<Payment> payments)
    {
        return payments
            .Where(p => p.Status == PaymentStatus.Completed)
            .Sum(p => p.Amount) >= totalAmount;
    }

    private static decimal RoundVnd(decimal amount)
    {
        return Math.Round(amount, 0, MidpointRounding.AwayFromZero);
    }
}

public sealed record PaymentInstallment(
    int InstallmentNumber,
    decimal Percentage,
    decimal Amount);