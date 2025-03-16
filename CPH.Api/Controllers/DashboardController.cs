﻿using CPH.BLL.Interfaces;
using CPH.Common.DTO.General;
using CPH.DAL.Entities;
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
    }
}
