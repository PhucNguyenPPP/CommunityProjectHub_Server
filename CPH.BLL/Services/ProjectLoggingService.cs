using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Execution;
using CPH.BLL.Interfaces;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Member;
using CPH.Common.DTO.Paging;
using CPH.Common.DTO.ProjectLogging;
using CPH.DAL.Entities;
using CPH.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CPH.BLL.Services
{
    public class ProjectLoggingService : IProjectLoggingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ProjectLoggingService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseDTO> GetAllProjectLogging(Guid projectId, string? searchValue, int? pageNumber, int? rowsPerPage)
        {
            try
            {
                var project = _unitOfWork.Project.GetAllByCondition(c => c.ProjectId == projectId).FirstOrDefault();
                if (project == null)
                {
                    return new ResponseDTO("Dự án không tồn tại", 400, false);
                }

                IQueryable<ProjectLogging> loggings = _unitOfWork.ProjectLogging
                    .GetAllByCondition(c => c.ProjectId == projectId)
                    .Include(c => c.Account).ThenInclude(c => c.Role)
                    .OrderByDescending(c=> c.ActionDate);

                if (searchValue.IsNullOrEmpty() && pageNumber == null && rowsPerPage == null)
                {
                    var listDTO = _mapper.Map<List<GetAllProjectLoggingDTO>>(loggings);
                    return new ResponseDTO("Lấy danh sách nhật ký dự án thành công", 200, true, listDTO);
                }
                else
                {
                    if (!searchValue.IsNullOrEmpty())
                    {
                        loggings = loggings.Where(c =>
                            c.Account.FullName.ToLower().Contains(searchValue.ToLower()) ||
                            c.Account.AccountName.ToLower().Contains(searchValue.ToLower()) ||
                            c.Account.AccountCode.ToLower().Contains(searchValue.ToLower())
                        );
                    }

                    if (!loggings.Any())
                    {
                        return new ResponseDTO("Không có nhật ký dự án trùng khớp", 400, false);
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

                    var listDTO = _mapper.Map<List<GetAllProjectLoggingDTO>>(loggings);

                    if (pageNumber != null && rowsPerPage != null)
                    {
                        var pagedList = PagedList<GetAllProjectLoggingDTO>.ToPagedList(listDTO.AsQueryable(), pageNumber, rowsPerPage);
                        var result = new ListProjectLoggings
                        {
                            getAllProjectLoggingDTOs = pagedList,
                            CurrentPage = pageNumber,
                            RowsPerPages = rowsPerPage,
                            TotalCount = listDTO.Count,
                            TotalPages = (int)Math.Ceiling(listDTO.Count / (double)rowsPerPage)
                        };
                        return new ResponseDTO("Tìm kiếm nhật ký dự án thành công", 200, true, result);
                    }
                    return new ResponseDTO("Tìm kiếm nhật ký dự án thành công", 200, true, listDTO);
                }
            }
            catch (Exception ex) 
            {
                return new ResponseDTO("Tìm kiếm nhật ký dự án thất bại", 500, false);
            }
        }
    }
}
