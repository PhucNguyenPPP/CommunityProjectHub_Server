﻿using System.ComponentModel.DataAnnotations;
using System.IO;
using CPH.BLL.Interfaces;
using CPH.BLL.Services;
using CPH.Common.DTO.Auth;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Trainee;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CPH.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TraineeController : ControllerBase
    {
        private readonly ITraineeService _traineeService;
        private readonly IClassService _classService;
        public TraineeController(ITraineeService traineeService, IClassService classService)
        {
            _traineeService = traineeService;
            _classService = classService;
        }

        [Authorize(Roles = "Student,Lecturer,Trainee,Department Head,Associate,Business Relation")]
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

        [Authorize(Roles = "Lecturer")]
        [HttpPut("update-trainee-score")]
        public async Task<IActionResult> UpdateTraineeScore(ScoreTraineeRequestDTO model)
        {
            ResponseDTO result = await _traineeService.UpdateScoreTrainee(model);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [Authorize(Roles = "Lecturer,Department Head,Associate,Business Relation")]
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

        [Authorize(Roles = "Lecturer,Department Head")]
        [HttpDelete("trainee")]
        public async Task<IActionResult> RemoverTrainee([Required] Guid classId, [Required] Guid accountId, string? reason)
        {
            var result = await _traineeService.RemoveTrainee(classId, accountId, reason);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [Authorize(Roles = "Lecturer")]
        [HttpPost("import-trainee-score")]
        public async Task<IActionResult> ImportTraineeScoreByExcel(IFormFile file, Guid classId)
        {
            var result = await _traineeService.ImportTraineeScoreByExcel(file, classId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [Authorize(Roles = "Lecturer,Department Head,Associate,Business Relation")]
        [HttpPost("export-trainee")]
        public async Task<IActionResult> ExportTrainee(Guid classId)
        {
            var check = await _classService.CheckClassIdExist(classId);
            if (!check)
            {
                return BadRequest(new ResponseDTO("Lớp không tồn tại", 400, false));
            }

            var stream = _traineeService.ExportTraineeListExcel(classId);
            string fileName = "DanhSachHocVien.xlsx";

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [Authorize(Roles = "Lecturer")]
        [HttpPost("trainee")]
        public async Task<IActionResult> AddTraineeHadAccount([FromBody] AddTraineeHadAccountDTO addTraineeHadAccountDTO)
        {
            var result = await _traineeService.AddTraineeHadAccount(addTraineeHadAccountDTO);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [Authorize(Roles = "Trainee")]
        [HttpPut("trainee-report")]
        public async Task<IActionResult> UpdateReport([Required] Guid accountId, [Required] Guid classId, [Required] IFormFile file)
        {
            Console.WriteLine($"File received: {file?.FileName}, Size: {file?.Length}");
            var result = await _traineeService.UpdateReport(accountId, classId, file);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [Authorize(Roles = "Lecturer")]
        [HttpPost("new-account-of-trainee")]
        public async Task<IActionResult> SignUp([FromForm] SignUpRequestOfTraineeDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseDTO(ModelState.ToString() ?? "Unknow error", 400, false, null));
            }

            var checkValid = await _traineeService.CheckValidationTrainee(model);
            if (!checkValid.IsSuccess)
            {
                return BadRequest(checkValid);
            }

            var signUpResult = await _traineeService.AddTraineeNoAccount(model);
            if (signUpResult.IsSuccess)
            {
                return Created("Sucessfully",
                    new ResponseDTO("Thêm học viên vào lớp thành công", 201, true, null));
            }
            else
            {
                return BadRequest(new ResponseDTO("Thêm học viên vào lớp thất bại", 400, true, null));
            }
        }

        [Authorize(Roles = "Lecturer")]
        [HttpGet("search-trainee-add-to-class")]
        public IActionResult SearchTraineeForAssigningToClass(string? searchValue)
        {
            var result = _traineeService.SearchTraineeToAddToClass(searchValue);
            if (result.Count > 0)
            {
                return Ok(new ResponseDTO("Tìm kiếm học viên thành công", 200, true, result));
            }
            else
            {
                return NotFound(new ResponseDTO("Không tìm thấy học viên", 404, false));
            }
        }

        [Authorize(Roles = "Trainee")]
        [HttpPut("trainee-moving-class")]
        public async Task<IActionResult> MovingTraineeToAnotherClass(MoveTraineeClassDTO moveTraineeClassDTO)
        {
            ResponseDTO result = await _traineeService.MoveTraineeClass(moveTraineeClassDTO);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [Authorize(Roles = "Trainee")]
        [HttpGet("available-group-of-class")]
        public async Task<IActionResult> GetAvailableGroupOfClass(Guid currentClassId, Guid accountId)
        {
            ResponseDTO result = await _traineeService.GetAvailableGroupOfClass(currentClassId, accountId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [Authorize(Roles = "Trainee")]
        [HttpPut("trainee-moving-group")]
        public async Task<IActionResult> MoveTraineeToAnotherGroupInClass(MovingTraineeToAnotherGroupInClass traineeToAnotherGroupInClassDTO)
        {
            ResponseDTO result = await _traineeService.MoveTraineeToAnotherGroupInClass(traineeToAnotherGroupInClassDTO);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [Authorize(Roles = "Associate")]
        [HttpPost("import-trainee")]
        public async Task<IActionResult> ImportTrainee(IFormFile file, Guid projectId)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ResponseDTO("File không hợp lệ", 400, false, null));
            }

            ResponseDTO result = await _traineeService.ImportTrainee(file, projectId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [Authorize(Roles = "Lecturer")]
        [HttpPost("export-trainee-template")]
        public async Task<IActionResult> ExportTraineeTemplate(Guid classId)
        {
            var check = await _classService.CheckClassIdExist(classId);
            if (!check)
            {
                return BadRequest(new ResponseDTO("Lớp không tồn tại", 400, false));
            }

            var stream = _traineeService.ExportTraineeListTemplateExcel(classId);
            string fileName = "DanhSachHocVienTemplate.xlsx";

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [Authorize(Roles = "Associate")]
        [HttpPost("export-trainee-class-template")]
        public async Task<IActionResult> ExportTraineeClassTemplate()
        {
            var stream = _traineeService.ExportTraineeClassListTemplateExcel();
            string fileName = "DanhSachLopCuaHocVienTemplate.xlsx";

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
