using CPH.BLL.Interfaces;
using CPH.BLL.Services;
using CPH.Common.DTO.General;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CPH.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LecturerController : ControllerBase
    {
        private readonly ILecturerService _lecturerService;

        public LecturerController(ILecturerService lecturerService)
        {
            _lecturerService = lecturerService;
        }

        [HttpGet("search-lecturer")]
        public IActionResult SearchLecturer(string? searchValue)
        {
            var result = _lecturerService.SearchLecturer(searchValue);
            if (result.Count > 0)
            {
                return Ok(new ResponseDTO("Tìm kiếm giảng viên thành công", 200, true, result));
            }
            else
            {
                return NotFound(new ResponseDTO("Không tìm thấy giảng viên", 404, false));
            }
        }

        [HttpGet("all-lecturer-of-project")]
        public async Task<IActionResult> GetAllLecturerProject([FromQuery] Guid projectId,
                                                                [FromQuery] string? searchValue,
                                                                [FromQuery] int? pageNumber,
                                                                [FromQuery] int? rowsPerPage)
        {
            ResponseDTO responseDTO = await _lecturerService.GetAllLecturerOfProject(projectId, searchValue, pageNumber, rowsPerPage);
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

        [HttpDelete("lecturer")]
        public async Task<IActionResult> RemoveLectureFromProject(Guid lecturerId, Guid classId)
        {
            var result = await _lecturerService.RemoveLecturerFromProject(lecturerId, classId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("search-lecturer-assigning-pm")]
        public IActionResult SearchLecturerForAssigningPM(string? searchValue, Guid projectId)
        {
            var result = _lecturerService.SearchLecturerForAssigningPM(searchValue, projectId);
            if (result.Count > 0)
            {
                return Ok(new ResponseDTO("Tìm kiếm giảng viên thành công", 200, true, result));
            }
            else
            {
                return NotFound(new ResponseDTO("Không tìm thấy giảng viên", 404, false));
            }
        }
    }
}
