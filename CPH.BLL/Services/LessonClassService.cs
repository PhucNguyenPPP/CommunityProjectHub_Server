using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CPH.BLL.Interfaces;
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
                var c = _unitOfWork.Class.GetByCondition(c => c.ClassId == classId);
                if (c == null)
                {
                    return new ResponseDTO("Lớp không tồn tại", 404, false);
                }
                var lesson = _unitOfWork.LessonClass
                    .GetAllByCondition(c => c.ClassId == classId)
                    .Include(l => l.Lesson).ToList();
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
    }
}
