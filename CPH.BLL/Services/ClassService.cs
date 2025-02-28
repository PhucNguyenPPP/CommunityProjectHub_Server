using AutoMapper;
using CPH.BLL.Interfaces;
using CPH.Common.Constant;
using CPH.Common.DTO.Account;
using CPH.Common.DTO.Class;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Member;
using CPH.Common.DTO.Message;
using CPH.Common.DTO.Paging;
using CPH.DAL.Entities;
using CPH.DAL.UnitOfWork;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public async Task<ResponseDTO> DivideGroupOfClass(DevideGroupOfClassDTO devideGroupOfClassDTO)
        {
            var existed  = await CheckClassIdExist(devideGroupOfClassDTO.ClassId);  
            if(!existed)
            {
                return new ResponseDTO("Lớp không tồn tại", 400, false);
            }
            var check = await CheckDivision(devideGroupOfClassDTO);
            if (!check.IsSuccess)
            {
                return new ResponseDTO(check.Message.ToString(), 400, false);
            }
            if(check.Result==null)
            {
                return new ResponseDTO("Lớp học lỗi", 400, false);
            }    
            Class temp = (Class) check.Result;  
            temp.NumberGroup = devideGroupOfClassDTO.NumberGroup;   
            _unitOfWork.Class.Update(temp);
            var traineesOfClass = _unitOfWork.Trainee.GetAllByCondition(t => t.ClassId.Equals(devideGroupOfClassDTO.ClassId));
            int i = 1;
            foreach (var trainee in traineesOfClass)
            {
                if (i <= devideGroupOfClassDTO.NumberGroup)
                {
                    trainee.GroupNo = i;
                    i++;
                }
                else
                {
                    i = 1;
                    trainee.GroupNo=i;
                }
            }
            _unitOfWork.Trainee.UpdateRange(traineesOfClass.ToList());
            var saved = await _unitOfWork.SaveChangeAsync();
            if (saved)
            {
                return new ResponseDTO("Chia nhóm cho lớp thành công",200,true);
            }
            return new ResponseDTO("Chia nhóm cho lớp thất bại", 500, false);
        }

        private async Task<ResponseDTO> CheckDivision(DevideGroupOfClassDTO devideGroupOfClassDTO)
        {
            var numbTrainOfClass = _unitOfWork.Trainee.GetAllByCondition(t => t.ClassId.Equals(devideGroupOfClassDTO.ClassId)).Count();
            if (numbTrainOfClass <= 0)
            {
                return new ResponseDTO("Số lượng học viên bị lỗi", 400, false);
            }
            if (devideGroupOfClassDTO.NumberGroup > numbTrainOfClass)
            {
                return new ResponseDTO("Số lượng nhóm không được lớn hơn số lượng học viên của lớp", 400, false);
            }
            var c = await _unitOfWork.Class.GetByCondition(c=>c.ClassId.Equals(devideGroupOfClassDTO.ClassId));
            var pro = await _unitOfWork.Project.GetByCondition(p => p.ProjectId.Equals(c.ProjectId));
            if (pro == null)
            {
                return new ResponseDTO("Lớp thuộc dự án bị lỗi", 400, false);
            }
            if(!pro.Status.Equals(ProjectStatusConstant.UpComing))
            {
                return new ResponseDTO("Dự án đã ở trạng thâi " +pro.Status.ToString(), 400, false);
            }
            return new ResponseDTO("Thông tin chia nhóm của lớp hợp lệ", 200, true, c);
        }

        public async Task<ResponseDTO> GetAllClassOfProject(Guid projectId, string? searchValue, int? pageNumber, int? rowsPerPage)
        {
            IQueryable<Class> list = _unitOfWork.Class.GetAllByCondition(c => c.ProjectId == projectId).Include(c => c.Lecturer).Include(c=> c.Members).Include(c=> c.Trainees);
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

            int lecturerSlotAvailable = 0;

            var mapList = list.ToList().Select(Class =>
            {
                int? groupRequiredPerClass = Class.NumberGroup;

                int groupWithStudent = Class.Members?.Count() ?? 0;

                int? studentSlotAvailable = groupRequiredPerClass - groupWithStudent;
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
            return new ResponseDTO("lấy các lớp của dự án thành công", 200, true, mapList);
            return new ResponseDTO("lấy các lớp của dự án thành công", 200, true);
        }

        public async Task<ResponseDTO> GetClassDetail(Guid classId)
        {
            //var clas = _unitOfWork.Class.GetAllByCondition(c => c.ClassId == classId)
            //    .Include(c => c.Trainees)
            //    .Include(c => c.Members)
            //    .Include(c => c.Project)
            //    .FirstOrDefault();
            //if (clas == null)
            //{
            //    return new ResponseDTO("Lớp không tồn tại", 400, false);
            //}

            //int lecturerSlotAvailable = 0;

            //if (clas.LecturerId == null)
            //{
            //    lecturerSlotAvailable = 1;
            //}

            //var projectId = _unitOfWork.Class.GetAllByCondition(c => c.ClassId == classId).Select(c => c.ProjectId).FirstOrDefault();
            //int perGroup = _unitOfWork.Project.GetAllByCondition(c => c.ProjectId == projectId).Select(c => c.NumberTraineeEachGroup).FirstOrDefault();
            //int totalTrainees = clas.Trainees.Count();

            //int groupRequiredPerClass = (int)Math.Ceiling((double)totalTrainees / perGroup);

            //int groupWithStudentPerClass = clas.Members
            //        .Where(m => m.ClassId != null)
            //        .Select(m => m.GroupSupportNo)
            //        .Distinct()
            //        .Count();

            //int studentSlotAvailable = Math.Max(groupRequiredPerClass - groupWithStudentPerClass, 0);

            //var member = _unitOfWork.Member
            //    .GetAllByCondition(c => c.ClassId == classId)
            //    .Select(c => c.Account)
            //    .ToList();

            //var memberDto = _mapper.Map<List<GetMemberOfClassDTO>>(member);

            //var dto = _mapper.Map<ClassDetailDTO>(clas);
            //dto.LecturerSlotAvailable = lecturerSlotAvailable;
            //dto.StudentSlotAvailable = studentSlotAvailable;
            //dto.getMemberOfClassDTOs = memberDto;

            //return new ResponseDTO("Lấy thông tin chi tiết của lớp thành công", 200, true, dto); 
            return new ResponseDTO("Lấy thông tin chi tiết của lớp thành công", 200, true); 

        }
    }
}
