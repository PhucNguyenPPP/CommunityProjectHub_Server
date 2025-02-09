using CPH.BLL.Interfaces;
using CPH.BLL.Services;
using CPH.Common.DTO.General;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CPH.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LessonClassController : ControllerBase
    {
        private readonly ILessonClassService _lessonClassService;
        public LessonClassController(ILessonClassService lessonClassService)
        {
            _lessonClassService = lessonClassService;
        }
        [HttpGet("lessons-of-class")]
        public async Task<IActionResult> GetLessonClass(Guid classId)
        {
            ResponseDTO responseDTO = await _lessonClassService.GetLessonClass(classId);
            if (responseDTO.IsSuccess == false)
            {
                if (responseDTO.StatusCode == 404)
                {
                    return NotFound(responseDTO);
                }
                if (responseDTO.StatusCode == 500 || responseDTO.StatusCode == 400)
                {
                    return BadRequest(responseDTO);
                }
            }
            return Ok(responseDTO);
        }
    }
}
