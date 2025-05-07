using System.ComponentModel.DataAnnotations;
using CPH.BLL.Interfaces;
using CPH.BLL.Services;
using CPH.Common.DTO.General;
using CPH.Common.DTO.LessonClass;
using Microsoft.AspNetCore.Authorization;
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

        [Authorize(Roles = "Student,Lecturer,Trainee,Department Head,Associate,Business Relation")]
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

        [Authorize(Roles = "Lecturer,Department Head")]
        [HttpPut("lesson-of-class")]
        public async Task<IActionResult> UpdateLessonClass([Required]Guid projectId, [FromBody] List<UpdateLessonClassDTO> updateLessonClassDTO)
        {
            var checkValid = await _lessonClassService.CheckValidationUpdateLessonClass(projectId, updateLessonClassDTO);
            if (!checkValid.IsSuccess)
            {
                return BadRequest(checkValid);
            }

            ResponseDTO responseDTO = await _lessonClassService.UpdateLessonClass(projectId, updateLessonClassDTO);
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
    }
}
