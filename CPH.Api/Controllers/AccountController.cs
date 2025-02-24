using CPH.BLL.Interfaces;
using CPH.Common.DTO.Account;
using CPH.Common.DTO.General;
using CPH.DAL.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace CPH.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet("all-accounts")]
        public async Task<IActionResult> GetAllAccounts([FromQuery] string? searchValue,
                                            [FromQuery] int? pageNumber,
                                            [FromQuery] int? rowsPerPage)
        {
            var list = await _accountService.GetAllAccounts(searchValue, pageNumber, rowsPerPage);
            return Ok(list);
        }

        [HttpPost("import-account")]
        public async Task<IActionResult> ImportAccount(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ResponseDTO("File không hợp lệ", 400, false, null));
            }

            var result = await _accountService.ImportAccountFromExcel(file);
            if(result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("import-trainee-test")]
        public async Task<IActionResult> ImportTrainee(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ResponseDTO("File không hợp lệ", 400, false, null));
            }

            var result = await _accountService.ImportTraineeFromExcel(file);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("otp-verifying")]
        public async Task<IActionResult> VerifyingOtp(string email, string otp)
        {
            var result = await _accountService.VerifyingOtp(email, otp);
            if (result)
            {
                return Ok(new ResponseDTO("OTP hợp lệ", 200, true));
            }
            return BadRequest(new ResponseDTO("OTP không hợp lệ", 400, false));
        }

        [HttpPost("password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseDTO(ModelState.ToString() ?? "Unknown error", 400, false));
            }

            var result = await _accountService.ChangePassword(model);
            if (result)
            {
                return Ok(new ResponseDTO("Thay đổi mật khẩu thành công", 200, true));
            }
            return BadRequest(new ResponseDTO("Thay đổi mật khẩu không thành công", 400, false));
        }
    }
}
