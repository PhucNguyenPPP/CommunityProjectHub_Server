using CPH.BLL.Interfaces;
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
        public IActionResult GetAllAccounts()
        {
            var list = _accountService.GetAllAccounts();
            return Ok(new ResponseDTO("Get all accounts successfully", 200, true, list));
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
    }
}
