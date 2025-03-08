using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CPH.BLL.Interfaces;
using CPH.Common.Constant;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Lecturer;
using CPH.Common.DTO.LessonClass;
using CPH.Common.DTO.Member;
using CPH.Common.DTO.Paging;
using CPH.Common.DTO.Project;
using CPH.Common.Enum;
using CPH.Common.Notification;
using CPH.DAL.Entities;
using CPH.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CPH.BLL.Services
{
    public class MemberService : IMemberService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        public MemberService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        public async Task<ResponseDTO> GetAllMemberOfProject(Guid projectId, string? searchValue, int? pageNumber, int? rowsPerPage)
        {
            try
            {
                var project = _unitOfWork.Project.GetAllByCondition(c => c.ProjectId == projectId).FirstOrDefault();
                if (project == null)
                {
                    return new ResponseDTO("Dự án không tồn tại", 400, false);
                }

                IQueryable<Member> member = _unitOfWork.Member
                    .GetAllByCondition(c => c.Class.ProjectId == projectId)
                    .Include(cl => cl.Class)
                    .Include(ac => ac.Account).ThenInclude(r => r.Role);

                if (searchValue.IsNullOrEmpty() && pageNumber == null && rowsPerPage == null)
                {
                    var listDTO = _mapper.Map<List<MemberProjectDTO>>(member);
                    return new ResponseDTO("Lấy danh sách sinh viên hỗ trợ thành công", 200, true, listDTO);

                }
                else
                {
                    if (!searchValue.IsNullOrEmpty())
                    {
                        member = member.Where(c =>
                            c.Account.FullName.ToLower().Contains(searchValue.ToLower()) ||
                            c.Account.Email.ToLower().Contains(searchValue.ToLower()) ||
                            c.Account.AccountCode.ToLower().Contains(searchValue.ToLower()) ||
                            c.Class.ClassCode.ToLower().Contains(searchValue.ToLower())
                        );
                    }

                    if (!member.Any())
                    {
                        return new ResponseDTO("Không có sinh viên hỗ trợ trùng khớp", 400, false);
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

                    var listDTO = _mapper.Map<List<MemberProjectDTO>>(member);
                    if (pageNumber != null && rowsPerPage != null)
                    {
                        var pagedList = PagedList<MemberProjectDTO>.ToPagedList(listDTO.AsQueryable(), pageNumber, rowsPerPage);
                        var result = new ListMemberDTO
                        {
                            MemberProjectDTOs = pagedList,
                            CurrentPage = pageNumber,
                            RowsPerPages = rowsPerPage,
                            TotalCount = listDTO.Count,
                            TotalPages = (int)Math.Ceiling(listDTO.Count / (double)rowsPerPage)
                        };
                        return new ResponseDTO("Tìm kiếm sinh viên hỗ trợ thành công", 200, true, result);
                    }
                    return new ResponseDTO("Tìm kiếm sinh viên hỗ trợ thành công", 200, true, listDTO);
                }
            }
            catch (Exception ex)
            {
                return new ResponseDTO("Tìm kiếm sinh viên hỗ trợ thất bại", 500, false);
            }
        }

        public async Task<ResponseDTO> RemoveMemberFromProject(Guid memberId)
        {
            var member = _unitOfWork.Member.GetAllByCondition(c => c.MemberId == memberId)
                .Include(c => c.Account)
                .Include(c => c.Class)
                .ThenInclude(c => c.Project)
                .FirstOrDefault();

            if (member == null)
            {
                return new ResponseDTO("Sinh viên không tồn tại", 400, false, null);
            }

            if(member.Class.Project.Status != ProjectStatusConstant.UpComing)
            {
                return new ResponseDTO("Không thể xóa sinh viên ở giai đoạn này của dự án", 400, false, null);
            }

            _unitOfWork.Member.Delete(member);

            //Create notification

            var classRemove = _unitOfWork.Class
                .GetAllByCondition(c => c.ClassId == member.ClassId)
                .Include(c => c.Project)
                .FirstOrDefault();

            if (classRemove == null)
            {
                return new ResponseDTO("Không tìm thấy lớp học", 400, false, null);
            }

            var messageNotification = RemoveMemberNotification.SendRemovedNotification(classRemove!.ClassCode, classRemove!.Project.Title);
            var accountId = member.AccountId;
            await _notificationService.CreateNotification(accountId, messageNotification);

            ProjectLogging logging = new ProjectLogging()
            {
                ProjectNoteId = Guid.NewGuid(),
                ActionDate = DateTime.Now,
                ProjectId = member.Class.Project.ProjectId,
                ActionContent = $"{member.Account.FullName} không còn là sinh viên hỗ trợ lớp {member.Class.ClassCode} của dự án {member.Class.Project.Title}",
                AccountId = accountId,
            };

            await _unitOfWork.ProjectLogging.AddAsync(logging);

            var result = await _unitOfWork.SaveChangeAsync();
            if (result)
            {
                return new ResponseDTO("Xóa sinh viên thành công", 200, true, null);
            }
            return new ResponseDTO("Xóa sinh viên thất bại", 500, false, null);
        }

        public List<MemberResponseDTO> SearchStudentForAssigningToClass(string? searchValue)
        {
            if (searchValue.IsNullOrEmpty())
            {
                return new List<MemberResponseDTO>();
            }

            var searchedList = _unitOfWork.Account.GetAllByCondition(c => (c.AccountCode.ToLower().Contains(searchValue!.ToLower())
            || c.FullName.ToLower().Contains(searchValue.ToLower()) || c.Email.ToLower().Contains(searchValue.ToLower())
            || c.Phone.ToLower().Contains(searchValue.ToLower())) && c.RoleId == (int)RoleEnum.Student).ToList();

            var mappedSearchedList = _mapper.Map<List<MemberResponseDTO>>(searchedList);
            return mappedSearchedList;
        }
    }
}
