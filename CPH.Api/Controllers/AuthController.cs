using CPH.BLL.Interfaces;
using CPH.Common.DTO.Auth;
using CPH.Common.DTO.General;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CPH.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("new-account")]
        public async Task<IActionResult> SignUp([FromForm] SignUpRequestDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseDTO(ModelState.ToString() ?? "Unknow error", 400, false, null));
            }

            var checkValid = await _authService.CheckValidationSignUp(model);
            if (!checkValid.IsSuccess)
            {
                return BadRequest(checkValid);
            }

            var signUpResult = await _authService.SignUp(model);
            if (signUpResult)
            {
                return Created("Sign up successfully",
                    new ResponseDTO("Đăng kí tài khoản thành công", 201, true, null));
            }
            else
            {
                return BadRequest(new ResponseDTO("Đăng kí tài khoản thất bại", 400, true, null));
            }
        }

        [HttpPost("new-account2")]
        public async Task<IActionResult> SignUp2([FromForm] SignUpRequestDTO2 model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseDTO(ModelState.ToString() ?? "Unknow error", 400, false, null));
            }

            var checkValid = await _authService.CheckValidationSignUp2(model);
            if (!checkValid.IsSuccess)
            {
                return BadRequest(checkValid);
            }

            var signUpResult = await _authService.SignUp2(model);
            if (signUpResult)
            {
                return Created("Sign up successfully",
                    new ResponseDTO("Đăng kí tài khoản thành công", 201, true, null));
            }
            else
            {
                return BadRequest(new ResponseDTO("Đăng kí tài khoản thất bại", 400, true, null));
            }
        }

        [HttpPost("sign-in")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequestDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseDTO(ModelState.ToString() ?? "Unknown error", 400, false, ModelState));
            }
            var result = await _authService.CheckLogin(loginRequestDTO);
            if (result != null)
            {
                return Ok(new ResponseDTO("Đăng nhập thành công", 200, true, result));
            }
            return BadRequest(new ResponseDTO("Đăng nhập thất bại", 400, false));
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> GetNewTokenFromRefreshToken([FromBody] RequestTokenDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseDTO(ModelState.ToString() ?? "Unknown error", 400, false));
            }

            var result = await _authService.RefreshAccessToken(model);
            if (result == null || string.IsNullOrEmpty(result.AccessToken))
            {
                return BadRequest(new ResponseDTO("Tạo refresh token thất bại", 400, false, result));
            }
            return Created("Create refresh token successfully",
                new ResponseDTO("Tạo refresh token thành công", 201, true, result));
        }

        [Authorize(Roles = "Student,Lecturer,Trainee,Department Head,Associate,Business Relation,Admin")]
        [HttpGet("/account/access-token/{accessToken}")]
        public async Task<IActionResult> GetUserByToken(string accessToken)
        {
            var result = await _authService.GetAccountByAccessToken(accessToken);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [Authorize(Roles = "Student,Lecturer,Trainee,Department Head,Associate,Business Relation,Admin")]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([Required] string rfToken)
        {

            var response = await _authService.LogOut(rfToken);

            if (response)
            {
                return Ok(new ResponseDTO("Đăng xuất thành công", 200, true));
            }

            return BadRequest(new ResponseDTO("Đăng xuất thất bại", 400, false));
        }

        [Authorize(Roles = "Student,Lecturer,Trainee,Department Head,Associate,Business Relation,Admin")]
        [HttpPost("old-password")]
        public IActionResult CheckOldPassword(CheckOldPasswordDTO model)
        {

            var response = _authService.CheckOldPassword(model);

            if (response.IsSuccess)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
    }
}
