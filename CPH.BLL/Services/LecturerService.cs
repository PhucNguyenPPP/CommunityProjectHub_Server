using AutoMapper;
using AutoMapper.Execution;
using CPH.BLL.Interfaces;
using CPH.Common.Constant;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Lecturer;
using CPH.Common.DTO.Member;
using CPH.Common.DTO.Paging;
using CPH.Common.Enum;
using CPH.Common.Notification;
using CPH.DAL.Entities;
using CPH.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.BLL.Services
{
    public class LecturerService : ILecturerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public LecturerService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
        }
        public List<LecturerResponseDTO> SearchLecturer(string? searchValue)
        {
            if (searchValue.IsNullOrEmpty())
            {
                return new List<LecturerResponseDTO>();
            }

            var searchedList = _unitOfWork.Account.GetAllByCondition(c => (c.AccountCode.ToLower().Contains(searchValue.ToLower())
            || c.FullName.ToLower().Contains(searchValue.ToLower()) || c.Email.ToLower().Contains(searchValue.ToLower())
            || c.Phone.ToLower().Contains(searchValue.ToLower())) && c.RoleId == (int)RoleEnum.Lecturer);

            var mappedSearchedList = _mapper.Map<List<LecturerResponseDTO>>(searchedList);
            return mappedSearchedList;
        }

        public async Task<ResponseDTO> GetAllLecturerOfProject(Guid projectId, string? searchValue, int? pageNumber, int? rowsPerPage)
        {
            try
            {
                var project = _unitOfWork.Project.GetAllByCondition(c => c.ProjectId == projectId).FirstOrDefault();
                if (project == null)
                {
                    return new ResponseDTO("Dự án không tồn tại", 400, false);
                }

                IQueryable<Class> member = _unitOfWork.Class
                    .GetAllByCondition(c => c.ProjectId == projectId && c.LecturerId != null)
                    .Include(ac => ac.Lecturer).ThenInclude(r => r.Role);

                if (searchValue.IsNullOrEmpty() && pageNumber == null && rowsPerPage == null)
                {
                    var listDTO = _mapper.Map<List<LecturerProjectDTO>>(member);
                    return new ResponseDTO("Lấy danh sách giảng viên thành công", 200, true, listDTO);

                }
                else
                {
                    if (!searchValue.IsNullOrEmpty())
                    {
                        member = member.Where(c =>
                            c.Lecturer.FullName.ToLower().Contains(searchValue.ToLower()) ||
                            c.Lecturer.Email.ToLower().Contains(searchValue.ToLower()) ||
                            c.Lecturer.AccountCode.ToLower().Contains(searchValue.ToLower()) ||
                            c.ClassCode.ToLower().Contains(searchValue.ToLower())
                        );
                    }

                    if (!member.Any())
                    {
                        return new ResponseDTO("Không có giảng viên trùng khớp", 400, false);
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

                    var listDTO = _mapper.Map<List<LecturerProjectDTO>>(member);
                    if (pageNumber != null && rowsPerPage != null)
                    {
                        var pagedList = PagedList<LecturerProjectDTO>.ToPagedList(listDTO.AsQueryable(), pageNumber, rowsPerPage);
                        var result = new ListLecturerDTO
                        {
                            LecturerProjectDTOs = pagedList,
                            CurrentPage = pageNumber,
                            RowsPerPages = rowsPerPage,
                            TotalCount = listDTO.Count,
                            TotalPages = (int)Math.Ceiling(listDTO.Count / (double)rowsPerPage)
                        };
                        return new ResponseDTO("Tìm kiếm giảng viên thành công", 200, true, result);
                    }
                    return new ResponseDTO("Tìm kiếm giảng viên thành công", 200, true, listDTO);
                }
            }
            catch (Exception ex)
            {
                return new ResponseDTO("Tìm kiếm giảng viên thất bại", 500, false);
            }
        }

        public async Task<ResponseDTO> RemoveLecturerFromProject(Guid lecturerId, Guid classId)
        {
            var lecturer = await _unitOfWork.Account.GetByCondition(c => c.AccountId == lecturerId);
            if (lecturer == null)
            {
                return new ResponseDTO("Giảng viên không tồn tại", 400, false);
            }

            var c = await _unitOfWork.Class.GetByCondition(c => c.ClassId == classId);
            if (c == null)
            {
                return new ResponseDTO("Lớp không tồn tại", 400, false);
            }

            var lecturerClass = _unitOfWork.Class.GetAllByCondition(c => c.ClassId == classId && c.LecturerId == lecturerId)
                .Include(c => c.Lecturer)
                .Include(c => c.Project)
                .FirstOrDefault();

            if (lecturerClass == null)
            {
                return new ResponseDTO("Giảng viên hiện không đảm nhận lớp này", 400, false);
            }

            if (lecturerClass.Project.Status != ProjectStatusConstant.UpComing)
            {
                return new ResponseDTO("Không thể xóa giảng viên ở giai đoạn này của dự án", 400, false, null);
            }

            _unitOfWork.Class.Update(lecturerClass);

            var messageNotification = RemoveMemberNotification.SendRemovedLecturerNotification(lecturerClass!.ClassCode, lecturerClass.Project.Title);
            await _notificationService.CreateNotification((Guid)lecturerClass.LecturerId!, messageNotification);

            ProjectLogging logging = new ProjectLogging()
            {
                ProjectNoteId = Guid.NewGuid(),
                ActionDate = DateTime.Now,
                ProjectId = lecturerClass.ProjectId,
                ActionContent = $"{lecturerClass.Lecturer!.FullName} không còn là giảng viên của lớp {lecturerClass.ClassCode} của dự án {lecturerClass.Project.Title}",
                AccountId = (Guid)lecturerClass.LecturerId,
            };

            lecturerClass.LecturerId = null;
            await _unitOfWork.ProjectLogging.AddAsync(logging);

            var result = await _unitOfWork.SaveChangeAsync();
            if(result)
            {
                return new ResponseDTO("Xóa giảng viên thành công", 200, true);
            } else
            {
                return new ResponseDTO("Xóa giảng viên thất bại", 500, true);
            }

        }

        public List<LecturerResponseDTO> SearchLecturerForAssigningPM(string? searchValue, Guid projectId)
        {
            if (searchValue.IsNullOrEmpty())
            {
                return new List<LecturerResponseDTO>();
            }

            var project = _unitOfWork.Project.GetAllByCondition(c => c.ProjectId == projectId).FirstOrDefault();
            if (project == null)
            {
                return new List<LecturerResponseDTO>();
            }

            List<Account> searchedList = new List<Account>();
            if (project.ProjectManagerId == null)
            {
                searchedList = _unitOfWork.Account.GetAllByCondition(c => (c.AccountCode.ToLower().Contains(searchValue!.ToLower())
                || c.FullName.ToLower().Contains(searchValue.ToLower()) || c.Email.ToLower().Contains(searchValue.ToLower())
                || c.Phone.ToLower().Contains(searchValue.ToLower())) && c.RoleId == (int)RoleEnum.Lecturer).ToList();

            }
            else
            {
                searchedList = _unitOfWork.Account.GetAllByCondition(c => (c.AccountCode.ToLower().Contains(searchValue!.ToLower())
               || c.FullName.ToLower().Contains(searchValue.ToLower()) || c.Email.ToLower().Contains(searchValue.ToLower())
               || c.Phone.ToLower().Contains(searchValue.ToLower())) && c.RoleId == (int)RoleEnum.Lecturer && c.AccountId != project.ProjectManagerId).ToList();
            }


            var mappedSearchedList = _mapper.Map<List<LecturerResponseDTO>>(searchedList);
            return mappedSearchedList;
        }
    }
}
