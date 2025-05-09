﻿using CPH.BLL.Interfaces;
using CPH.BLL.Services;
using CPH.Common.DTO.General;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CPH.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;
        private readonly IClassService _classService;
        public AttendanceController(IAttendanceService attendanceService, IClassService classService)
        {
            _attendanceService = attendanceService;
            _classService = classService;
        }

        [Authorize(Roles = "Associate")]
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

        /// <summary>
        /// k xai api nay
        /// </summary>
        /// <param name="classId"></param>
        /// <returns></returns>
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

        [Authorize(Roles = "Associate")]
        [HttpPost("export-attendance")]
        public async Task<IActionResult> ExportAttendanceFile(Guid classId)
        {
            var check = await _classService.CheckClassIdExist(classId);
            if (!check)
            {
                return BadRequest(new ResponseDTO("Lớp không tồn tại", 400, false));
            }

            var stream = _attendanceService.ExportAttendanceTraineeExcel(classId);
            string fileName = "AttendanceChecking.xlsx";

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [Authorize(Roles = "Associate")]
        [HttpPost("export-attendance-template")]
        public async Task<IActionResult> ExportAttendanceTemplateFile(Guid classId)
        {
            var check = await _classService.CheckClassIdExist(classId);
            if (!check)
            {
                return BadRequest(new ResponseDTO("Lớp không tồn tại", 400, false));
            }

            var stream = _attendanceService.ExportAttendanceTraineeTemplateExcel(classId);
            string fileName = "AttendanceChecking.xlsx";

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
