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
using CPH.Common.Enum;
using CPH.Common.Notification;
using CPH.DAL.Entities;
using CPH.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CPH.BLL.Services
{
    public class TraineeService : ITraineeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public TraineeService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
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

            if (classObj.Project.Status != ProjectStatusConstant.InProgress)
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

                if (i.Score != null)
                {
                    trainee!.Score = Math.Round((decimal)i.Score, 2);
                }
                else
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

        public async Task<ResponseDTO> RemoveTrainee(Guid classId, Guid accountId, string? reason)
        {
            var account = _unitOfWork.Account.GetAllByCondition(c => c.AccountId == accountId).FirstOrDefault();

            if (account == null)
            {
                return new ResponseDTO("Người dùng không tồn tại", 400, false);
            }

            var classRemove = _unitOfWork.Class
                .GetAllByCondition(c => c.ClassId == classId)
                .Include(c => c.Project)
                .FirstOrDefault();

            if (classRemove == null)
            {
                return new ResponseDTO("Không tìm thấy lớp học", 400, false, null);
            }

            var trainee = _unitOfWork.Trainee.GetAllByCondition(c => c.ClassId == classId && c.AccountId == accountId).FirstOrDefault();
            if (trainee == null)
            {
                return new ResponseDTO("Người dùng không tồn tại trong lớp", 400, false);
            }

            var status = classRemove.Project.Status;

            if (status == ProjectStatusConstant.Cancelled ||
                status == ProjectStatusConstant.InProgress ||
                status == ProjectStatusConstant.Planning ||
                status == ProjectStatusConstant.Completed)
            {
                return new ResponseDTO("Học viên chỉ được xóa khi dự án sắp diễn ra", 400, false);
            }

            _unitOfWork.Trainee.Delete(trainee);

            var messageNotification = RemoveTraineeNotification.SendRemovedNotification(classRemove!.ClassCode, classRemove!.Project.Title);
            await _notificationService.CreateNotification(accountId, messageNotification);

            ProjectLogging logging = new ProjectLogging()
            {
                ProjectNoteId = Guid.NewGuid(),
                ActionDate = DateTime.Now,
                ProjectId = classRemove.Project.ProjectId,
                ActionContent = $"{trainee.Account.FullName} không còn là sinh viên hỗ trợ lớp {classRemove.ClassCode} của dự án {classRemove.Project.Title}",
                AccountId = accountId,
            };

            await _unitOfWork.ProjectLogging.AddAsync(logging);

            var result = await _unitOfWork.SaveChangeAsync();
            if (result)
            {
                return new ResponseDTO("Xóa học viên thành công", 200, true, null);
            }

            return new ResponseDTO("Xóa học viên thất bại", 500, false, null);

        }

        public async Task<ResponseDTO> AddTraineeHadAccount(AddTraineeHadAccountDTO addTraineeHadAccountDTO)
        {
            ResponseDTO responseDTO = await CheckAddTraineeHadAccount(addTraineeHadAccountDTO);
            if (!responseDTO.IsSuccess)
            {
                return responseDTO;
            }
            Trainee trainee = new Trainee()
            {
                TraineeId = Guid.NewGuid(),
                AccountId = addTraineeHadAccountDTO.AccountId,
                ClassId = addTraineeHadAccountDTO.ClassId,
            };
            var classToAdd = await _unitOfWork.Class.GetByCondition(c => c.ClassId.Equals(addTraineeHadAccountDTO.ClassId));
            if (classToAdd.NumberGroup.HasValue)
            {
                var traineeInClass = _unitOfWork.Trainee.GetAllByCondition(t => t.ClassId.Equals(classToAdd.ClassId));
                var traineesGroupedByGroupNo = traineeInClass
    .GroupBy(t => t.GroupNo); // Nhóm trainees theo GroupNo

                var groupCounts = traineesGroupedByGroupNo
            .Select(group => new { GroupNo = group.Key, Count = group.Count() });

                int maxGroupSize = groupCounts.Any() ? groupCounts.Max(g => g.Count) : 0;

                var smallerGroupNo = groupCounts
                    .Where(g => g.Count < maxGroupSize)
                    .Select(g => g.GroupNo).FirstOrDefault();
                if (smallerGroupNo == null)
                {
                    trainee.GroupNo = 1;
                }
                else
                {
                    trainee.GroupNo = smallerGroupNo.Value;
                }

            }
            await _unitOfWork.Trainee.AddAsync(trainee);
            var s = await _unitOfWork.SaveChangeAsync();
            if (!s)
            {
                return new ResponseDTO("Thêm học viên vào lớp thất bại", 500, false);
            }

            return new ResponseDTO("Thêm học viên vào lớp thành công", 201, true);
        }

        private async Task<ResponseDTO> CheckAddTraineeHadAccount(AddTraineeHadAccountDTO addTraineeHadAccountDTO)
        {
            List<string> listErr = new List<string>();
            // 1. Kiểm tra accountId có tồn tại
            var account = await _unitOfWork.Account.GetByCondition(a => a.AccountId.Equals(addTraineeHadAccountDTO.AccountId));
            if (account == null)
            {
                listErr.Add("Tài khoản không tồn tại");
            }
            if (!account.RoleId.Equals((int)RoleEnum.Trainee))
            {
                listErr.Add("Tài khoản không phải của học viên");
            }
            // 2. Kiểm tra classId có tồn tại và lấy projectId từ class
            var classToAdd = await _unitOfWork.Class.GetByCondition(c => c.ClassId.Equals(addTraineeHadAccountDTO.ClassId));
            if (classToAdd == null)
            {
                listErr.Add("Lớp không tồn tại");
            }
            else
            {
                var projectId = classToAdd.ProjectId; // Lấy projectId từ class

                // 3. Kiểm tra stage project đang là "Sắp diễn ra"
                var project = await _unitOfWork.Project.GetByCondition(p => p.ProjectId == projectId);
                if (project == null)
                {
                    listErr.Add("Dự án không tồn tại");
                }
                else
                {
                    if (project.Status != ProjectStatusConstant.Planning)
                    {
                        listErr.Add("Dự án không thể thêm học viên");
                    }
                }
                // Lấy danh sách các ClassId từ dự án
                var allClassOfProjectIds = _unitOfWork.Class
     .GetAllByCondition(c => c.ProjectId.Equals(projectId))
     .Select(c => c.ClassId)
     .ToList();

                var existingTrainee = _unitOfWork.Trainee
    .GetAllByCondition(t => allClassOfProjectIds.Contains(t.ClassId))
    .Where(t => t.AccountId.Equals(addTraineeHadAccountDTO.AccountId))
    .ToList();



                if (existingTrainee.Count() > 0)
                {
                    listErr.Add("Học viên đã ở trong 1 lớp của dự án này");
                }
            }
            if (listErr.Count > 0)
            {
                return new ResponseDTO("Thông tin thêm học viên không hợp lệ", 400, false, listErr);
            }
            return new ResponseDTO("Thông tin thêm học viên hợp lệ", 200, true);


        }
    }
}

