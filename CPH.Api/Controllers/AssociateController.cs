using CPH.BLL.Interfaces;
using CPH.BLL.Services;
using CPH.Common.DTO.Associate;
using CPH.Common.DTO.Auth;
using CPH.Common.DTO.General;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CPH.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssociateController : ControllerBase
    {
        private readonly IAssociateService _associateService;

        public AssociateController(IAssociateService associateService)
        {
            _associateService = associateService;
        }

        [Authorize(Roles = "Lecturer,Department Head")]
        [HttpGet("search-associate-to-add-project")]
        public IActionResult SearchAssociateToAddToProject(string? searchValue)
        {
            var result = _associateService.SearchAssociateToAssignToProject(searchValue);
            if (result.Count > 0)
            {
                return Ok(new ResponseDTO("Tìm kiếm đối tác thành công", 200, true, result));
            }
            else
            {
                return NotFound(new ResponseDTO("Không tìm thấy đối tác", 404, false));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("new-associate")]
        public async Task<IActionResult> SignUpAssociate([FromForm] SignUpAssociateRequestDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseDTO(ModelState.ToString() ?? "Unknow error", 400, false, null));
            }

            var checkValid = _associateService.CheckValidationSignUpAssociate(model);
            if (!checkValid.IsSuccess)
            {
                return BadRequest(checkValid);
            }

            var signUpResult = await _associateService.SignUpAssociate(model);
            if (signUpResult)
            {
                return Created("Sign up successfully",
                    new ResponseDTO("Đăng kí tài khoản đối tác thành công", 201, true, null));
            }
            else
            {
                return BadRequest(new ResponseDTO("Đăng kí tài khoản đối tác thất bại", 400, true, null));
            }
        }
    }
}
