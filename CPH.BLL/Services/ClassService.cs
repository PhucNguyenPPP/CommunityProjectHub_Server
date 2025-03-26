using AutoMapper;
using CPH.BLL.Interfaces;
using CPH.Common.Constant;
using CPH.Common.DTO.Account;
using CPH.Common.DTO.Class;
using CPH.Common.DTO.General;
using CPH.Common.DTO.LessonClass;
using CPH.Common.DTO.Member;
using CPH.Common.DTO.Message;
using CPH.Common.DTO.Paging;
using CPH.Common.DTO.Trainee;
using CPH.Common.Enum;
using CPH.Common.Notification;
using CPH.DAL.Entities;
using CPH.DAL.UnitOfWork;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.Filters;
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
        private readonly INotificationService _notificationService;

        public ClassService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
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
            var existed = await CheckClassIdExist(devideGroupOfClassDTO.ClassId);
            if (!existed)
            {
                return new ResponseDTO("Lớp không tồn tại", 400, false);
            }

            var check = await CheckDivision(devideGroupOfClassDTO);
            if (!check.IsSuccess)
            {
                return new ResponseDTO(check.Message.ToString(), 400, false);
            }
            if (check.Result == null)
            {
                return new ResponseDTO("Lớp học lỗi", 400, false);
            }
            Class temp = (Class)check.Result;
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
                    trainee.GroupNo = i;
                    i++;
                }
            }
            _unitOfWork.Trainee.UpdateRange(traineesOfClass.ToList());
            var saved = await _unitOfWork.SaveChangeAsync();
            if (saved)
            {
                return new ResponseDTO("Chia nhóm cho lớp thành công", 200, true);
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
            var c = await _unitOfWork.Class.GetByCondition(c => c.ClassId.Equals(devideGroupOfClassDTO.ClassId));
            if(!c.LecturerId.HasValue)
            {
                return new ResponseDTO("Lớp cần được phân giảng viên trước khi chia nhóm", 400, false);
            }
            var pro = await _unitOfWork.Project.GetByCondition(p => p.ProjectId.Equals(c.ProjectId));
            if (pro == null)
            {
                return new ResponseDTO("Lớp thuộc dự án bị lỗi", 400, false);
            }
            if (!pro.Status.Equals(ProjectStatusConstant.UpComing))
            {
                return new ResponseDTO("Dự án đang ở trạng thâi " + pro.Status.ToString()+ " nên không thể chia nhóm", 400, false);
            }
            var mem =  _unitOfWork.Member.GetAllByCondition(m => m.ClassId.Equals(devideGroupOfClassDTO.ClassId));
            if (mem.Count() > 0)
            {
                return new ResponseDTO("Không thể chia nhóm lại khi đã có sinh viên tham gia hỗ trợ lớp", 400, false);
            }
            return new ResponseDTO("Thông tin chia nhóm của lớp hợp lệ", 200, true, c);
        }

        public async Task<ResponseDTO> GetAllClassOfProject(Guid projectId, string? searchValue, int? pageNumber, int? rowsPerPage)
        {
            IQueryable<Class> list = _unitOfWork.Class.GetAllByCondition(c => c.ProjectId == projectId).Include(c => c.Lecturer).Include(c => c.Members).Include(c => c.Trainees);
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
        }

        public async Task<ResponseDTO> GetClassDetail(Guid classId)
        {
            var clas = _unitOfWork.Class.GetAllByCondition(c => c.ClassId == classId)
                .Include(c => c.Trainees)
                .Include(c => c.Members)
                .Include(c => c.Lecturer)
                .Include(c => c.Project)
                .FirstOrDefault();
            if (clas == null)
            {
                return new ResponseDTO("Lớp không tồn tại", 400, false);
            }

            int lecturerSlotAvailable = 0;

            if (clas.LecturerId == null)
            {
                lecturerSlotAvailable = 1;
            }

            var projectId = _unitOfWork.Class.GetAllByCondition(c => c.ClassId == classId).Select(c => c.ProjectId).FirstOrDefault();

            int totalTrainees = clas.Trainees.Count();

            int? groupRequired = clas.NumberGroup;

            int groupWithStudent = clas.Members?.Count() ?? 0;

            int? studentSlotAvailable = groupRequired - groupWithStudent;

            var member = _unitOfWork.Member
                .GetAllByCondition(c => c.ClassId == classId)
                .Select(c => c.Account)
                .ToList();

            var memberDto = _mapper.Map<List<GetMemberOfClassDTO>>(member);

            var traineeList = _unitOfWork.Trainee
                .GetAllByCondition(c => c.ClassId == classId)
                .Select(c => c.Account)
                .ToList();

            var traineeListDto = _mapper.Map<List<GetTraineeOfClassDTO>>(traineeList);


            var dto = _mapper.Map<ClassDetailDTO>(clas);
            dto.LecturerSlotAvailable = lecturerSlotAvailable;
            dto.StudentSlotAvailable = studentSlotAvailable;
            dto.getMemberOfClassDTOs = memberDto;
            dto.getTraineeOfClassDTOs = traineeListDto;

            return new ResponseDTO("Lấy thông tin chi tiết của lớp thành công", 200, true, dto);

        }

        public async Task<ResponseDTO> UpdateClass(UpdateClassDTO updateClassDTO)
        {
            ResponseDTO responseDTO = await CheckUpdateClass(updateClassDTO);
            if (!responseDTO.IsSuccess)
            {
                return responseDTO;
            }
            var c = await _unitOfWork.Class.GetByCondition(c => c.ClassId.Equals(updateClassDTO.ClassId));
            var acc = await _unitOfWork.Account.GetByCondition(a => a.AccountId.Equals(updateClassDTO.AccountId));
            if (updateClassDTO.RoleId == (int)RoleEnum.Student)
            {
                var numOfGroup = c.NumberGroup;
                for (int i = 1; i <= numOfGroup; i++)
                {
                    var mem = await _unitOfWork.Member.GetByCondition(m => m.ClassId.Equals(updateClassDTO.ClassId) && m.GroupSupportNo.Equals(i));
                    if (mem == null)
                    {
                        Member member = new Member()
                        {
                            MemberId = Guid.NewGuid(),
                            ClassId = updateClassDTO.ClassId,
                            GroupSupportNo = i,
                            AccountId = updateClassDTO.AccountId,
                        };
                        await _unitOfWork.Member.AddAsync(member);

                        ProjectLogging logging = new ProjectLogging()
                        {
                            ProjectNoteId = Guid.NewGuid(),
                            ActionDate = DateTime.Now,
                            ProjectId = c.ProjectId,
                            ActionContent = $"{acc.FullName} đã tham gia hỗ trợ lớp {c.ClassCode}",
                            AccountId = acc.AccountId,

                        };
                        await _unitOfWork.ProjectLogging.AddAsync(logging);
                        break;
                    }
                }
            }
            else
            {
                c.LecturerId = updateClassDTO.AccountId;
                _unitOfWork.Class.Update(c);
                ProjectLogging logging = new ProjectLogging()
                {
                    ProjectNoteId = Guid.NewGuid(),
                    ActionDate = DateTime.Now,
                    ProjectId = c.ProjectId,
                    ActionContent = $"{acc.FullName} đã trở thành giảng viên của lớp {c.ClassCode}",
                    AccountId = acc.AccountId,

                };
                await _unitOfWork.ProjectLogging.AddAsync(logging);
            }
            //Create notification
            var sender = await _unitOfWork.Account.GetByCondition(c => c.AccountId == updateClassDTO.AccountId);
            if (updateClassDTO.RoleId.Equals((int)RoleEnum.Student))
            {
                var messageNotification = UpdateClassNotification.SendUpdateClassNotification("sinh viên hỗ trợ", c.ClassCode, c.Project.Title);
                await _notificationService.CreateNotification(updateClassDTO.AccountId, messageNotification);
            }
            else
            {
                var messageNotification = UpdateClassNotification.SendUpdateClassNotification("giảng viên", c.ClassCode, c.Project.Title);
                await _notificationService.CreateNotification(updateClassDTO.AccountId, messageNotification);
            }

            //End of create notification

            var ans = await _unitOfWork.SaveChangeAsync();
            if (ans)
            {
                return new ResponseDTO("Phân công thành công", 200, true);

            }
            return new ResponseDTO("Phân công thất bại", 500, false);
        }

        public async Task<ResponseDTO> CheckUpdateClass(UpdateClassDTO updateClassDTO)
        {
            List<string> errs = new List<string>();
            var clas = await _unitOfWork.Class.GetByCondition(c => c.ClassId.Equals(updateClassDTO.ClassId));
            if (clas == null)
            {
                errs.Add("Lớp không tồn tại");
            }
            //if (clas != null && !clas.NumberGroup.HasValue)
            //{
            //    errs.Add("Không thể phân công vào lớp chưa chia nhóm");
            //}
            var pro = await _unitOfWork.Project.GetByCondition(p => p.ProjectId.Equals(clas.ProjectId));
            if (pro.Status != ProjectStatusConstant.UpComing)
            {
                errs.Add("Lớp thuộc dự án có trạng thái " + pro.Status.ToString() + " nên không thể phân công");
            }
            var acc = await _unitOfWork.Account.GetByCondition(a => a.AccountId.Equals(updateClassDTO.AccountId));
            if (acc == null)
            {
                errs.Add("Tài khoản không tồn tại");
            }
            if (acc != null && !acc.RoleId.Equals(updateClassDTO.RoleId))
            {
                errs.Add("Tài khoản này không phù hợp với vị trí cần thay thế");
            }
            if (updateClassDTO.RoleId.Equals((int)RoleEnum.Lecturer))
            {
                if (clas.LecturerId.HasValue)
                {
                    errs.Add("Lớp không còn trống slot dành cho giảng viên");
                }
                //    var pros = _unitOfWork.Project.GetAllByCondition(p => p.Status.Equals(ProjectStatusConstant.UpComing) || p.Status.Equals(ProjectStatusConstant.InProgress)).Select(p => p.ProjectId).ToList();
                var classOfAcc = _unitOfWork.Registration.GetAllByCondition(r => r.AccountId.ToString().Equals(updateClassDTO.AccountId.ToString()) &&
                               r.Status.Equals(RegistrationStatusConstant.Processing)).Select(r => r.ClassId).ToList();
                var cla = _unitOfWork.Class.GetAllByCondition(c => c.LecturerId.Equals(updateClassDTO.AccountId)).Select(c => c.ClassId).ToList();
                if (cla.Count > 0)
                {
                    classOfAcc.AddRange(cla);
                }
                var classAct = _unitOfWork.Class.GetAllByCondition(c => c.Project.Status.Equals(ProjectStatusConstant.UpComing) || c.Project.Status.Equals(ProjectStatusConstant.InProgress)).ToList();
                var classActivate = classAct.Where(c => classOfAcc.Contains(c.ClassId)).Select(c => c.ClassId).ToList();
                if (classActivate != null)
                {
                    var lscToRegister = _unitOfWork.LessonClass.GetAllByCondition(lsc => lsc.ClassId.Equals(updateClassDTO.ClassId)); //đang đky

                    for (int i = 0; i < classActivate.Count(); i++)
                    {
                        if (classActivate[i].Equals(updateClassDTO.ClassId))
                        {
                            errs.Add("Bạn đã đăng ký hoặc được phân công vào lớp này trước đó");
                            break;
                        }
                        var lscOfAccRegistered = _unitOfWork.LessonClass.GetAllByCondition(lsc => lsc.ClassId.Equals(classActivate[i])).ToList(); //đã đky rồi
                        for (int j = 0; j < lscOfAccRegistered.Count(); j++)
                        {
                            if (lscOfAccRegistered[j].StartTime != null && lscOfAccRegistered[j].EndTime != null)
                            {
                                var checklsc = lscToRegister.Where(t => !(t.StartTime < lscOfAccRegistered[j].StartTime
                                && t.EndTime < lscOfAccRegistered[j].StartTime || t.StartTime > lscOfAccRegistered[j].EndTime && t.EndTime > lscOfAccRegistered[j].EndTime) && t.StartTime != null && t.EndTime != null);
                                if (checklsc.Count() > 0)
                                {
                                    var c = await _unitOfWork.Class.GetByCondition(c => c.ClassId.Equals(lscOfAccRegistered[j].ClassId));
                                    var l = await _unitOfWork.Lesson.GetByCondition(c => c.LessonId.Equals(lscOfAccRegistered[j].LessonId));
                                    errs.Add("Lớp học trùng lịch với buổi học " + l.LessonNo.ToString() + " của lớp " + c.ClassCode + " mà " + acc.FullName + " đăng ký trước đó");
                                }
                            }
                        }

                    }
                }
            }
            else
            {
                var mems = _unitOfWork.Member.GetAllByCondition(m => m.ClassId.Equals(updateClassDTO.ClassId));
                if (mems.Count() >= clas.NumberGroup)
                {
                    errs.Add("Lớp không còn trống slot dành cho sinh viên hỗ trợ");
                }
                var pros = _unitOfWork.Project.GetAllByCondition(p => p.Status.Equals(ProjectStatusConstant.UpComing) || p.Status.Equals(ProjectStatusConstant.InProgress)).Select(p => p.ProjectId);
                List<Guid> classOfAcc = _unitOfWork.Registration.GetAllByCondition(r => r.AccountId.ToString().Equals(updateClassDTO.AccountId.ToString()) &&
                               r.Status.Equals(RegistrationStatusConstant.Processing) || r.Status.Equals(RegistrationStatusConstant.Inspected)).Select(r => r.ClassId).ToList();
                var mem = _unitOfWork.Member.GetAllByCondition(m => m.AccountId.Equals(updateClassDTO.AccountId)).Select(m => m.ClassId).ToList();
                classOfAcc.AddRange(mem);
                var classAct = _unitOfWork.Class.GetAllByCondition(c => pros.Contains(c.ProjectId)).ToList();
                var classActivate = classAct.Where(c => classOfAcc.Contains(c.ClassId)).Select(c => c.ClassId).ToList();
                var lscToRegister = _unitOfWork.LessonClass.GetAllByCondition(lsc => lsc.ClassId.Equals(updateClassDTO.ClassId)); //đang đky
                if (classActivate != null)
                {
                    for (int i = 0; i < classActivate.Count(); i++)
                    {
                        if (classActivate[i].Equals(updateClassDTO.ClassId))
                        {
                            errs.Add("Bạn đã đăng ký hoặc được phân công vào lớp này trước đó");
                        }
                        var lscOfAccRegistered = _unitOfWork.LessonClass.GetAllByCondition(lsc => lsc.ClassId.Equals(classActivate[i])).ToList(); //đã đky rồi
                        for (int j = 0; j < lscOfAccRegistered.Count(); j++)
                        {
                            if (lscOfAccRegistered[j].StartTime != null && lscOfAccRegistered[j].EndTime != null)
                            {
                                var checklsc = lscToRegister.Where(t => !(t.StartTime < lscOfAccRegistered[j].StartTime
                                && t.EndTime < lscOfAccRegistered[j].StartTime || t.StartTime > lscOfAccRegistered[j].EndTime && t.EndTime > lscOfAccRegistered[j].EndTime));
                                if (checklsc.Count() > 0)
                                {
                                    var c = await _unitOfWork.Class.GetByCondition(c => c.ClassId.Equals(lscOfAccRegistered[j].ClassId));
                                    var l = await _unitOfWork.Lesson.GetByCondition(c => c.LessonId.Equals(lscOfAccRegistered[j].LessonId));
                                    errs.Add("Lớp học trùng lịch với buổi học " + l.LessonNo.ToString() + " của lớp " + c.ClassCode + " mà " + acc.FullName + " đăng ký trước đó");
                                }
                            }
                        }
                    }

                }
            }
            if (errs.Count > 0)
            {
                return new ResponseDTO("Thông tin cần cập nhật không hợp lệ", 400, false, errs);
            }
            return new ResponseDTO("Thông tin cần cập nhật hợp lệ", 200, true);
        }

        public async Task<ResponseDTO> GetAllClassOfLecturer(string? searchValue, Guid lecturerId)
        {
            var lecturer = await _unitOfWork.Account.GetByCondition(c => c.AccountId == lecturerId && c.RoleId == (int)RoleEnum.Lecturer);

            if (lecturer == null)
            {
                return new ResponseDTO("Giảng viên không tồn tại", 400, false);
            }


            List<Class>? classLecturer = new List<Class>();
            if (searchValue.IsNullOrEmpty())
            {
                classLecturer = _unitOfWork.Class.GetAllByCondition(c => c.LecturerId == lecturerId)
                   .Include(c => c.Project)
                   .Include(c => c.Lecturer)
                   .Include(c => c.Trainees)
                   .ToList();
            }
            else
            {
                classLecturer = _unitOfWork.Class.GetAllByCondition(c => c.LecturerId == lecturerId)
                  .Include(c => c.Project)
                  .Include(c => c.Lecturer)
                  .Include(c => c.Trainees)
                  .Where(c => c.ClassCode.Contains(searchValue!) || c.Project.Title.Contains(searchValue!))
                  .ToList();
            }

            var mappedList = _mapper.Map<List<GetAllClassOfLecturer>>(classLecturer);
            return new ResponseDTO("Lấy danh sách lớp của giảng viên thành công", 200, true, mappedList);
        }

        public async Task<ResponseDTO> RemoveUpdateClass(RemoveUpdateClassDTO model)
        {
            var removedAccount = _unitOfWork.Account.GetAllByCondition(c => c.AccountId == model.RemovedAccountId).FirstOrDefault();
            if (removedAccount == null)
            {
                return new ResponseDTO("Tài khoản bị thay thế không tồn tại", 400, false);
            }

            var classObj = _unitOfWork.Class.GetAllByCondition(c => c.ClassId == model.ClassId)
                .Include(c => c.Project)
                .Include(c => c.Members)
                .Include(c => c.LessonClasses)
                .FirstOrDefault();
            if (classObj == null)
            {
                return new ResponseDTO("Lớp không tồn tại", 400, false);
            }

            var newAccount = _unitOfWork.Account.GetAllByCondition(c => c.AccountId == model.AccountId)
                .Include(c => c.Role)
                .FirstOrDefault();
            if (newAccount == null)
            {
                return new ResponseDTO("Tài khoản được phân công không tồn tại", 400, false);
            }

            if (classObj.Project.Status != ProjectStatusConstant.InProgress)
            {
                return new ResponseDTO("Không thể xóa thành viên trong giai đoạn này của dự án", 400, false);
            }

            if (model.RoleId == (int)RoleEnum.Student)
            {
                var oldMember = _unitOfWork.Member
                  .GetAllByCondition(c => c.AccountId == model.RemovedAccountId && c.ClassId == model.ClassId)
                  .Include(c => c.Account)
                  .Include(c => c.Class)
                  .ThenInclude(c => c.Project)
                  .FirstOrDefault();

                if (oldMember == null)
                {
                    return new ResponseDTO("Sinh viên bị xóa không tồn tại trong lớp", 400, false);
                }

                var memberListInClass = classObj.Members.ToList();
                if (memberListInClass.Any(c => c.AccountId == model.AccountId))
                {
                    return new ResponseDTO("Sinh viên đã được phân công trong lớp từ trước đó", 400, false);
                }

                var assignedClassListStudent = _unitOfWork.Member.GetAllByCondition(c => c.AccountId == model.AccountId)
                    .Include(c => c.Class)
                    .ThenInclude(c => c.LessonClasses)
                    .Select(c => c.Class)
                    .ToList();

                var assignedLessonTimes = assignedClassListStudent
                    .SelectMany(c => c.LessonClasses)
                    .Select(lc => new { lc.StartTime, lc.EndTime })
                    .ToList();

                bool isTimeConflict = classObj.LessonClasses.Any(lc =>
                    assignedLessonTimes.Any(al =>
                        (lc.StartTime < al.EndTime && lc.EndTime > al.StartTime)
                    )
                );

                if (isTimeConflict)
                {
                    return new ResponseDTO("Lịch bị trùng, không thể phân công sinh viên này vào lớp", 400, false);
                }

                var removedMessageNotification = RemoveMemberNotification.SendRemovedNotification(classObj.ClassCode, classObj.Project.Title);
                await _notificationService.CreateNotification(model.RemovedAccountId, removedMessageNotification);

                ProjectLogging removedLogging = new ProjectLogging()
                {
                    ProjectNoteId = Guid.NewGuid(),
                    ActionDate = DateTime.Now,
                    ProjectId = oldMember.Class.Project.ProjectId,
                    ActionContent = $"{oldMember.Account.FullName} không còn là sinh viên hỗ trợ lớp {oldMember.Class.ClassCode} của dự án {oldMember!.Class.Project.Title}",
                    AccountId = model.RemovedAccountId,
                };

                await _unitOfWork.ProjectLogging.AddAsync(removedLogging);
                oldMember.AccountId = model.AccountId;
                _unitOfWork.Member.Update(oldMember);

                var addNewMessageNotification = UpdateClassNotification.SendUpdateClassNotification(newAccount.Role.RoleName, classObj.ClassCode, classObj.Project.Title);
                await _notificationService.CreateNotification(model.AccountId, addNewMessageNotification);

                ProjectLogging addNewLogging = new ProjectLogging()
                {
                    ProjectNoteId = Guid.NewGuid(),
                    ActionDate = DateTime.Now,
                    ProjectId = oldMember.Class.Project.ProjectId,
                    ActionContent = $"{newAccount.FullName} đã tham gia hỗ trợ lớp {classObj.ClassCode}",
                    AccountId = model.AccountId,
                };
                await _unitOfWork.ProjectLogging.AddAsync(addNewLogging);

                var result = await _unitOfWork.SaveChangeAsync();
                if (result)
                {
                    return new ResponseDTO("Phân công sinh viên khác thành công", 200, true);
                }
                return new ResponseDTO("Phân công sinh viên khác thất bại", 400, false);
            }
            else
            {
                if (classObj.LecturerId != model.RemovedAccountId)
                {
                    return new ResponseDTO("Giảng viên bị xóa không tồn tại trong lớp", 400, false);
                }

                if (classObj.LecturerId == model.AccountId)
                {
                    return new ResponseDTO("Giảng viên đã được phân công trong lớp từ trước đó", 400, false);
                }

                var assignedClassListLecturer = _unitOfWork.Class.GetAllByCondition(c => c.LecturerId == model.AccountId)
                    .Include(c => c.LessonClasses)
                    .ToList();

                var assignedLessonTimes = assignedClassListLecturer
                    .SelectMany(c => c.LessonClasses)
                    .Select(lc => new { lc.StartTime, lc.EndTime })
                    .ToList();

                bool isTimeConflict = classObj.LessonClasses.Any(lc =>
                    assignedLessonTimes.Any(al =>
                        (lc.StartTime < al.EndTime && lc.EndTime > al.StartTime)
                    )
                );

                if (isTimeConflict)
                {
                    return new ResponseDTO("Lịch bị trùng, không thể phân công giảng viên này vào lớp", 400, false);
                }

                var removedMessageNotification = RemoveMemberNotification.SendRemovedLecturerNotification(classObj.ClassCode, classObj.Project.Title);
                await _notificationService.CreateNotification(model.RemovedAccountId, removedMessageNotification);

                ProjectLogging removedLogging = new ProjectLogging()
                {
                    ProjectNoteId = Guid.NewGuid(),
                    ActionDate = DateTime.Now,
                    ProjectId = classObj.ProjectId,
                    ActionContent = $"{removedAccount.FullName} không còn là giảng viên {classObj.ClassCode} của dự án {classObj.Project.Title}",
                    AccountId = model.RemovedAccountId,
                };

                await _unitOfWork.ProjectLogging.AddAsync(removedLogging);
                classObj.LecturerId = model.AccountId;
                _unitOfWork.Class.Update(classObj);

                var addNewMessageNotification = UpdateClassNotification.SendUpdateClassNotification(newAccount.Role.RoleName, classObj.ClassCode, classObj.Project.Title);
                await _notificationService.CreateNotification(model.AccountId, addNewMessageNotification);

                ProjectLogging addNewLogging = new ProjectLogging()
                {
                    ProjectNoteId = Guid.NewGuid(),
                    ActionDate = DateTime.Now,
                    ProjectId = classObj.ProjectId,
                    ActionContent = $"{newAccount.FullName} đã trở thành giảng viên của lớp {classObj.ClassCode}",
                    AccountId = model.AccountId,
                };
                await _unitOfWork.ProjectLogging.AddAsync(addNewLogging);

                var result = await _unitOfWork.SaveChangeAsync();
                if (result)
                {
                    return new ResponseDTO("Phân công giảng viên khác thành công", 200, true);
                }
                return new ResponseDTO("Phân công giảng viên khác thất bại", 400, false);
            }
        }

        public async Task<ResponseDTO> GetAllClassOfTrainee(string? searchValue, Guid accountId)
        {
            var trainee = await _unitOfWork.Account.GetByCondition(c => c.AccountId == accountId);

            if (trainee == null)
            {
                return new ResponseDTO("Học viên không tồn tại", 400, false);
            }


            List<GetAllClassOfTrainee>? classTrainee = new List<GetAllClassOfTrainee>();
            if (searchValue.IsNullOrEmpty())
            {
                classTrainee = (List<GetAllClassOfTrainee>)_unitOfWork.Trainee.GetAllByCondition(c => c.AccountId == accountId)
                   .Include(c => c.Class)
                   .ThenInclude(c => c.Project)
                   .Include(c => c.Class)
                   .ThenInclude(c => c.Lecturer)
                    .Select(c => new GetAllClassOfTrainee
                    {
                        ClassId = c.Class.ClassId,
                        ClassCode = c.Class.ClassCode,
                        ReportContent = c.Class.ReportContent,
                        ReportCreatedDate = c.Class.ReportCreatedDate,
                        TraineeScore = c.Score,
                        TraineeReportContent = c.ReportContent,
                        TraineeReportCreatedDate = c.ReportCreatedDate,
                        ProjectId = c.Class.Project.ProjectId,
                        ProjectTitle = c.Class.Project.Title,
                        ProjectStartDate = c.Class.Project.StartDate,
                        ProjectEndDate = c.Class.Project.EndDate,
                        ProjectAddress = c.Class.Project.Address,
                        ProjectNumberLesson = c.Class.Project.NumberLesson,
                        ProjectApplicationStartDate = c.Class.Project.ApplicationStartDate,
                        ProjectApplicationEndDate = c.Class.Project.ApplicationEndDate,
                        ProjectCreatedDate = c.Class.Project.CreatedDate,
                        ProjectStatus = c.Class.Project.Status,
                        ProjectManagerId = c.Class.Project.ProjectManagerId,
                        LecturerId = c.Class.Lecturer.AccountId,
                        LecturerName = c.Class.Lecturer.FullName,
                        LecturerPhone = c.Class.Lecturer.Phone,
                        LecturerEmail = c.Class.Lecturer.Email
                    }).ToList();
            }
            else
            {
                classTrainee = _unitOfWork.Trainee.GetAllByCondition(c => c.AccountId == accountId)
                   .Include(c => c.Class)
                   .ThenInclude(c => c.Project)
                   .Include(c => c.Class)
                   .ThenInclude(c => c.Lecturer)
                    .Select(c => new GetAllClassOfTrainee
                    {
                        ClassId = c.Class.ClassId,
                        ClassCode = c.Class.ClassCode,
                        ReportContent = c.Class.ReportContent,
                        ReportCreatedDate = c.Class.ReportCreatedDate,
                        TraineeScore = c.Score,
                        TraineeReportContent = c.ReportContent,
                        TraineeReportCreatedDate = c.ReportCreatedDate,
                        ProjectId = c.Class.Project.ProjectId,
                        ProjectTitle = c.Class.Project.Title,
                        ProjectStartDate = c.Class.Project.StartDate,
                        ProjectEndDate = c.Class.Project.EndDate,
                        ProjectAddress = c.Class.Project.Address,
                        ProjectNumberLesson = c.Class.Project.NumberLesson,
                        ProjectApplicationStartDate = c.Class.Project.ApplicationStartDate,
                        ProjectApplicationEndDate = c.Class.Project.ApplicationEndDate,
                        ProjectCreatedDate = c.Class.Project.CreatedDate,
                        ProjectStatus = c.Class.Project.Status,
                        ProjectManagerId = c.Class.Project.ProjectManagerId,
                        LecturerId = c.Class.Lecturer.AccountId,
                        LecturerName = c.Class.Lecturer.FullName,
                        LecturerPhone = c.Class.Lecturer.Phone,
                        LecturerEmail = c.Class.Lecturer.Email
                    })
                    .Where(c => c.ClassCode.Contains(searchValue!) || c.ProjectTitle.Contains(searchValue!))
                    .ToList();
            }
            return new ResponseDTO("Lấy danh sách lớp của học viên thành công", 200, true, classTrainee);
        }

        public async Task<ResponseDTO> GetAllClassOfStudent(string? searchValue, Guid accountId)
        {
            var student = await _unitOfWork.Account.GetByCondition(c => c.AccountId == accountId);

            if (student == null)
            {
                return new ResponseDTO("Sinh viên không tồn tại", 400, false);
            }


            List<Class>? classStudent = new List<Class>();
            if (searchValue.IsNullOrEmpty())
            {
                classStudent = _unitOfWork.Member.GetAllByCondition(c => c.AccountId == accountId)
                   .Include(c => c.Class)
                   .ThenInclude(c => c.Project)
                   .Include(c => c.Class)
                   .ThenInclude(c => c.Lecturer)
                   .Select(c => c.Class)
                   .ToList();
            }
            else
            {
                classStudent = _unitOfWork.Member.GetAllByCondition(c => c.AccountId == accountId)
                  .Include(c => c.Class)
                  .ThenInclude(c => c.Project)
                  .Include(c => c.Class)
                  .ThenInclude(c => c.Lecturer)
                  .Select(c => c.Class)
                  .Where(c => c.ClassCode.Contains(searchValue!) || c.Project.Title.Contains(searchValue!))
                  .ToList();
            }

            var mappedList = _mapper.Map<List<GetAllClassOfStudent>>(classStudent);
            return new ResponseDTO("Lấy danh sách lớp của sinh viên thành công", 200, true, mappedList);
        }

        public async Task<ResponseDTO> GetAllAvailableClassOfTrainee(Guid accountId, Guid currentClassId)
        {
            var account = await _unitOfWork.Account.GetByCondition(a => a.AccountId.Equals(accountId));
            if (account == null)
            {
                return new ResponseDTO("Tài khoản của học viên không tồn tại", 400, false);
            }
            var classOfTrainee = await _unitOfWork.Class.GetByCondition(c => c.ClassId == currentClassId);
            if (classOfTrainee == null)
            {
                return new ResponseDTO("Lớp của học viên không tồn tại", 400, false);
            }
            var trainee = await _unitOfWork.Trainee.GetByCondition(t=>t.AccountId.Equals(accountId) && t.ClassId.Equals(currentClassId));
            if(trainee == null)
            {
                return new ResponseDTO("Thông tin học viên và lớp hiện tại không khớp nhau", 400, false);
            }    
            var project = await _unitOfWork.Project.GetByCondition(p => p.ProjectId.Equals(classOfTrainee.ProjectId));
            if (project == null || !project.Status.Equals(ProjectStatusConstant.UpComing))
            {
                return new ResponseDTO("Dự án này hiện không thể đổi lớp", 400, false);
            }
            var otherClassIds = _unitOfWork.Class.GetAllByCondition(c => c.ProjectId.Equals(classOfTrainee.ProjectId) && !c.ClassId.Equals(currentClassId)).Select(c => c.ClassId).ToList();
            if (!otherClassIds.Any())
            {
                return new ResponseDTO("Không có lớp phù hợp để chuyển vào", 400, false);
            }
         //   var query = _unitOfWork.Trainee.GetAllByCondition(t => otherClassIds.Contains(t.ClassId));
           // Console.WriteLine(query.ToQueryString()); // Hiển thị câu truy vấn SQL cuối cùng            Console.WriteLine(query.ToQueryString()); // Hiển thị câu lệnh SQL được tạo
            var traineesOfClassAvailable = _unitOfWork.Trainee.GetAllByCondition(t => otherClassIds.Contains(t.ClassId)).ToList();

            if (!traineesOfClassAvailable.Any())
            {
                return new ResponseDTO("Các lớp có thể chuyển vào đã bị lỗi", 400, false);
            }

            var availableClasses = new List<Class>();
            foreach (var classId in otherClassIds)
            {
                var currentClassToCheck = await _unitOfWork.Class.GetByCondition(c => c.ClassId.Equals(classId)); // Giả sử bạn có GetById
                if (currentClassToCheck == null) continue; // Lớp có thể đã bị xóa.
                var traineesInClass = traineesOfClassAvailable.Where(t => t.ClassId == classId).ToList();
                if (!traineesInClass.Any())
                {
                    // Lớp không có học viên, bỏ qua
                    continue;
                }
                var groupCounts = traineesInClass.Where(t => t.GroupNo.HasValue).GroupBy(t => t.GroupNo.Value).ToDictionary(g => g.Key, g => g.Count());

                if (!groupCounts.Any())
                {
                    // Lớp không có nhóm, bỏ qua
                    continue;
                }
                // Tìm kích thước nhóm tối đa
                int maxGroupSize = groupCounts.Values.Max();
                // Kiểm tra xem có nhóm nào còn chỗ trống không
                if (groupCounts.Any(group => group.Value < maxGroupSize))
                {
                    availableClasses.Add(currentClassToCheck);
                }
            }
            if (!availableClasses.Any())
            {
                return new ResponseDTO("Không có lớp nào có nhóm còn chỗ trống.", 400, false);
            }
            List<ClassAvailableDTO> classAvailableDTOs = _mapper.Map<List<ClassAvailableDTO>>(availableClasses);
            foreach (var classAvailableDTO in classAvailableDTOs)
            {
                var lecturer = await _unitOfWork.Account.GetByCondition(a => a.AccountId.Equals(classAvailableDTO.LecturerId));
                if (lecturer == null)
                {
                    return new ResponseDTO("Lỗi giảng viên của lớp học", 500, false);
                }
                classAvailableDTO.LecturerName = lecturer.FullName;
                var lessonClass = _unitOfWork.LessonClass.GetAllByCondition(lsc => lsc.ClassId.Equals(classAvailableDTO.ClassId));
                if (lessonClass == null)
                {
                    return new ResponseDTO("Thông tin buổi học của danh sách lớp học bị lỗi", 500,false);
                }    
                classAvailableDTO.Lessons = _mapper.Map<List<LessonClassOfClassAvailableDTO>> (lessonClass);
            }
            return new ResponseDTO("Lấy danh sách lớp thành công", 200, true, classAvailableDTOs);

        }

    }

}
