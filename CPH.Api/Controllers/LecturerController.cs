using CPH.BLL.Interfaces;
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
    }
}
