namespace Aura.Application.Interfaces;

public interface IEmailTemplateService
{
    string GetPaymentSuccessCustomerTemplate(string fullName, string projectName, string packageName, decimal amount, string transactionId);
    string GetPaymentSuccessAdminTemplate(string fullName, string email, string projectName, string packageName, decimal amount, string transactionId);
    string GetOtpEmailTemplate(string fullName, string otp);
}
