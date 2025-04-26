using System.ComponentModel.DataAnnotations;
using CPH.BLL.Interfaces;
using CPH.BLL.Services;
using CPH.Common.DTO.General;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CPH.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        private readonly IMemberService _memberService;
        public MemberController(IMemberService member)
        {
            _memberService = member;
        }

        [Authorize(Roles = "Lecturer,Department Head")]
        [HttpGet("all-member-of-project")]
        public async Task<IActionResult> GetAllMemberProject([FromQuery][Required] Guid projectId,
                                                                [FromQuery] string? searchValue,
                                                                [FromQuery] int? pageNumber,
                                                                [FromQuery] int? rowsPerPage)
        {
            ResponseDTO responseDTO = await _memberService.GetAllMemberOfProject(projectId, searchValue, pageNumber, rowsPerPage);
            if (responseDTO.IsSuccess == false)
            {
                if (responseDTO.StatusCode == 400)
                {
                    return NotFound(responseDTO);
                }
                if (responseDTO.StatusCode == 500)
                {
                    return BadRequest(responseDTO);
                }
            }
            return Ok(responseDTO);
        }

        [Authorize(Roles = "Lecturer,Department Head")]
        [HttpDelete("member")]
        public async Task<IActionResult> RemoveMemberFromProject(Guid memberId)
        {
            var result = await _memberService.RemoveMemberFromProject(memberId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [Authorize(Roles = "Lecturer,Department Head")]
        [HttpGet("search-student-assigning-to-class")]
        public IActionResult SearchStudentForAssigningToClass(string? searchValue)
        {
            var result = _memberService.SearchStudentForAssigningToClass(searchValue);
            if (result.Count > 0)
            {
                return Ok(new ResponseDTO("Tìm kiếm sinh viên thành công", 200, true, result));
            }
            else
            {
                return NotFound(new ResponseDTO("Không tìm thấy sinh viên", 404, false));
            }
        }
    }
}
