using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CPH.BLL.Interfaces;
using CPH.Common.Constant;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Paging;
using CPH.Common.DTO.Project;
using CPH.DAL.Entities;
using CPH.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CPH.BLL.Services
{
    public class ProjectService :  IProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ProjectService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseDTO> GetAllProject(string? searchValue, int? pageNumber, int? rowsPerPage, string? filterField, string? filterOrder)
        {
            try
            {
                IQueryable<Project> list = _unitOfWork.Project
                    .GetAllByCondition(c=> c.Status == true)
                    .Include(c=> c.Classes).ThenInclude(c=> c.Lecturer)
                    .Include(c=> c.ProjectManager);
                if (searchValue.IsNullOrEmpty() && pageNumber == null && rowsPerPage == null && filterField.IsNullOrEmpty() && filterOrder.IsNullOrEmpty())
                {
                    var listDTO = _mapper.Map<List<GetAllProjectDTO>>(list);
                    return new ResponseDTO("Lấy thông tin dự án cộng đồng thành công", 200, true, listDTO);
                }
                else
                {
                    if (!searchValue.IsNullOrEmpty())
                    {
                        list = list.Where(c =>
                            c.Title.ToLower().Contains(searchValue.ToLower()) ||
                            c.ProjectManager.FullName.ToLower().Contains(searchValue.ToLower()) ||
                            (c.Classes != null && c.Classes.Any(cl => cl.Lecturer.FullName.ToLower().Contains(searchValue.ToLower())))
                        );
                    }

                    if (filterField.IsNullOrEmpty() && !filterOrder.IsNullOrEmpty())
                    {
                        return new ResponseDTO("Vui lòng chọn trường lọc!", 400, false);
                    }
                    if (!filterField.IsNullOrEmpty() && filterOrder.IsNullOrEmpty())
                    {
                        return new ResponseDTO("Vui lòng chọn thứ tự lọc!", 400, false);
                    }

                    if (!filterField.IsNullOrEmpty() && !filterOrder.IsNullOrEmpty())
                    {
                        if (!filterField.Equals("Title") && !filterField.Equals("StartDate") && !filterField.Equals("EndDate") && !filterField.Equals("CreatedDate"))
                        {
                            return new ResponseDTO("Trường lọc không hợp lệ", 400, false);
                        }

                        if (!filterOrder.Equals(FilterConstant.Ascending) && !filterOrder.Equals(FilterConstant.Descending))
                        {
                            return new ResponseDTO("Thứ tự lọc không hợp lệ", 400, false);
                        }

                        list = ApplySorting(list, filterField, filterOrder);
                    }


                    if (!list.Any())
                    {
                        return new ResponseDTO("Không có dự án trùng khớp", 400, false);
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

                    var listDTO = _mapper.Map<List<GetAllProjectDTO>>(list);
                    if(pageNumber != null && rowsPerPage != null)
                    {
                        var pagedList = PagedList<GetAllProjectDTO>.ToPagedList(listDTO.AsQueryable(), pageNumber, rowsPerPage);
                        var result = new ListProjectDTO
                        {
                            GetAllProjectDTOs = pagedList,
                            CurrentPage = pageNumber,
                            RowsPerPages = rowsPerPage,
                            TotalCount = listDTO.Count,
                            TotalPages = (int)Math.Ceiling(listDTO.Count / (double)rowsPerPage)
                        };
                        return new ResponseDTO("Tìm kiếm dự án thành công", 200, true, result);
                    }
                    return new ResponseDTO("Tìm kiếm dự án thành công", 200, true, listDTO);
                }
            }
            catch (Exception ex)
            {
                return new ResponseDTO("Tìm kiếm dự án thất bại", 500, false);
            }
        }

        private IQueryable<Project> ApplySorting(IQueryable<Project> list, string filterField, string filterOrder)
        {
            return filterField switch
            {
                "Title" => filterOrder == FilterConstant.Descending
                    ? list.OrderByDescending(c => c.Title)
                    : list.OrderBy(c => c.Title),
                "StartDate" => filterOrder == FilterConstant.Descending
                    ? list.OrderByDescending(c => c.StartDate)
                    : list.OrderBy(c => c.StartDate),
                "EndDate" => filterOrder == FilterConstant.Descending
                    ? list.OrderByDescending(c => c.EndDate)
                    : list.OrderBy(c => c.EndDate),
                "CreatedDate" => filterOrder == FilterConstant.Descending
                    ? list.OrderByDescending(c => c.CreatedDate)
                    :list.OrderBy(c => c.CreatedDate)
            };
        }
    }
}
