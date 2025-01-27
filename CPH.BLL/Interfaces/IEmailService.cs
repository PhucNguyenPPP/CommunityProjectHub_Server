using CPH.Common.DTO.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.BLL.Interfaces
{
    public interface IEmailService
    {
        OtpCodeDTO GenerateOTP();
        Task SendOTPEmail(string userEmail, string userName, OtpCodeDTO otpCode, string subject);
    }
}
