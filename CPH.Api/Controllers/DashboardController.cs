using CPH.BLL.Interfaces;
using CPH.Common.DTO.General;
using CPH.DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CPH.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [Authorize(Roles = "Department Head,Business Relation")]
        [HttpGet("student-amount")]
        public async Task<IActionResult> GetAllNumberOfStudent()
        {
            ResponseDTO responseDTO = await _dashboardService.GetAllNumberOfStudent();
            if (responseDTO.IsSuccess == false)
            {
                if (responseDTO.StatusCode == 404)
                {
                    return NotFound(responseDTO);
                }
                else
                {
                    return BadRequest(responseDTO);
                }
            }
            return Ok(responseDTO);
        }

        [Authorize(Roles = "Department Head,Business Relation")]
        [HttpGet("lecturer-amount")]
        public async Task<IActionResult> GetAllNumberOfLecturer()
        {
            ResponseDTO responseDTO = await _dashboardService.GetAllNumberOfLecturer();
            if (responseDTO.IsSuccess == false)
            {
                if (responseDTO.StatusCode == 404)
                {
                    return NotFound(responseDTO);
                }
                else
                {
                    return BadRequest(responseDTO);
                }
            }
            return Ok(responseDTO);
        }

        [Authorize(Roles = "Department Head,Associate,Business Relation")]
        [HttpGet("trainee-amount")]
        public async Task<IActionResult> GetAllNumberOfTrainee(Guid accountId)
        {
            ResponseDTO responseDTO = await _dashboardService.GetAllNumberOfTrainee(accountId);
            if (responseDTO.IsSuccess == false)
            {
                if (responseDTO.StatusCode == 404)
                {
                    return NotFound(responseDTO);
                }
                else
                {
                    return BadRequest(responseDTO);
                }
            }
            return Ok(responseDTO);
        }

        [Authorize(Roles = "Lecturer,Department Head,Associate,Business Relation,Admin")]
        [HttpGet("project-amount")]
        public async Task<IActionResult> GetAllNumberOfProject(Guid accountId)
        {
            ResponseDTO responseDTO = await _dashboardService.GetAllNumberOfProject(accountId);
            if (responseDTO.IsSuccess == false)
            {
                if (responseDTO.StatusCode == 404)
                {
                    return NotFound(responseDTO);
                }
                else
                {
                    return BadRequest(responseDTO);
                }
            }
            return Ok(responseDTO);
        }

        [Authorize(Roles = "Lecturer,Department Head,Associate,Business Relation,Admin")]
        [HttpGet("project-with-status-amount")]
        public async Task<IActionResult> GetAllNumberOfProjectWithStatus(Guid accountId)
        {
            ResponseDTO responseDTO = await _dashboardService.GetAllNumberOfProjectWithStatus(accountId);
            if (responseDTO.IsSuccess == false)
            {
                if (responseDTO.StatusCode == 404)
                {
                    return NotFound(responseDTO);
                }
                else
                {
                    return BadRequest(responseDTO);
                }
            }
            return Ok(responseDTO);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("user-amount")]
        public IActionResult GetAllNumberOfUser()
        {
            ResponseDTO responseDTO = _dashboardService.GetAllNumberOfUser();
            return Ok(responseDTO);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("user-with-role-amount")]
        public IActionResult GetAllNumberOfUserByRole()
        {
            ResponseDTO responseDTO = _dashboardService.GetAllNumberOfUserByRole();
            return Ok(responseDTO);
        }

        [Authorize(Roles = "Lecturer,Department Head,Associate,Business Relation")]
        [HttpGet("progress-all-project")]
        public async Task<IActionResult> GetProgressOfAllProject(Guid accountId)
        {
            ResponseDTO responseDTO = await _dashboardService.GetProgressOfAllProject(accountId);
            return Ok(responseDTO);
        }
    }
}
