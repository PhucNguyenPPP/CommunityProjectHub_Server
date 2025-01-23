using CPH.BLL.Interfaces;
using CPH.BLL.Services;
using CPH.Common.DTO.General;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CPH.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;
        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpGet("all-project")]
        public async Task<IActionResult> GetAllProject([FromQuery]string? searchValue, 
                                                        [FromQuery] int? pageNumber, 
                                                        [FromQuery] int? rowsPerPage, 
                                                        [FromQuery] string? filterField, 
                                                        [FromQuery] string? filterOrder)
        {
            ResponseDTO responseDTO = await _projectService.GetAllProject(searchValue, pageNumber, rowsPerPage, filterField, filterOrder);
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
