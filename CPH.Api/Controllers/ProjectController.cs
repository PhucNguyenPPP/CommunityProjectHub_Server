﻿using System.Buffers;
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
        public async Task<IActionResult> CreateProject([FromForm]NewProjectDTO projectDTO)
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
        public async Task<IActionResult> GetAllAvailableProject([FromQuery] string? searchValue,
                                                        [FromQuery] int? pageNumber,
                                                        [FromQuery] int? rowsPerPage,
                                                        [FromQuery] string? filterField,
                                                        [FromQuery] string? filterOrder)
        {
            ResponseDTO responseDTO = await _projectService.GetAvailableProject(searchValue, pageNumber, rowsPerPage, filterField, filterOrder);
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
