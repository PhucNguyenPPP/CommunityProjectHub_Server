using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CPH.BLL.Interfaces;
using CPH.Common.Constant;
using CPH.Common.DTO.General;
using CPH.Common.DTO.LessonClass;
using CPH.Common.DTO.Project;
using CPH.DAL.Entities;
using CPH.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace CPH.BLL.Services
{
    public class LessonClassService : ILessonClassService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;


        public LessonClassService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseDTO> GetLessonClass(Guid classId)
        {
            try
            {
                var c = await _unitOfWork.Class.GetByCondition(c => c.ClassId == classId);
                if (c == null)
                {
                    return new ResponseDTO("Lớp không tồn tại", 404, false);
                }
                var lesson = _unitOfWork.LessonClass
                    .GetAllByCondition(c => c.ClassId == classId)
                    .Include(l => l.Lesson)
                    .ToList()
                    .OrderBy(c=> c.Lesson.LessonNo);
                
                if (lesson == null)
                {
                    return new ResponseDTO("Không tìm thấy bài học tương ứng", 400, false);
                }

                var lessonClassDTO = _mapper.Map<List<GetAllLessonClassByClassDTO>>(lesson);
                return new ResponseDTO("Lấy thông tin dự án thành công", 200, true, lessonClassDTO);
            }
            catch (Exception ex)
            {
                return new ResponseDTO(ex.Message.ToString(),500, false);
            }
        }

        //Ánh ánh xạ cột lesson no
        private async Task<Dictionary<Guid, int>> GetLessonNosAsync(List<Guid> lessonClassIds)
        {
            // Lấy danh sách LessonClass theo lessonClassId
            var lessonClassData = _unitOfWork.LessonClass
                .GetAllByCondition(c => lessonClassIds.Contains(c.LessonClassId))
                .Select(c => new { c.LessonClassId, c.LessonId })
                .ToList();

            // Lấy danh sách LessonId và LessonNo
            var lessonIds = lessonClassData.Select(lc => lc.LessonId).ToList();
            var lessonData = _unitOfWork.Lesson
                .GetAllByCondition(l => lessonIds.Contains(l.LessonId))
                .Select(l => new { l.LessonId, l.LessonNo })
                .ToList();

            // Ghép LessonNo vào LessonClassId
            var lessonNoDict = lessonClassData.ToDictionary(
                lc => lc.LessonClassId,
                lc => lessonData.FirstOrDefault(l => l.LessonId == lc.LessonId)?.LessonNo ?? -1
            );

            return lessonNoDict;
        }

        public async Task<ResponseDTO> CheckValidationUpdateLessonClass(Guid projectId, List<UpdateLessonClassDTO> updateLessonClassDTOs)
        {
            var project = _unitOfWork.Project.GetAllByCondition(c => c.ProjectId == projectId).FirstOrDefault();
            if (project == null)
            {
                return new ResponseDTO("Dự án không tồn tại", 400, false);
            }
            else if(project.Status != ProjectStatusConstant.Planning)
            {
                return new ResponseDTO("Trạng thái dự án không khả dụng để chỉnh sửa", 400, false);
            }

            int totalProjectLesson = _unitOfWork.Lesson.GetAllByCondition(c => c.ProjectId == projectId).Count();
            if (updateLessonClassDTOs.Count() < totalProjectLesson || updateLessonClassDTOs.Count() > totalProjectLesson)
            {
                return new ResponseDTO("Số lượng bài giảng của lớp không hợp lệ!", 400, false);
            }

            var startProject = _unitOfWork.Project
                .GetAllByCondition(c => c.ProjectId == projectId)
                .Select(c => c.StartDate)
                .FirstOrDefault();

            var endProject = _unitOfWork.Project
                .GetAllByCondition(c => c.ProjectId == projectId)
                .Select(c => c.EndDate)
                .FirstOrDefault();

            var allLessonClasses = _unitOfWork.LessonClass.GetAll().ToList();


            var lessonClassIds = updateLessonClassDTOs.Select(lc => lc.LessonClassId).ToList();

            var lessonNoDict = await GetLessonNosAsync(lessonClassIds);

            var lessonClasses = updateLessonClassDTOs
                .Select(lc => new
                {
                    LessonClassDTO = lc,
                    LessonNo = lessonNoDict.ContainsKey(lc.LessonClassId) ? lessonNoDict[lc.LessonClassId] : -1
                })
                .OrderBy(lc => lc.LessonNo)
                .ToList();

            List<Guid> classIds = new List<Guid>();
            List<DateTime> startTime = new List<DateTime>();
            List<DateTime> endTime = new List<DateTime>();
            int minTimeLesson = project.MinLessonTime;
            int maxTimeLesson = project.MaxLessonTime;

            for (int i = 0; i < lessonClasses.Count(); i++)
            {
                var lessonClass = _unitOfWork.LessonClass
                    .GetAllByCondition(c => c.LessonClassId == lessonClasses[i].LessonClassDTO.LessonClassId)
                    .FirstOrDefault();

                if (lessonClass == null)
                {
                    return new ResponseDTO($"Bài giảng thứ {i + 1} không tồn tại", 400, false);
                }

                var currentStart = lessonClasses[i].LessonClassDTO.StartTime;
                var currentEnd = lessonClasses[i].LessonClassDTO.EndTime;

                if (lessonClasses[i].LessonClassDTO.StartTime < startProject || lessonClasses[i].LessonClassDTO.StartTime > endProject)
                {
                    return new ResponseDTO($"Bài giảng thứ {i + 1}: Thời gian bắt đầu của buổi học phải nằm trong thời gian diễn ra dự án", 400, false);
                }

                if (lessonClasses[i].LessonClassDTO.EndTime < startProject || lessonClasses[i].LessonClassDTO.EndTime > endProject)
                {
                    return new ResponseDTO($"Bài giảng thứ {i + 1}: Thời gian kết thúc buổi học phải nằm trong thời gian diễn ra dự án", 400, false);
                }

                if (lessonClasses[i].LessonClassDTO.StartTime >= lessonClasses[i].LessonClassDTO.EndTime)
                {
                    return new ResponseDTO($"Bài giảng thứ {i + 1}: Thời gian bắt đầu phải sớm hơn thời gian kết thúc!", 400, false);
                }

                TimeSpan duration = lessonClasses[i].LessonClassDTO.EndTime - lessonClasses[i].LessonClassDTO.StartTime;
                int totalMinutes = (int)duration.TotalMinutes;
                if (totalMinutes < minTimeLesson || totalMinutes > maxTimeLesson)
                {
                    return new ResponseDTO($"Bài giảng thứ {i + 1}: Thời lượng buổi học phải nằm trong khoảng từ {minTimeLesson} đến {maxTimeLesson} phút. Hiện tại: {totalMinutes} phút.", 400, false);
                }

                if (i > 0)
                {
                    var previousEnd = lessonClasses[i - 1].LessonClassDTO.EndTime;
                    if (currentStart <= previousEnd)
                    {
                        return new ResponseDTO($"Bài giảng {lessonClasses[i].LessonNo} phải bắt đầu sau khi bài {lessonClasses[i - 1].LessonNo} kết thúc!", 400, false);
                    }
                }

                foreach (var existingLesson in allLessonClasses)
                {
                    if (existingLesson.LessonClassId != lessonClasses[i].LessonClassDTO.LessonClassId &&
                        existingLesson.Room == lessonClasses[i].LessonClassDTO.Room &&
                        (
                            (lessonClasses[i].LessonClassDTO.StartTime >= existingLesson.StartTime && lessonClasses[i].LessonClassDTO.StartTime < existingLesson.EndTime) || // Bắt đầu trong khoảng thời gian đã có
                            (lessonClasses[i].LessonClassDTO.EndTime > existingLesson.StartTime && lessonClasses[i].LessonClassDTO.EndTime <= existingLesson.EndTime) || // Kết thúc trong khoảng thời gian đã có
                            (lessonClasses[i].LessonClassDTO.StartTime <= existingLesson.StartTime && lessonClasses[i].LessonClassDTO.EndTime >= existingLesson.EndTime) // Chứa toàn bộ khoảng thời gian đã có
                        ))
                    {
                        return new ResponseDTO($"Phòng {lessonClasses[i].LessonClassDTO.Room} đã được đặt từ {existingLesson.StartTime} đến {existingLesson.EndTime}. Vui lòng chọn thời gian khác!", 400, false);
                    }
                }

                classIds.Add(lessonClass.ClassId);
                startTime.Add(lessonClasses[i].LessonClassDTO.StartTime);
                endTime.Add(lessonClasses[i].LessonClassDTO.EndTime);
            }

            bool check = classIds.Distinct().Count() == 1;
            if (!check)
            {
                return new ResponseDTO("Tất cả các bài giảng phải thuộc cùng một lớp", 400, false);
            }

            bool checkStart = startTime.Distinct().Count() == lessonClasses.Count() ;
            bool checkEnd = endTime.Distinct().Count() == lessonClasses.Count(); 
            if (!checkStart && !checkEnd)
            {
                return new ResponseDTO("Thời gian của các buổi học phải khác nhau", 400, false);
            }
            return new ResponseDTO("Check thành công", 200, true);
        }

        public async Task<ResponseDTO> UpdateLessonClass(Guid projectId, List<UpdateLessonClassDTO> updateLessonClassDTOs)
        {
            foreach (var dto in updateLessonClassDTOs)
            {
                var lessonClass = _unitOfWork.LessonClass.GetAllByCondition(c => c.LessonClassId == dto.LessonClassId).FirstOrDefault();
                if (lessonClass != null)
                {
                    // Cập nhật thông tin từ DTO
                    lessonClass.Room = dto.Room;
                    lessonClass.StartTime = dto.StartTime;
                    lessonClass.EndTime = dto.EndTime;

                    _unitOfWork.LessonClass.Update(lessonClass);
                }
            }
            await _unitOfWork.SaveChangeAsync();
            return new ResponseDTO("Cập nhật thành công", 200, true);
        }
    }
}
