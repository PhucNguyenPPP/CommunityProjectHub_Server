using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CPH.BLL.Interfaces;
using CPH.Common.DTO.General;
using CPH.Common.DTO.LessonClass;
using CPH.Common.DTO.Member;
using CPH.Common.DTO.Paging;
using CPH.Common.DTO.Project;
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
        public MemberService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
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
            var member = await _unitOfWork.Member.GetByCondition(c => c.MemberId == memberId);
            if (member == null)
            {
                return new ResponseDTO("Sinh viên không tồn tại", 400, false, null);
            }

            _unitOfWork.Member.Delete(member);
            var result = await _unitOfWork.SaveChangeAsync();
            if(result)
            {
                return new ResponseDTO("Xóa sinh viên thành công", 200, true, null);
            }
            return new ResponseDTO("Xóa sinh viên thất bại", 500, false, null);
        }
    }
}
