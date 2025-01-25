using CPH.BLL.Interfaces;
using CPH.Common.DTO.Email;
using CPH.Common.Template;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CPH.BLL.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly Random random = new Random();
        private readonly IAccountService _accountService;

        public EmailService(IConfiguration configuration,
            IAccountService accountService)
        {
            _configuration = configuration;
            _accountService = accountService;
        }

        public OtpCodeDTO GenerateOTP()
        {
            int otpCode = random.Next(100000, 1000000);
            var otpDto = new OtpCodeDTO
            {
                OTPCode = otpCode.ToString(),
                ExpiredTime = DateTime.Now.AddMinutes(15),
            };
            return otpDto;
        }

        public async Task SendOTPEmail(string userEmail, string userName, OtpCodeDTO otpCode, string subject)
        {
            var user = await _accountService.GetAccountByEmail(userEmail);
            if (user != null)
            {
                var sendEmail = _configuration.GetSection("SendEmailAccount")["Email"];
                var toEmail = userEmail;
                var htmlBody = EmailTemplate.OTPEmailTemplate(userName, otpCode.OTPCode, subject);
                MailMessage mailMessage = new MailMessage(sendEmail, toEmail, subject, htmlBody);
                mailMessage.IsBodyHtml = true;

                var smtpServer = _configuration.GetSection("SendEmailAccount")["SmtpServer"];
                int.TryParse(_configuration.GetSection("SendEmailAccount")["Port"], out int port);
                var userNameEmail = _configuration.GetSection("SendEmailAccount")["UserName"];
                var password = _configuration.GetSection("SendEmailAccount")["Password"];

                SmtpClient client = new SmtpClient(smtpServer, port);
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(userNameEmail, password);
                client.EnableSsl = true;

                client.Send(mailMessage);
            }
        }
    }
}
