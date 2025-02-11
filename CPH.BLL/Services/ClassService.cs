using AutoMapper;
using CPH.BLL.Interfaces;
using CPH.Common.DTO.Account;
using CPH.Common.DTO.Class;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Message;
using CPH.Common.DTO.Paging;
using CPH.DAL.Entities;
using CPH.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.BLL.Services
{
    public class ClassService : IClassService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ClassService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<bool> CheckClassIdExist(Guid classId)
        {
            var objClass = await _unitOfWork.Class.GetByCondition(c => c.ClassId == classId);
            if (objClass == null)
            {
                return false;
            }
            return true;
        }

        public async Task<ResponseDTO> GetAllClassOfProject(Guid projectId, string? searchValue, int? pageNumber, int? rowsPerPage)
        {
            IQueryable<Class> list = _unitOfWork.Class.GetAllByCondition(c => c.ProjectId == projectId).Include(c => c.Lecturer);
            if (!searchValue.IsNullOrEmpty())
            {
                list = list.Where(c =>
                    c.ClassCode.ToLower().Contains(searchValue.ToLower())
                );
            }
            if (!list.Any())
            {
                return new ResponseDTO("Không có lớp của dự án", 400, false);
            }

            if (pageNumber == null && rowsPerPage != null)
            {
                return new ResponseDTO("Vui lòng chọn số trang", 400, false);
            }
            if (pageNumber != null && rowsPerPage == null)
            {
                return new ResponseDTO("Vui lòng chọn số dòng mỗi trang", 400, false);
            }
            if (pageNumber <= 0 || rowsPerPage <= 0)
            {
                return new ResponseDTO("Giá trị phân trang không hợp lệ", 400, false);
            }

            int lecturerSlotAvailable = list.Count(c => c.LecturerId == null);

            int perGroup = _unitOfWork.Project.GetAllByCondition(c => c.ProjectId == projectId).Select(c => c.NumberTraineeEachGroup).FirstOrDefault();
            var traineeCounts = list.Select(c => c.Trainees).ToList();

            var mapList = list.ToList().Select(Class =>
            {
                int totalTrainees = Class.Trainees.Count();
                int groupRequiredPerClass = (int)Math.Ceiling((double)totalTrainees / perGroup);

                int groupWithStudentPerClass = Class.Members
                    .Where(m => m.ClassId != null)
                    .Select(m => m.GroupSupportNo)
                    .Distinct()
                    .Count();

                int studentSlotAvailable = Math.Max(groupRequiredPerClass - groupWithStudentPerClass, 0);
                lecturerSlotAvailable = (Class.LecturerId == null) ? 1 : 0;

                var dto = _mapper.Map<GetAllClassOfProjectDTO>(Class);
                dto.LecturerSlotAvailable = lecturerSlotAvailable;
                dto.StudentSlotAvailable = studentSlotAvailable;

                return dto;
            }).ToList();

            if (pageNumber != null && rowsPerPage != null)
            {
                var pagedList = PagedList<GetAllClassOfProjectDTO>.ToPagedList(mapList.AsQueryable(), pageNumber, rowsPerPage);
                var result = new ListClassDTO
                {
                    GetAllClassOfProjectDTOs = pagedList,
                    CurrentPage = pageNumber,
                    RowsPerPages = rowsPerPage,
                    TotalCount = mapList.Count,
                    TotalPages = (int)Math.Ceiling(mapList.Count / (double)rowsPerPage)
                };
                return new ResponseDTO("Lấy các lớp của dự án thành công", 200, true, result);
            }
            return new ResponseDTO("lấy các lớp của dự án thất bại", 500, false, mapList);
        }
    }
}
