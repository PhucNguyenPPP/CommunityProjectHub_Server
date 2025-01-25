using CPH.BLL.Interfaces;
using CPH.Common.DTO.General;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CPH.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IEmailService _emailService;

        public EmailController(IAccountService accountService, IEmailService emailService)
        {
            _accountService = accountService;
            _emailService = emailService;
        }

        [HttpPost("otp-email")]
        public async Task<IActionResult> SendOtpEmail(string email)
        {
            var account = await _accountService.GetAccountByEmail(email);
            if (account == null)
            {
                return NotFound(new ResponseDTO("Email không tồn tại", 404, false));
            }

            var otpDto = _emailService.GenerateOTP();

            await _emailService.SendOTPEmail(account.Email, account.AccountName, otpDto, "Community Project Hub: OTP Code For Resetting Password");
            await _accountService.SetOtp(account.Email, otpDto);
            return Ok(new ResponseDTO("Gửi otp thành công đến " + account.Email, 201, true, otpDto));
        }

    }
}
