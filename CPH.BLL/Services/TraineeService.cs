using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CPH.BLL.Interfaces;
using CPH.Common.Constant;
using CPH.Common.DTO.Auth;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Member;
using CPH.Common.DTO.Paging;
using CPH.Common.DTO.Project;
using CPH.Common.DTO.Trainee;
using CPH.Common.Enum;
using CPH.Common.Notification;
using CPH.DAL.Entities;
using CPH.DAL.UnitOfWork;
using Firebase.Storage;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CPH.BLL.Services
{
    public class TraineeService : ITraineeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly IConfiguration _config;
        private readonly FirebaseApp _firebaseApp;
        private readonly string _firebaseBucket;
        private readonly IImageService _imageService;
        private readonly IAccountService _accountService;
        private readonly IEmailService _emailService;

        public TraineeService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService, IAccountService accountService, IEmailService emailService,
            IImageService imageService, IConfiguration config)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _accountService = accountService;
            _emailService = emailService;
            _imageService = imageService;
            _config = config;
            _firebaseBucket = _config.GetSection("FirebaseConfig")["storage_bucket"];

            if (FirebaseApp.DefaultInstance == null)
            {
                _firebaseApp = FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile("firebase.json"),
                    ProjectId = _config.GetSection("FirebaseConfig")["project_id"],
                });
            }
            else
            {
                _firebaseApp = FirebaseApp.DefaultInstance;
            }
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
                            c.Account.Phone.ToLower().Contains(searchValue.ToLower()) ||
                            (c.GroupNo != null && c.GroupNo.ToString()!.Contains(searchValue.ToLower()))
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
                        if (!filterField.Equals("fullName") && !filterField.Equals("accountCode") && !filterField.Equals("email") && !filterField.Equals("groupNo"))
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
                    : list.OrderBy(c => c.Account.Email),
                "groupNo" => filterOrder == FilterConstant.Descending
                    ? list.OrderByDescending(c => c.GroupNo)
                    : list.OrderBy(c => c.GroupNo)
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
                    return new ResponseDTO("Danh sách có chứa học viên không tồn tại trong lớp", 400, false);
                }

                if (trainee.Score == null)
                {
                    return new ResponseDTO("Không thể cập nhật điểm cho học viên nếu học viên chưa được chấm điểm trước đó", 400, false);
                }

                if (i.Score == null || i.Score < 0 || i.Score > 10)
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

            if (status != ProjectStatusConstant.Planning)
            {
                return new ResponseDTO("Học viên chỉ được xóa khi dự án ở giai đoạn Lên kế hoạch", 400, false);
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
                NoteContent = reason,
            };

            await _unitOfWork.ProjectLogging.AddAsync(logging);

            var result = await _unitOfWork.SaveChangeAsync();
            if (result)
            {
                return new ResponseDTO("Xóa học viên thành công", 200, true, null);
            }

            return new ResponseDTO("Xóa học viên thất bại", 500, false, null);
        }
        public async Task<ResponseDTO> ImportTraineeScoreByExcel(IFormFile file, Guid classId)
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var classObj = _unitOfWork.Class.GetAllByCondition(c => c.ClassId == classId)
                .Include(c => c.Trainees)
                .FirstOrDefault();
            if (classObj == null)
            {
                return new ResponseDTO("Lớp không tồn tại", 400, false);
            }
            var traineeOfClass = classObj.Trainees.ToList();
            if (traineeOfClass.Any(c => c.Score != null))
            {
                return new ResponseDTO("Sinh viên của lớp đã được cập nhật điểm trước đó", 400, false);
            }
            var studentScores = new List<TraineeScoreExcelDTO>();
            var errors = new List<string>();
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;
                for (int row = 2; row <= rowCount; row++)
                {
                    string? accountCode = worksheet.Cells[row, 2].Text.Trim();
                    string? traineeFullName = worksheet.Cells[row, 4].Text.Trim();
                    string? scoreText = worksheet.Cells[row, 5].Text.Trim();
                    studentScores.Add(new TraineeScoreExcelDTO()
                    {
                        AccountCode = accountCode,
                        TraineeFullName = traineeFullName,
                        Score = scoreText
                    });
                }
            }
            for (int i = 0; i < studentScores.Count; i++)
            {
                if (string.IsNullOrEmpty(studentScores[i].AccountCode))
                {
                    errors.Add($"Bỏ qua dòng {i + 2}: Mã học viên bị thiếu");
                }
                var account = await _unitOfWork.Account.GetByCondition(c => c.AccountCode == studentScores[i].AccountCode);
                if (!string.IsNullOrEmpty(studentScores[i].AccountCode) && account == null)
                {
                    errors.Add($"Bỏ qua dòng {i + 2}: Học viên có mã học viên {studentScores[i].AccountCode} không tồn tại");
                }
                if (account != null)
                {
                    var trainee = await _unitOfWork.Trainee.GetByCondition(c => c.AccountId == account.AccountId && c.ClassId == classId);
                    if (trainee == null)
                    {
                        errors.Add($"Bỏ qua dòng {i + 2}: Học viên có mã học viên {studentScores[i].AccountCode} không thuộc lớp hiện tại");
                    }
                }
                decimal score = 0;
                if (!decimal.TryParse(studentScores[i].Score, out score) || string.IsNullOrEmpty(studentScores[i].Score))
                {
                    errors.Add($"Bỏ qua dòng {i + 2}: Học viên có mã học viên {studentScores[i].AccountCode} có điểm không hợp lệ");
                }
                if (decimal.TryParse(studentScores[i].Score, out score) && (score < 0 || score > 10))
                {
                    errors.Add($"Bỏ qua dòng {i + 2}: Học viên có mã học viên {studentScores[i].AccountCode} có điểm không hợp lệ");
                }
            }
            if (errors.Count > 0)
            {
                return new ResponseDTO("File excel không hợp lệ", 400, false, errors);
            }
            foreach (var i in studentScores)
            {
                var account = await _unitOfWork.Account.GetByCondition(c => c.AccountCode == i.AccountCode);
                var trainee = await _unitOfWork.Trainee.GetByCondition(c => c.AccountId == account!.AccountId && c.ClassId == classId);
                trainee!.Score = decimal.Parse(i.Score!);
                _unitOfWork.Trainee.Update(trainee!);
            }
            var result = await _unitOfWork.SaveChangeAsync();
            if (result)
            {
                return new ResponseDTO("Lưu điểm học viên thành công", 200, true);
            }
            return new ResponseDTO("Lưu điểm học viên thất bại", 400, false);
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
        public MemoryStream ExportTraineeListExcel(Guid classId)
        {
            var traineeList = _unitOfWork.Trainee.GetAllByCondition(c => c.ClassId == classId).Include(c => c.Account).OrderBy(c => c.GroupNo);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("DanhSachDiem");

                worksheet.Cells[1, 1].Value = "STT";
                worksheet.Cells[1, 2].Value = "Mã học viên";
                worksheet.Cells[1, 3].Value = "Tên học viên";
                worksheet.Cells[1, 4].Value = "Nhóm";
                worksheet.Cells[1, 5].Value = "Điểm";

                using (var range = worksheet.Cells[1, 1, 1, 5])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Font.Size = 12;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                int row = 2;
                int stt = 1;
                foreach (var trainee in traineeList)
                {
                    worksheet.Cells[row, 1].Value = stt;
                    worksheet.Cells[row, 2].Value = trainee.Account.AccountCode;
                    worksheet.Cells[row, 3].Value = trainee.Account.FullName;
                    worksheet.Cells[row, 4].Value = trainee.GroupNo;
                    worksheet.Cells[row, 5].Value = trainee.Score;
                    row++;
                    stt++;
                }

                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                return stream;
            }
        }

        public async Task<string> StoreFileAndGetLink(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
            {
                return string.Empty;
            }

            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var firebaseStorage = new FirebaseStorage(
                    _firebaseBucket,
                    new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(string.Empty),
                        ThrowOnCancel = true
                    });

                var storageUrl = await firebaseStorage
                    .Child(folderName)
                    .Child(fileName)
                    .PutAsync(memoryStream);

                return storageUrl;
            }
        }

        public async Task<ResponseDTO> UpdateReport (Guid accountId, Guid classId, IFormFile file)
        {
            var account = await _unitOfWork.Account.GetByCondition(c => c.AccountId == accountId);
            if(account == null)
            {
                return new ResponseDTO("Người dùng không tồn tại", 400, false);
            }

            var clas = _unitOfWork.Class.GetAllByCondition(c => c.ClassId == classId).Include(c=> c.Project).FirstOrDefault();
            if (clas == null)
            {
                return new ResponseDTO("Lớp không tồn tại", 400, false);
            }

            var trainee = await _unitOfWork.Trainee.GetByCondition(c => c.ClassId == classId && c.AccountId == accountId);
            if(trainee == null)
            {
                return new ResponseDTO("Không tìm thấy học viên này trong lớp", 400, false);
            }

            var status = clas.Project.Status.ToString();
            if (!status.Equals(ProjectStatusConstant.InProgress))
            {
                return new ResponseDTO($"Dự án đang ở trạng thái {status}không thể cập nhật báo cáo", 400, false);
            }

            var lesson = _unitOfWork.Lesson
                .GetAllByCondition(c => c.ProjectId == clas.ProjectId)
                .OrderByDescending(c => c.LessonNo)
                .FirstOrDefault();


            var finishTime = _unitOfWork.LessonClass
                .GetAllByCondition(c => c.ClassId == classId && c.LessonId == lesson.LessonId)
                .Select(c=> c.EndTime)
                .FirstOrDefault();

            if(finishTime >= DateTime.Now)
            {
                return new ResponseDTO($"Báo cáo chỉ được cập nhật sau {finishTime}", 400, false);
            }

            if(trainee.FeedbackContent != null)
            {
                await _imageService.DeleteFileFromFirebase(trainee.FeedbackContent);
            }

            if (file == null || file.Length == 0)
            {
                return new ResponseDTO("File không hợp lệ!", 400, false);
            }

            var url = await StoreFileAndGetLink(file, "cph_trainee_report");
            if (url.IsNullOrEmpty())
            {
                return new ResponseDTO("Lưu tài liệu không thành công", 400, false);
            }
            trainee.ReportContent = url;
            trainee.ReportCreatedDate = DateTime.Now;

            _unitOfWork.Trainee.Update(trainee);

            await _unitOfWork.SaveChangeAsync();

            return new ResponseDTO("Cập nhật báo cáo thành công", 200, true);
        }

        public async Task<ResponseDTO> CheckValidationTrainee(SignUpRequestOfTraineeDTO model)
        {
            var classOfTrainee = await _unitOfWork.Class.GetByCondition(c => c.ClassId.Equals(model.ClassId));
            if (classOfTrainee == null)
            {
                return new ResponseDTO("Lớp không tồn tại", 400, false);
            }
            var project = await _unitOfWork.Project.GetByCondition(c => c.ProjectId.Equals(classOfTrainee.ProjectId));
            if (project == null)
            {
                return new ResponseDTO("Dự án không tồn tại", 400, false);
            }
            if (!project.Status.Equals(ProjectStatusConstant.Planning))
            {
                return new ResponseDTO("Dự án không nằm trong giai đoạn lên kế hoạch", 400, false);
            }
            return new ResponseDTO("Thông tin thêm trainee hợp lệ", 200, true);

        }
        public async Task<ResponseDTO> CheckValidationSignUpOfTrainee(SignUpRequestOfTraineeDTO model)
        {
            if (model.DateOfBirth >= DateTime.Now)
            {
                return new ResponseDTO("Ngày sinh phải nhỏ hơn ngày hiện tại", 400, false);
            }

            if (model.Gender != GenderConstant.Male
                && model.Gender != GenderConstant.Female)
            {
                return new ResponseDTO("Giới tính không hợp lệ", 400, false);
            }

            var checkAccountNameExist = _accountService.CheckAccountNameExist(model.AccountName);
            if (checkAccountNameExist)
            {
                return new ResponseDTO("Tên tài khoản đã tồn tại", 400, false);
            }

            var checkEmailExist = _accountService.CheckEmailExist(model.Email);
            if (checkEmailExist)
            {
                return new ResponseDTO("Email đã tồn tại", 400, false);
            }

            var checkPhoneExist = _accountService.CheckPhoneExist(model.Phone);
            if (checkPhoneExist)
            {
                return new ResponseDTO("Số điện thoại đã tồn tại", 400, false);
            }
            return new ResponseDTO("Kiểm tra thành công", 200, true);
        }
        public async Task<ResponseDTO> AddTraineeNoAccount(SignUpRequestOfTraineeDTO model)
        {
            var check1 = await CheckValidationTrainee(model);
            if (!check1.IsSuccess)
            {
                return check1;
            }
            var check2 = await CheckValidationSignUpOfTrainee(model);
            if (!check2.IsSuccess)
            {
                return check2;
            }
            var account = _mapper.Map<Account>(model);

            var salt = _accountService.GenerateSalt();
            var generatedPassword = _accountService.GeneratePasswordString();
            var passwordHash = _accountService.GenerateHashedPassword(generatedPassword, salt);
            //var avatarLink = await _imageService.StoreImageAndGetLink(model.AvatarLink, FileNameFirebaseStorage.UserImage);
            var accountId = Guid.NewGuid();
            account.AccountId = accountId;
            account.AccountCode = _accountService.GenerateAccountCode((int)RoleEnum.Trainee);
            account.Salt = salt;
            account.PasswordHash = passwordHash;
            account.Status = true;
            account.RoleId = (int)RoleEnum.Trainee;
            await _unitOfWork.Account.AddAsync(account);


            Trainee trainee = new Trainee()
            {
                TraineeId = Guid.NewGuid(),
                AccountId = accountId,
                ClassId = model.ClassId,
            };
            var classToAdd = await _unitOfWork.Class.GetByCondition(c => c.ClassId.Equals(model.ClassId));
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
            await _emailService.SendAccountEmail(account.Email, account.AccountName, generatedPassword, "The Community Project Hub's account");
            return new ResponseDTO("Thêm học viên vào lớp thành công", 201, true);
        }

        public List<MemberResponseDTO> SearchTraineeToAddToClass(string? searchValue)
        {
            if (searchValue.IsNullOrEmpty())
            {
                return new List<MemberResponseDTO>();
            }

            var searchedList = _unitOfWork.Account.GetAllByCondition(c => (c.AccountCode.ToLower().Contains(searchValue!.ToLower())
            || c.FullName.ToLower().Contains(searchValue.ToLower()) || c.Email.ToLower().Contains(searchValue.ToLower())
            || c.Phone.ToLower().Contains(searchValue.ToLower())) && c.RoleId == (int)RoleEnum.Trainee).ToList();

            var mappedSearchedList = _mapper.Map<List<MemberResponseDTO>>(searchedList);
            return mappedSearchedList;
        }
    }
}



