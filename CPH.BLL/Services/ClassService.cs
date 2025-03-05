using AutoMapper;
using CPH.BLL.Interfaces;
using CPH.Common.Constant;
using CPH.Common.DTO.Account;
using CPH.Common.DTO.Class;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Member;
using CPH.Common.DTO.Message;
using CPH.Common.DTO.Paging;
using CPH.Common.Enum;
using CPH.Common.Notification;
using CPH.DAL.Entities;
using CPH.DAL.UnitOfWork;
using Microsoft.AspNetCore.Http.HttpResults;
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
            var pro = await _unitOfWork.Project.GetByCondition(p => p.ProjectId.Equals(c.ProjectId));
            if (pro == null)
            {
                return new ResponseDTO("Lớp thuộc dự án bị lỗi", 400, false);
            }
            if (!pro.Status.Equals(ProjectStatusConstant.Planning))
            {
                return new ResponseDTO("Dự án đã ở trạng thâi " + pro.Status.ToString(), 400, false);
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

            var dto = _mapper.Map<ClassDetailDTO>(clas);
            dto.LecturerSlotAvailable = lecturerSlotAvailable;
            dto.StudentSlotAvailable = studentSlotAvailable;
            dto.getMemberOfClassDTOs = memberDto;

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
            if (clas != null && !clas.NumberGroup.HasValue)
            {
                errs.Add("Không thể phân công vào lớp chưa chia nhóm");
            }
            var pro = await _unitOfWork.Project.GetByCondition(p => p.ProjectId.Equals(clas.ProjectId));
            if (pro.Status != ProjectStatusConstant.UpComing && pro.Status != ProjectStatusConstant.InProgress)
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
                var pros = _unitOfWork.Project.GetAllByCondition(p => p.Status.Equals(ProjectStatusConstant.UpComing) && p.Status.Equals(ProjectStatusConstant.InProgress)).Select(p => p.ProjectId);
                var classOfAcc = _unitOfWork.Registration.GetAllByCondition(r => r.AccountId.ToString().Equals(updateClassDTO.AccountId.ToString()) &&
                               r.Status.Equals(RegistrationStatusConstant.Processing)).Select(r => r.ClassId).ToList();
                var cla = _unitOfWork.Class.GetAllByCondition(c => c.LecturerId.Equals(updateClassDTO.AccountId)).Select(c => c.ClassId).ToList();
                if (cla.Count > 0)
                {
                    classOfAcc.AddRange(cla);
                }
                var classActivate = _unitOfWork.Class.GetAllByCondition(c => pros.Contains(c.ProjectId) && classOfAcc.Contains(c.ClassId)).Select(c => c.ClassId).ToList();
                if (classActivate != null)
                {
                    var lscToRegister = _unitOfWork.LessonClass.GetAllByCondition(lsc => lsc.ClassId.Equals(updateClassDTO.ClassId)); //đang đky

                    for (int i = 0; i < classActivate.Count(); i++)
                    {
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
                var pros = _unitOfWork.Project.GetAllByCondition(p => p.Status.Equals(ProjectStatusConstant.UpComing) && p.Status.Equals(ProjectStatusConstant.InProgress)).Select(p => p.ProjectId);
                var classOfAcc = _unitOfWork.Registration.GetAllByCondition(r => r.AccountId.ToString().Equals(updateClassDTO.AccountId.ToString()) &&
                               r.Status.Equals(RegistrationStatusConstant.Processing)).Select(r => r.ClassId).ToList();
                var classActivate = _unitOfWork.Class.GetAllByCondition(c => pros.Contains(c.ProjectId) && classOfAcc.Contains(c.ClassId)).Select(c => c.ClassId).ToList();
                if (classOfAcc != null)
                {
                    var lscToRegister = _unitOfWork.LessonClass.GetAllByCondition(lsc => lsc.ClassId.Equals(updateClassDTO.ClassId)); //đang đky
                    if (classOfAcc != null)
                    {
                        for (int i = 0; i < classOfAcc.Count(); i++)
                        {
                            var lscOfAccRegistered = _unitOfWork.LessonClass.GetAllByCondition(lsc => lsc.ClassId.Equals(classOfAcc[i])).ToList(); //đã đky rồi
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
            }
            if (errs.Count > 0)
            {
                return new ResponseDTO("Thông tin cần cập nhật không hợp lệ", 400, false, errs);
            }
            return new ResponseDTO("Thông tin cần cập nhật hợp lệ", 200, true);
        }
    }
}
