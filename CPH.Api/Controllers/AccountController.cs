using CPH.BLL.Interfaces;
using CPH.Common.DTO.General;
using CPH.DAL.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

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

        [HttpGet("all-account")]
        public IActionResult GetAllAccounts()
        {
            var list = _accountService.GetAllAccounts();
            return Ok(new ResponseDTO("Get all accounts successfully", 200, true, list));
        }
    }
}
