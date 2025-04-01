using CPH.BLL.Interfaces;
using CPH.BLL.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CPH.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;
        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        [HttpPost("import-attendance-file")]
        public async Task<IActionResult> ImportAttendanceExcelFile(IFormFile file, Guid classId)
        {
            var result = await _attendanceService.ImportAttendanceFile(file, classId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("class-attendance")]
        public IActionResult GetAttendanceOfClass(Guid classId)
        {
            var result = _attendanceService.GetAttendanceClass(classId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
