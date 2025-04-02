using System.Buffers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations;
using CPH.BLL.Interfaces;
using CPH.BLL.Services;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Lesson;
using CPH.Common.DTO.Project;
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
        public async Task<IActionResult> GetAllProject([FromQuery] string? searchValue,
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

        [HttpGet("all-related-project")]
        public async Task<IActionResult> GetAllRelatedProject([FromQuery] string? searchValue,
                                                        [FromQuery] int? pageNumber,
                                                        [FromQuery] int? rowsPerPage,
                                                        [FromQuery] string? filterField,
                                                        [FromQuery] string? filterOrder,
                                                        [FromQuery][Required] Guid userId)
        {
            ResponseDTO responseDTO = await _projectService.GetAllRelatedProject(searchValue, pageNumber, rowsPerPage, filterField, filterOrder, userId);
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

        [HttpGet("project-detail")]
        public async Task<IActionResult> GetProjectDetail([Required] Guid projectId)
        {
            ResponseDTO responseDTO = await _projectService.GetProjectDetail(projectId);
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
        [HttpPut("inactivated-project")]
        public async Task<IActionResult> InactivateProject([FromQuery][Required]Guid projectID)
        {
            ResponseDTO responseDTO = await _projectService.InActivateProject(projectID);
            if (responseDTO.IsSuccess == false)
            {
                if (responseDTO.StatusCode == 404)
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
        [HttpPost("new-project")]
        public async Task<IActionResult> CreateProject([FromBody]NewProjectDTO projectDTO)
        {
   
            ResponseDTO responseDTO = await _projectService.CreateProject(projectDTO);
            if (responseDTO.IsSuccess == false)
            {                
                
                    return BadRequest(responseDTO);
                     
            }
            return Ok(responseDTO);
        }
        [HttpPut("project")]
        public async Task<IActionResult> UpdateProject([FromForm] UpdateProjectDTO projectDTO)
        {

            ResponseDTO responseDTO = await _projectService.UpdateProject(projectDTO);
            if (responseDTO.IsSuccess == false)
            {

                return BadRequest(responseDTO);

            }
            return Ok(responseDTO);
        }

        [HttpGet("available-project")]
        public async Task<IActionResult> GetAllAvailableProject([FromQuery] Guid userId,
                                                        [FromQuery] string? searchValue,
                                                        [FromQuery] int? pageNumber,
                                                        [FromQuery] int? rowsPerPage,
                                                        [FromQuery] string? filterField,
                                                        [FromQuery] string? filterOrder)
        {
            ResponseDTO responseDTO = await _projectService.GetAvailableProject(userId, searchValue, pageNumber, rowsPerPage, filterField, filterOrder);
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

        [HttpPut("to-up-coming-status")]
        public async Task<IActionResult> UpdateProjectStatusUpcoming(Guid projectId)
        {
            ResponseDTO responseDTO = await _projectService.UpdateProjectStatusUpcoming(projectId);
            if (responseDTO.IsSuccess)
            {
                return Ok(responseDTO);
            }
            return BadRequest(responseDTO);
        }

        [HttpPut("assign-pm-to-project")]
        public async Task<IActionResult> AssignPMToProject(Guid projectId, Guid accountId)
        {
            ResponseDTO responseDTO = await _projectService.AssignPMToProject(projectId, accountId);
            if (responseDTO.IsSuccess)
            {
                return Ok(responseDTO);
            }
            return BadRequest(responseDTO);
        }

        [HttpPost("export-final-report-of-project")]
        public async Task<IActionResult> ExportFinalReportOfProject(Guid projectId)
        {
            var check = await _projectService.CheckProjectIdExisted(projectId);
            if (!check)
            {
                return BadRequest(new ResponseDTO("Dự án không tồn tại", 400, false));
            }

            var stream = _projectService.ExportFinalReportOfProjectExcel(projectId);
            string fileName = "FinalReport.xlsx";

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPut("max-absent-percentage-and-failing-score")]
        public async Task<IActionResult> UpdateMaxAbsentPercentageAndFailingScore(UpdateAbsentPercentageFailingScoreRequestDTO model)
        {
            var result = await _projectService.UpdateMaxAbsentPercentageAndFailingScore(model);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
