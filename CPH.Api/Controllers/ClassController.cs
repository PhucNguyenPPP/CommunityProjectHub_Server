using CPH.BLL.Interfaces;
using CPH.BLL.Services;
using CPH.Common.DTO.Class;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Project;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CPH.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassController : ControllerBase
    {
        private readonly IClassService _classService;
        public ClassController(IClassService classService)
        {
            _classService = classService;
        }

        [HttpGet("all-class-of-project")]
        public async Task<IActionResult> GetAllClassOfProject([FromQuery] Guid projectId,
                                            [FromQuery] string? searchValue,
                                            [FromQuery] int? pageNumber,
                                            [FromQuery] int? rowsPerPage)
        {
            ResponseDTO responseDTO = await _classService.GetAllClassOfProject(projectId, searchValue, pageNumber, rowsPerPage);
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

        [HttpGet("class-detail")]
        public async Task<IActionResult> GetClassDetail([FromQuery] Guid classId)
        {
            ResponseDTO responseDTO = await _classService.GetClassDetail(classId);
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
        [HttpPut("group-of-class")]
        public async Task<IActionResult> DevideGroupOfClass([FromBody] DevideGroupOfClassDTO devideGroupOfClassDTO)
        {

            ResponseDTO responseDTO = await _classService.DevideGroupOfClass(devideGroupOfClassDTO);
            if (responseDTO.IsSuccess == false)
            {

                return BadRequest(responseDTO);

            }
            return Ok(responseDTO);
        }
    }
}
