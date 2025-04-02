using AutoMapper;
using CPH.BLL.Interfaces;
using CPH.Common.Constant;
using CPH.Common.DTO.Attendance;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Trainee;
using CPH.DAL.Entities;
using CPH.DAL.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.BLL.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AttendanceService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseDTO> ImportAttendanceFile(IFormFile file, Guid classId)
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var classObj = _unitOfWork.Class.GetAllByCondition(c => c.ClassId == classId)
                .Include(c => c.Trainees)
                .Include(c => c.LessonClasses)
                .ThenInclude(c => c.Lesson)
                .Include(c => c.Project)
                .FirstOrDefault();

            if (classObj == null)
            {
                return new ResponseDTO("Lớp không tồn tại", 400, false);
            }

            if (classObj.Project.Status != ProjectStatusConstant.InProgress)
            {
                return new ResponseDTO("Chỉ có thể lưu điểm danh trong khi dự án đang diễn ra", 400, false);
            }

            var totalSlot = classObj.LessonClasses.Count();

            var attendanceTrainee = new List<AttendanceTraineeDTO>();
            var errors = new List<string>();

            int y = 0;
            using (var package = new ExcelPackage(stream))
            {
                foreach (var worksheet in package.Workbook.Worksheets)
                {
                    int rowCount = worksheet.Dimension.Rows;
                    y += 1;
                    for (int row = 2; row <= rowCount; row++)
                    {
                        string? accountCode = worksheet.Cells[row, 2].Text.Trim();
                        string? traineeFullName = worksheet.Cells[row, 3].Text.Trim();
                        string? attendanceStatus = worksheet.Cells[row, 5].Text.Trim();

                        attendanceTrainee.Add(new AttendanceTraineeDTO()
                        {
                            AccountCode = accountCode,
                            TraineeFullName = traineeFullName,
                            SlotNo = y,
                            AttendanceStatus = attendanceStatus
                        });
                    }
                }
            }

            if (y != totalSlot)
            {
                return new ResponseDTO("Số buổi điểm danh không hợp lệ", 400, false);
            }

            for (int i = 0; i < attendanceTrainee.Count; i++)
            {
                if (string.IsNullOrEmpty(attendanceTrainee[i].AccountCode))
                {
                    errors.Add($"Bỏ qua dòng {i + 2}: Mã học viên bị thiếu");
                    continue;
                }

                var account = await _unitOfWork.Account.GetByCondition(c => c.AccountCode == attendanceTrainee[i].AccountCode);
                if (account == null)
                {
                    errors.Add($"Bỏ qua dòng {i + 2}: Học viên có mã {attendanceTrainee[i].AccountCode} không tồn tại");
                    continue;
                }

                var trainee = await _unitOfWork.Trainee.GetByCondition(c => c.AccountId == account.AccountId && c.ClassId == classId);
                if (trainee == null)
                {
                    errors.Add($"Bỏ qua dòng {i + 2}: Học viên có mã {attendanceTrainee[i].AccountCode} không thuộc lớp hiện tại");
                    continue;
                }

                if (attendanceTrainee[i].AttendanceStatus.IsNullOrEmpty()
                    || (attendanceTrainee[i].AttendanceStatus != "Vắng"
                    && attendanceTrainee[i].AttendanceStatus != "Có mặt"))
                {
                    errors.Add($"Bỏ qua dòng {i + 2}: Học viên có mã {attendanceTrainee[i].AccountCode} có điểm danh không hợp lệ (Vắng/Có mặt)");
                    continue;
                }
            }

            if (errors.Count > 0)
            {
                return new ResponseDTO("File Excel không hợp lệ", 400, false, errors);
            }

            var lessonClassList = classObj.LessonClasses.OrderBy(c => c.StartTime).ToList();
            List<Attendance> attendanceTraineeTempList = new List<Attendance>();
            foreach (var i in attendanceTrainee)
            {
                var account = await _unitOfWork.Account.GetByCondition(c => c.AccountCode == i.AccountCode);
                var trainee = await _unitOfWork.Trainee.GetByCondition(c => c.AccountId == account!.AccountId && c.ClassId == classId);
                var lessonClassIndex = i.SlotNo - 1;
                Attendance attendance = new Attendance
                {
                    AttendanceId = Guid.NewGuid(),
                    Status = i.AttendanceStatus == "Có mặt" ? true : false,
                    TraineeId = trainee!.TraineeId,
                    LessonClassId = lessonClassList[lessonClassIndex].LessonClassId,
                };
                var attendanceExist = await _unitOfWork.Attendance.GetByCondition(c => c.LessonClassId == lessonClassList[lessonClassIndex].LessonClassId && c.TraineeId == trainee!.TraineeId);
                if (attendanceExist != null)
                {
                    _unitOfWork.Attendance.Delete(attendanceExist);
                }
                await _unitOfWork.Attendance.AddAsync(attendance);
                attendanceTraineeTempList.Add(attendance);
            }

            List<Guid> traineeIds = attendanceTraineeTempList.Select(a => a.TraineeId).Distinct().ToList();
            var project = classObj.Project; 
            foreach (var traineeId in traineeIds)
            {
                var trainee = await _unitOfWork.Trainee.GetByCondition(c => c.TraineeId == traineeId);
                if (trainee!.Score != null)
                {
                    var totalAbsentSlot = attendanceTraineeTempList.Where(c => c.TraineeId == traineeId && c.Status == false).Count();
                    var absentPercentage = totalAbsentSlot * 100 / totalSlot;
                    if (absentPercentage <= project.MaxAbsentPercentage && trainee!.Score >= project.FailingScore)
                    {
                        trainee!.Result = true;
                    }
                    else
                    {
                        trainee!.Result = false;
                    }
                    _unitOfWork.Trainee.Update(trainee);
                }
            }

            var result = await _unitOfWork.SaveChangeAsync();
            return result
                ? new ResponseDTO("Lưu điểm danh học viên thành công", 200, true)
                : new ResponseDTO("Lưu điểm danh học viên thất bại", 400, false);
        }

        public ResponseDTO GetAttendanceClass(Guid classId)
        {
            var classObj = _unitOfWork.Class.GetAllByCondition(c => c.ClassId == classId)
                .Include(c => c.Trainees)
                .Include(c => c.LessonClasses)
                .FirstOrDefault();

            if (classObj == null)
            {
                return new ResponseDTO("Lớp không tồn tại", 400, false);
            }

            var traineeList = _unitOfWork.Trainee.GetAllByCondition(c => c.ClassId == classId)
               .Include(c => c.Account)
               .ThenInclude(c => c.Role)
               .Include(c => c.Class)
               .ThenInclude(c => c.LessonClasses)
               .Include(c => c.Attendances);

            var totalLesson = classObj.LessonClasses.Count();

            var listDTO = _mapper.Map<List<AttendanceTraineeResponseDTO>>(traineeList);

            foreach (var trainee in listDTO)
            {
                var totalPresentLesson = _unitOfWork.Attendance.GetAllByCondition(c => c.TraineeId == trainee.TraineeId && c.Status == true).Count();
                trainee.TotalLesson = totalLesson;
                trainee.TotalPresentLesson = totalPresentLesson;
            }
            return new ResponseDTO("Lấy danh sách điểm của học viên thành công", 200, true, listDTO);
        }
    }
}
