using CPH.BLL.Interfaces;
using CPH.BLL.Services;
using CPH.Common.DTO.General;
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

        [HttpGet("all-member-of-project")]
        public async Task<IActionResult> GetAllMemberProject([FromQuery] Guid projectId,
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

    }
}
