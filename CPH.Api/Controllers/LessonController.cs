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
    public class LessonController : ControllerBase
    {
        private readonly ILessonService _lessonService;
        public LessonController(ILessonService lessonService)
        {
            _lessonService = lessonService;
        }
        [HttpPut()]
        public async Task<IActionResult> UpdateLessonOfProject([FromBody] UpdateLessonOfProjectDTO lessonOfProjectDTO)
        {

            ResponseDTO responseDTO = await _lessonService.UpdateLessonOfProject(lessonOfProjectDTO);
            if (responseDTO.IsSuccess == false)
            {

                return BadRequest(responseDTO);

            }
            return Ok(responseDTO);
        }
    }
}
