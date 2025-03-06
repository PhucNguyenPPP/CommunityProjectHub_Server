using System.ComponentModel.DataAnnotations;
using CPH.BLL.Interfaces;
using CPH.BLL.Services;
using CPH.Common.DTO.General;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CPH.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectLoggingController : ControllerBase
    {
        private readonly IProjectLoggingService _projectLoggingService;
        public ProjectLoggingController(IProjectLoggingService projectLoggingService)
        {
            _projectLoggingService = projectLoggingService;
        }

        [HttpGet("all-project-logging")]
        public async Task<IActionResult> GetAllProjectLogging([FromQuery][Required] Guid projectId,
                                                                [FromQuery] string? searchValue,
                                                                [FromQuery] int? pageNumber,
                                                                [FromQuery] int? rowsPerPage)
        {
            ResponseDTO responseDTO = await _projectLoggingService.GetAllProjectLogging(projectId, searchValue, pageNumber, rowsPerPage);
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
    }
}
