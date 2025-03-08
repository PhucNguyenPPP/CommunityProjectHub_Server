using System.ComponentModel.DataAnnotations;
using CPH.BLL.Interfaces;
using CPH.BLL.Services;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Trainee;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CPH.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TraineeController : ControllerBase
    {
        private readonly ITraineeService _traineeService;
        public TraineeController(ITraineeService traineeService)
        {
            _traineeService = traineeService;
        }

        [HttpGet("all-trainee")]
        public async Task<IActionResult> GetAllTraineeOfClass([FromQuery][Required] Guid classId,
                                                        [FromQuery] string? searchValue,
                                                        [FromQuery] int? pageNumber,
                                                        [FromQuery] int? rowsPerPage,
                                                        [FromQuery] string? filterField,
                                                        [FromQuery] string? filterOrder)
        {
            ResponseDTO responseDTO = await _traineeService.GetAllTraineeOfClass(classId, searchValue, pageNumber, rowsPerPage, filterField, filterOrder);
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

        [HttpPut("update-trainee-score")]
        public async Task<IActionResult> UpdateTraineeScore(ScoreTraineeRequestDTO model)
        {
            ResponseDTO result = await _traineeService.UpdateScoreTrainee(model);
            if(result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("score-trainee-list")]
        public async Task<IActionResult> GetScoreTraineeList(Guid classId)
        {
            ResponseDTO result = await _traineeService.GetScoreTraineeList(classId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
