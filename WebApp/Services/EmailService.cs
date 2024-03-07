using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;

namespace WebApp.Services
{
    public class MailSettings
    {
        public string Mail { get; set; }
        public string DisplayName { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }

    }

    public class EmailService : IEmailSender
    {
        private readonly MailSettings _mailSettings;

        private readonly ILogger<EmailService> logger;


        // mailSetting được Inject qua dịch vụ hệ thống
        // Có inject Logger để xuất log
        public EmailService(IOptions<MailSettings> mailSettings, ILogger<EmailService> _logger)
        {
            _mailSettings = mailSettings.Value;
            logger = _logger;
            logger.LogInformation("Create SendMailService");
        }


        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            MailboxAddress emailFrom = new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail);
            message.From.Add(emailFrom);
            message.To.Add(MailboxAddress.Parse(email));

            //message.Sender = new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail);
            //message.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail));
            //message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;


            var builder = new BodyBuilder();
            builder.HtmlBody = htmlMessage;
            message.Body = builder.ToMessageBody();

            // dùng SmtpClient của MailKit
            using var smtp = new SmtpClient();

            try
            {
                await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port,
                    SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_mailSettings.Mail, _mailSettings.Password);
                await smtp.SendAsync(message);
                await smtp.DisconnectAsync(true);
            }

            catch (Exception ex)
            {
                // Gửi mail thất bại, nội dung email sẽ lưu vào thư mục mailssave
                Directory.CreateDirectory("mailssave");
                var emailsavefile = string.Format(@"mailssave/{0}.eml", Guid.NewGuid());
                await message.WriteToAsync(emailsavefile);

                logger.LogInformation("Lỗi gửi mail, lưu tại - " + emailsavefile);
                logger.LogError(ex.Message);
            }

            smtp.Disconnect(true);

            logger.LogInformation("send mail to " + email);
        }
    }
}
