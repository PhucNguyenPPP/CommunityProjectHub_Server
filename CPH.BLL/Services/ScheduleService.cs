using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CPH.BLL.Interfaces;
using CPH.Common.DTO.General;
using CPH.Common.DTO.LessonClass;
using CPH.DAL.Entities;
using CPH.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace CPH.BLL.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ScheduleService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDTO> GetSchedule(Guid accountId)
        {
            var account = _unitOfWork.Account.GetAllByCondition(c=> c.AccountId == accountId).FirstOrDefault();
            if (account == null)
            {
                return new ResponseDTO("Người dùng không tồn tại", 400, false);
            }
            if(account.RoleId == 2)
            {
                var classes = _unitOfWork.Class
                .GetAllByCondition(c => c.LecturerId == accountId)
                .Select(c=> c.ClassId)
                .ToList();

                var lesson = _unitOfWork.LessonClass
                .GetAllByCondition(lc => classes.Contains(lc.ClassId) && lc.Class.LecturerId == accountId)
                .Include(c=> c.Lesson)
                .Include(c => c.Class).ThenInclude(c => c.Project)
                .ToList();

                var mappedList = _mapper.Map<List<GetAllLessonClassForScheduleDTO>>(lesson);
                return new ResponseDTO("Lấy thông tin buổi học thành công", 200, true, mappedList);
            }

            if(account.RoleId == 3)
            {
                var classIds = _unitOfWork.Trainee
                    .GetAllByCondition(t => t.AccountId == accountId)
                    .Select(t => t.ClassId)
                    .ToList();


                var lessons = _unitOfWork.LessonClass
                    .GetAllByCondition(lc => classIds.Contains(lc.ClassId))
                    .Include(lc => lc.Lesson)
                    .Include(c => c.Class).ThenInclude(c => c.Project)
                    .ToList();

                var mappedList = _mapper.Map<List<GetAllLessonClassForScheduleDTO>>(lessons);
                return new ResponseDTO("Lấy thông tin buổi học thành công", 200, true, mappedList);
            }

            if(account.RoleId == 1)
            {
                var classIds = _unitOfWork.Member
                    .GetAllByCondition (t => t.AccountId == accountId)
                    .Select(t => t.ClassId)
                    .ToList();

                var lessons = _unitOfWork.LessonClass
                    .GetAllByCondition(c=> classIds.Contains(c.ClassId))
                    .Include(lc => lc.Lesson)
                    .Include(c => c.Class).ThenInclude(c=> c.Project)
                    .ToList();

                var mappedList = _mapper.Map<List<GetAllLessonClassForScheduleDTO>>(lessons);
                return new ResponseDTO("Lấy thông tin buổi học thành công", 200, true, mappedList);
            }


            return new ResponseDTO("Lấy thông tin buổi học thất bại", 400, false);

        }
    }
}
