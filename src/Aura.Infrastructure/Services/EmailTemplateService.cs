using Aura.Application.Interfaces;

namespace Aura.Infrastructure.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        public string GetPaymentSuccessCustomerTemplate(string fullName, string projectName, string packageName, decimal amount, string transactionId)
        {
            return $@"
                <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; border: 1px solid #eee; padding: 30px;'>
                    <div style='border-bottom: 2px solid #ADFF00; padding-bottom: 10px; margin-bottom: 20px;'>
                        <h2 style='margin: 0; color: #000;'>AURA PRODUCTION HOUSE</h2>
                    </div>
                    
                    <p>Kính gửi <strong>{fullName}</strong>,</p>
                    
                    <p>Hệ thống đã ghi nhận khoản thanh toán thành công cho dự án <strong>{projectName}</strong>.</p>
                    <p>Dự án của bạn đã được chuyển sang trạng thái <strong>Đang thực hiện (In Production)</strong>. Đội ngũ AURA sẽ bắt đầu triển khai các bước tiếp theo ngay lập tức.</p>
                    
                    <div style='background-color: #f9f9f9; padding: 15px; border-radius: 4px; margin: 20px 0;'>
                        <h4 style='margin-top: 0; border-bottom: 1px solid #ddd; padding-bottom: 5px;'>Chi tiết thanh toán</h4>
                        <table style='width: 100%; border-collapse: collapse;'>
                            <tr>
                                <td style='padding: 8px 0; color: #666;'>Tên dự án:</td>
                                <td style='padding: 8px 0;'><strong>{projectName}</strong></td>
                            </tr>
                            <tr>
                                <td style='padding: 8px 0; color: #666;'>Gói dịch vụ:</td>
                                <td style='padding: 8px 0;'>{packageName}</td>
                            </tr>
                            <tr>
                                <td style='padding: 8px 0; color: #666;'>Số tiền đã nhận:</td>
                                <td style='padding: 8px 0;'><span style='color: #071FD9; font-weight: 700;'>{amount:N0} VNĐ</span></td>
                            </tr>
                            <tr>
                                <td style='padding: 8px 0; color: #666;'>Mã giao dịch:</td>
                                <td style='padding: 8px 0; font-family: monospace;'>{transactionId}</td>
                            </tr>
                        </table>
                    </div>
                    
                    <p>Cảm ơn quý khách đã tin tưởng và lựa chọn dịch vụ của AURA.</p>
                    <p style='margin-top: 40px;'>Trân trọng,<br /><strong>Ban quản trị AURA</strong></p>
                </div>";
        }

        public string GetOtpEmailTemplate(string fullName, string otp)
        {
            return $@"
                <div style='font-family: sans-serif; padding: 20px; border: 1px solid #eee; border-radius: 10px;'>
                    <h2 style='color: #071fd9;'>Mã xác thực đổi mật khẩu</h2>
                    <p>Chào <b>{fullName}</b>,</p>
                    <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản Aura. Mã OTP của bạn là:</p>
                    <div style='font-size: 24px; font-weight: bold; color: #071fd9; padding: 10px; background: #f0f2ff; display: inline-block; border-radius: 5px; letter-spacing: 5px;'>
                        {otp}
                    </div>
                    <p style='margin-top: 20px;'>Mã này sẽ hết hạn trong vòng <b>5 phút</b>.</p>
                    <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này.</p>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;' />
                    <p style='font-size: 12px; color: #888;'>Aura Production House - Creative Excellence</p>
                </div>";
        }

        public string GetPaymentSuccessAdminTemplate(string fullName, string email, string projectName, string packageName, decimal amount, string transactionId)
        {
            return $@"
                <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; border: 1px solid #eee; padding: 30px;'>
                    <div style='border-bottom: 2px solid #ADFF00; padding-bottom: 10px; margin-bottom: 20px;'>
                        <h2 style='margin: 0; color: #000;'>THÔNG BÁO ĐƠN HÀNG MỚI</h2>
                    </div>
                    <p>Khách hàng: <strong>{fullName}</strong> ({email})</p>
                    <p>Dự án: <strong>{projectName}</strong></p>
                    <p>Gói: {packageName}</p>
                    <p>Số tiền: <strong>{amount:N0} VNĐ</strong></p>
                    <p>Mã GD: {transactionId}</p>
                </div>";
        }
    }
}
