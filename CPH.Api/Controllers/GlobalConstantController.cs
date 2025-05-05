using CPH.BLL.Interfaces;
using CPH.BLL.Services;
using CPH.Common.DTO.General;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CPH.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GlobalConstantController : ControllerBase
    {
        private readonly IGlobalConstantService _globalConstantService;

        public GlobalConstantController(IGlobalConstantService globalConstantService)
        {
            _globalConstantService = globalConstantService;
        }
        [HttpGet("maximum-time-for-feedback")]
        public async Task<IActionResult> GetMaxTimeForFeedback()
        {
            ResponseDTO responseDTO = await _globalConstantService.GetMaxTimeForFeedback();
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
