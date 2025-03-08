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
using CPH.Common.DTO.Trainee;
using CPH.DAL.Entities;
using CPH.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CPH.BLL.Services
{
    public class TraineeService : ITraineeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TraineeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDTO> GetAllTraineeOfClass(Guid classId, string? searchValue, int? pageNumber, int? rowsPerPage, string? filterField, string? filterOrder)
        {
            try
            {
                var classes = _unitOfWork.Class.GetAllByCondition(c => c.ClassId == classId);
                if (!classes.Any())
                {
                    return new ResponseDTO("Lớp không tồn tại", 400, false);
                }

                IQueryable<Trainee> list = _unitOfWork.Trainee
                    .GetAllByCondition(c => c.ClassId == classId)
                    .Include(c => c.Account).ThenInclude(c => c.Role);

                if (searchValue.IsNullOrEmpty() && pageNumber == null && rowsPerPage == null && filterField.IsNullOrEmpty() && filterOrder.IsNullOrEmpty())
                {
                    var listDTO = _mapper.Map<List<GetAllTraineeOfClassDTO>>(list);
                    return new ResponseDTO("Lấy thông tin học viên thành công", 200, true, listDTO);
                }
                else
                {
                    if (!searchValue.IsNullOrEmpty())
                    {
                        list = list.Where(c =>
                            c.Account.FullName.ToLower().Contains(searchValue.ToLower()) ||
                            c.Account.AccountCode.ToLower().Contains(searchValue.ToLower()) ||
                            c.Account.AccountName.ToLower().Contains(searchValue.ToLower()) ||
                            c.Account.Email.ToLower().Contains(searchValue.ToLower()) ||
                            c.Account.Phone.ToLower().Contains(searchValue.ToLower())
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
                        if (!filterField.Equals("fullName") && !filterField.Equals("accountCode") && !filterField.Equals("email"))
                        {
                            return new ResponseDTO("Trường lọc không hợp lệ", 400, false);
                        }

                        if (!filterOrder.Equals(FilterConstant.Ascending) && !filterOrder.Equals(FilterConstant.Descending))
                        {
                            return new ResponseDTO("Thứ tự lọc không hợp lệ", 400, false);
                        }

                        list = ApplySorting(list, filterField, filterOrder);
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

                    var listDTO = _mapper.Map<List<GetAllTraineeOfClassDTO>>(list);
                    if (pageNumber != null && rowsPerPage != null)
                    {
                        var pagedList = PagedList<GetAllTraineeOfClassDTO>.ToPagedList(listDTO.AsQueryable(), pageNumber, rowsPerPage);
                        var result = new ListTraineeDTO
                        {
                            GetAllTraineeOfClassDTOs = pagedList,
                            CurrentPage = pageNumber,
                            RowsPerPages = rowsPerPage,
                            TotalCount = listDTO.Count,
                            TotalPages = (int)Math.Ceiling(listDTO.Count / (double)rowsPerPage)
                        };
                        return new ResponseDTO("Tìm kiếm học viên thành công", 200, true, result);
                    }
                    return new ResponseDTO("Tìm kiếm học viên thành công", 200, true, listDTO);
                }
            }
            catch (Exception ex)
            {
                return new ResponseDTO("Tìm kiếm học viên thất bại", 500, false);
            }
        }

        private IQueryable<Trainee> ApplySorting(IQueryable<Trainee> list, string filterField, string filterOrder)
        {
            return filterField switch
            {
                "fullName" => filterOrder == FilterConstant.Descending
                    ? list.OrderByDescending(c => c.Account.FullName)
                    : list.OrderBy(c => c.Account.FullName),
                "accountCode" => filterOrder == FilterConstant.Descending
                    ? list.OrderByDescending(c => c.Account.AccountCode)
                    : list.OrderBy(c => c.Account.AccountCode),
                "email" => filterOrder == FilterConstant.Descending
                    ? list.OrderByDescending(c => c.Account.Email)
                    : list.OrderBy(c => c.Account.Email)
            };
        }

        public async Task<ResponseDTO> UpdateScoreTrainee(ScoreTraineeRequestDTO model)
        {
            var classObj = _unitOfWork.Class.GetAllByCondition(c => c.ClassId == model.ClassId)
                .Include(c => c.Trainees)
                .ThenInclude(c => c.Account)
                .Include(c => c.Project)
                .FirstOrDefault();

            if (classObj == null)
            {
                return new ResponseDTO("Lớp không tồn tại", 400, false);
            }

            if(classObj.Project.Status != ProjectStatusConstant.InProgress)
            {
                return new ResponseDTO("Không thể cập nhật điểm ở giai đoạn này của dự án", 400, false);
            }

            var traineeClassList = classObj.Trainees.ToList();
            List<string> errors = new List<string>();
            foreach (var i in model.ScoreTrainees)
            {
                var trainee = traineeClassList.FirstOrDefault(c => c.TraineeId == i.TraineeId);
                if (trainee == null)
                {
                    return new ResponseDTO($"Danh sách có chứa học viên không tồn tại trong lớp", 400, false);
                }

                if (i.Score != null && (i.Score < 0 || i.Score > 10))
                {
                    errors.Add($"Điểm số của học viên {trainee.Account.AccountName} (Mã học viên: {trainee.Account.AccountCode}) không hợp lệ");
                }
            }

            if (errors.Count > 0)
            {
                return new ResponseDTO("Có lỗi xảy ra", 400, false, errors);
            }

            bool checkUpdate = false;
            foreach (var i in model.ScoreTrainees)
            {
                var trainee = traineeClassList.FirstOrDefault(c => c.TraineeId == i.TraineeId);
                if (trainee!.Score != i.Score)
                {
                    checkUpdate = true;
                }

                if(i.Score != null)
                {
                    trainee!.Score = Math.Round((decimal)i.Score, 2);
                } else
                {
                    trainee!.Score = null;
                }

                _unitOfWork.Trainee.Update(trainee);
            }

            if (checkUpdate)
            {
                var result = await _unitOfWork.SaveChangeAsync();
                if (result)
                {
                    return new ResponseDTO("Chỉnh sửa điểm số thành công", 200, true);
                }
                return new ResponseDTO("Chỉnh sửa điểm số thất bại", 400, false);
            }
            return new ResponseDTO("Chỉnh sửa điểm số thành công", 200, true);
        }

        public async Task<ResponseDTO> GetScoreTraineeList(Guid classId)
        {
            var classObj = await _unitOfWork.Class.GetByCondition(c => c.ClassId == classId);

            if (classObj == null)
            {
                return new ResponseDTO("Lớp không tồn tại", 400, false);
            }

            var traineeList = _unitOfWork.Trainee.GetAllByCondition(c => c.ClassId == classId)
                .Include(c => c.Account)
                .ThenInclude(c => c.Role);

            var listDTO = _mapper.Map<List<GetAllTraineeOfClassDTO>>(traineeList);
            return new ResponseDTO("Lấy danh sách điểm của học viên thành công", 200, true, listDTO);
        }
    }
}
