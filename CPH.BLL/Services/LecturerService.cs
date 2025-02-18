﻿using AutoMapper;
using CPH.BLL.Interfaces;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Lecturer;
using CPH.Common.DTO.Member;
using CPH.Common.DTO.Paging;
using CPH.Common.Enum;
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

        public LecturerService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
    }
}
