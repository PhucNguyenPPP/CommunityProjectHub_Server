using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CPH.BLL.Interfaces;
using CPH.Common.Constant;
using CPH.Common.DTO.Class;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Paging;
using CPH.Common.DTO.Project;
using CPH.Common.DTO.Registration;
using CPH.Common.Enum;
using CPH.Common.Notification;
using CPH.DAL.Entities;
using CPH.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;

namespace CPH.BLL.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        public RegistrationService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
        }

        public async Task<ResponseDTO> CancelRegistration(Guid cancelRegistrationId)
        {
            var check = await CheckCancel(cancelRegistrationId);
            if (check.IsSuccess == false)
            {
                return check;
            }
            Registration re = (Registration)check.Result;
            if (re != null)
            {
                re.Status = RegistrationStatusConstant.Canceled;
                _unitOfWork.Registration.Update(re);
                var canceled = await _unitOfWork.SaveChangeAsync();
                if (canceled)
                {
                    return new ResponseDTO("Huỷ đơn đăng ký thành công", 200, true);
                }
            }
            return new ResponseDTO("Huỷ đơn đăng ký thất bại", 500, true);

        }

        private async Task<ResponseDTO> CheckCancel(Guid cancelRegistrationId)
        {
            if (cancelRegistrationId == Guid.Empty)
            {
                return new ResponseDTO("Vui lòng nhập Id của đơn đăng ký muốn huỷ", 400, false);
            }
            var re = await _unitOfWork.Registration.GetByCondition(r => r.RegistrationId.Equals(cancelRegistrationId));
            if (re == null)
            {
                return new ResponseDTO("Đơn đăng ký không tồn tại", 404, false);
            }
            if (!re.Status.Equals(RegistrationStatusConstant.Processing))
            {
                return new ResponseDTO("Đơn đăng ký không thể huỷ", 400, false);
            }
            return new ResponseDTO("Hợp lệ", 200, true, re);
        }

        public async Task<ResponseDTO> SendRegistration(SendRegistrationDTO registrationDTO)
        {
            var check = await CheckRegistrationValid(registrationDTO);
            if (check.IsSuccess == false)
            {
                return check;
            }
            Registration registration = _mapper.Map<Registration>(registrationDTO);
            registration.RegistrationId = Guid.NewGuid();
            registration.Status = RegistrationStatusConstant.Processing;
            registration.CreatedAt = DateTime.Now;
            await _unitOfWork.Registration.AddAsync(registration);

            //Create notification
            var sender = await _unitOfWork.Account.GetByCondition(c => c.AccountId == registrationDTO.AccountId);
            var classRegistration = _unitOfWork.Class
                .GetAllByCondition(c => c.ClassId == registrationDTO.ClassId)
                .Include(c => c.Project)
                .ThenInclude(c => c.ProjectManager)
                .FirstOrDefault();

            var messageNotification = RegistrationNotification.SendRegistrationNotification(sender!.FullName, classRegistration!.ClassCode, classRegistration!.Project.Title);
            if (classRegistration!.Project.ProjectManagerId != null)
            {
                await _notificationService.CreateNotification((Guid)classRegistration!.Project.ProjectManagerId, messageNotification);
            };

            //End of create notification

            var added = await _unitOfWork.SaveChangeAsync();
            if (added)
            {
                return new ResponseDTO("Đăng ký thành công", 200, true);
            }
            return new ResponseDTO("Đăng ký thất bại", 500, false);
        }

        private async Task<ResponseDTO> CheckRegistrationValid(SendRegistrationDTO registrationDTO)
        {
            List<string> listError = new List<string>();

            var cl = await _unitOfWork.Class.GetByCondition(c => c.ClassId.Equals(registrationDTO.ClassId));
            if (cl == null)
            {
                listError.Add("Lớp học hiện không thể đăng ký tham gia");
            }
            else
            {
                var project = await _unitOfWork.Project.GetByCondition(p => p.ProjectId.Equals(cl.ProjectId));
                if (project == null)
                {
                    listError.Add("Dự án không tồn tại");
                }

                else
                {
                    if (!project.Status.Equals(ProjectStatusConstant.UpComing))
                    {
                        listError.Add("Không thể đăng ký vào lớp của dự án có trạng thái: " + project.Status.ToString());
                    }
                    if (project.ApplicationStartDate > DateTime.Now || project.ApplicationEndDate < DateTime.Now)
                    {
                        listError.Add("Dự án hiện không trong thời gian đăng ký");
                    }
                    else
                    {
                        var acc = await _unitOfWork.Account.GetByCondition(a => a.AccountId.ToString() == registrationDTO.AccountId.ToString());
                        if (acc == null)
                        {
                            listError.Add("Tài khoản không tồn tại");
                        }
                        else
                        {
                            var regis = await _unitOfWork.Registration.GetByCondition(r => r.AccountId.Equals(registrationDTO.AccountId)
                            && r.ClassId.Equals(registrationDTO.ClassId)
                            && (r.Status.Equals(RegistrationStatusConstant.Processing) || r.Status.Equals(RegistrationStatusConstant.Inspected)));
                            if (regis != null)
                            {
                                listError.Add("Bạn đã từng đăng ký tham gia lớp học này");
                            }
                            else
                            { /*
                                var classOfAcc = _unitOfWork.Registration.GetAllByCondition(r => r.AccountId.ToString().Equals(registrationDTO.AccountId.ToString()) &&
                                (r.Status.Equals(RegistrationStatusConstant.Processing) || r.Status.Equals(RegistrationStatusConstant.Inspected))).Select(r => r.ClassId).ToList();
                                if (classOfAcc != null)
                                {
                                    var lscToRegister = _unitOfWork.LessonClass.GetAllByCondition(lsc => lsc.ClassId.Equals(registrationDTO.ClassId)); //đang đky
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
                                                        listError.Add("Lớp học trùng lịch với buổi học " + l.LessonNo.ToString() + " của lớp " + c.ClassCode + " bạn đăng ký trước đó");
                                                    }
                                                }
                                            }
                                        }
                                    } 
                                }*/

                                if (acc.RoleId.Equals((int)RoleEnum.Lecturer))
                                {
                                    if (cl.LecturerId.HasValue)
                                    {
                                        listError.Add("Lớp đã đủ giảng viên");
                                    }
                                    else
                                    {
                                        var classOfAcc = _unitOfWork.Registration.GetAllByCondition(r => r.AccountId.ToString().Equals(registrationDTO.AccountId.ToString()) &&
                               r.Status.Equals(RegistrationStatusConstant.Processing)).Select(r => r.ClassId).ToList();
                                        var cla = _unitOfWork.Class.GetAllByCondition(c => c.LecturerId.Equals(registrationDTO.AccountId)).Select(c => c.ClassId).ToList();
                                        if (cla.Count > 0)
                                        {
                                            classOfAcc.AddRange(cla);
                                        }
                                        var classAct = _unitOfWork.Class.GetAllByCondition(c => c.Project.Status.Equals(ProjectStatusConstant.UpComing) || c.Project.Status.Equals(ProjectStatusConstant.InProgress)).ToList();
                                        var classActivate = classAct.Where(c => classOfAcc.Contains(c.ClassId)).Select(c => c.ClassId).ToList();
                                        if (classActivate != null)
                                        {
                                            var lscToRegister = _unitOfWork.LessonClass.GetAllByCondition(lsc => lsc.ClassId.Equals(registrationDTO.ClassId)); //đang đky

                                            for (int i = 0; i < classActivate.Count(); i++)
                                            {
                                                if (classActivate[i].Equals(registrationDTO.ClassId))
                                                {
                                                    listError.Add("Bạn đã đăng ký hoặc được phân công vào lớp này trước đó");
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
                                                            listError.Add("Lớp học trùng lịch với buổi học " + l.LessonNo.ToString() + " của lớp " + c.ClassCode + " mà " + acc.FullName + " đăng ký trước đó");
                                                        }
                                                    }
                                                }

                                            }
                                        }
                                    }
                                }
                                else if (acc.RoleId.Equals((int)RoleEnum.Student))
                                {
                                    if (!cl.LecturerId.HasValue || cl.NumberGroup == null)
                                    {
                                        listError.Add("Sinh viên hỗ trợ chỉ có thể đăng ký lớp đã có giảng viên và được chia nhóm");
                                    }
                                    var regisOfClass = _unitOfWork.Registration.GetAllByCondition(r => r.ClassId.Equals(registrationDTO.ClassId) && r.Status.Equals(RegistrationStatusConstant.Inspected));
                                    if (regisOfClass.Count() >= cl.NumberGroup)
                                    {
                                        listError.Add("Lớp đã đủ sinh viên hỗ trợ");
                                    }
                                    var pros = _unitOfWork.Project.GetAllByCondition(p => p.Status.Equals(ProjectStatusConstant.UpComing) || p.Status.Equals(ProjectStatusConstant.InProgress)).Select(p => p.ProjectId);
                                    List<Guid> classOfAcc = _unitOfWork.Registration.GetAllByCondition(r => r.AccountId.ToString().Equals(registrationDTO.AccountId.ToString()) &&
                                                   r.Status.Equals(RegistrationStatusConstant.Processing) || r.Status.Equals(RegistrationStatusConstant.Inspected)).Select(r => r.ClassId).ToList();
                                    var mem = _unitOfWork.Member.GetAllByCondition(m => m.AccountId.Equals(registrationDTO.AccountId)).Select(m => m.ClassId).ToList();
                                    classOfAcc.AddRange(mem);
                                    var classAct = _unitOfWork.Class.GetAllByCondition(c => pros.Contains(c.ProjectId)).ToList();
                                    var classActivate = classAct.Where(c => classOfAcc.Contains(c.ClassId)).Select(c => c.ClassId).ToList();
                                    var lscToRegister = _unitOfWork.LessonClass.GetAllByCondition(lsc => lsc.ClassId.Equals(registrationDTO.ClassId)); //đang đky
                                    if (classActivate != null)
                                    {
                                        for (int i = 0; i < classActivate.Count(); i++)
                                        {
                                            if (classActivate[i].Equals(registrationDTO.ClassId))
                                            {
                                                listError.Add("Bạn đã đăng ký hoặc được phân công vào lớp này trước đó");
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
                                                        listError.Add("Lớp học trùng lịch với buổi học " + l.LessonNo.ToString() + " của lớp " + c.ClassCode + " mà " + acc.FullName + " đăng ký trước đó");
                                                    }
                                                }
                                            }
                                        }

                                    }
                                }
                                else
                                {
                                    listError.Add("Tài khoản không thể đăng ký vào dự án");
                                }
                            }
                        }
                    }
                }
            }
            if (listError.Count > 0)
            {
                return new ResponseDTO("Thông tin đăng ký tham gia không hợp lệ", 400, false, listError);
            }
            return new ResponseDTO("Thông tin đăng ký tham gia hợp lệ", 200, true);
        }

        public async Task<ResponseDTO> AnswerRegistration(AnswerRegistrationDTO answerRegistrationDTO)
        {
            var check = await CheckAnswer(answerRegistrationDTO);
            if (check.IsSuccess == false)
            {
                return check;
            }
            Registration re = (Registration)check.Result;
            var accountId = re.AccountId;
            var clasRegis = await _unitOfWork.Class.GetByCondition(c => c.ClassId.Equals(re.ClassId));
            if (re != null && clasRegis != null)
            {
                if (answerRegistrationDTO.Type.Equals("Approve"))
                {
                    re.Status = RegistrationStatusConstant.Inspected;
                    var acc = await _unitOfWork.Account.GetByCondition(a => a.AccountId.Equals(re.AccountId));

                    if (acc.RoleId.Equals((int)RoleEnum.Lecturer))
                    {
                        if (clasRegis.LecturerId.HasValue)
                        {
                            return new ResponseDTO("Lớp " + clasRegis.ClassCode + " đã đủ giảng viên", 400, false);
                        }
                        else
                        {
                            clasRegis.LecturerId = re.AccountId;
                            _unitOfWork.Class.Update(clasRegis);
                            ProjectLogging logging = new ProjectLogging()
                            {
                                ProjectNoteId = Guid.NewGuid(),
                                ActionDate = DateTime.Now,
                                ProjectId = clasRegis.ProjectId,
                                ActionContent = $"{acc.FullName} đã trở thành giảng viên của lớp {clasRegis.ClassCode}",
                                AccountId = acc.AccountId,

                            };
                            await _unitOfWork.ProjectLogging.AddAsync(logging);
                        }
                    }
                    else if (acc.RoleId.Equals((int)RoleEnum.Student))
                    {
                        var stu = _unitOfWork.Member.GetAllByCondition(m => m.ClassId.Equals(re.ClassId));
                        if (clasRegis.NumberGroup.HasValue)
                        {
                            if (stu.Count() >= clasRegis.NumberGroup)
                            {
                                return new ResponseDTO("Lớp " + clasRegis.ClassCode + " đã đủ sinh viên hỗ trợ", 400, false);
                            }
                            else
                            {
                                Member mem = new Member()
                                {
                                    ClassId = re.ClassId,
                                    AccountId = re.AccountId,
                                    GroupSupportNo = stu.Count() + 1,
                                    MemberId = Guid.NewGuid(),

                                };
                                await _unitOfWork.Member.AddAsync(mem);
                                ProjectLogging logging = new ProjectLogging()
                                {
                                    ProjectNoteId = Guid.NewGuid(),
                                    ActionDate = DateTime.Now,
                                    ProjectId = clasRegis.ProjectId,
                                    ActionContent = $"{acc.FullName} đã tham gia hỗ trợ lớp {clasRegis.ClassCode}",
                                    AccountId = acc.AccountId,

                                };
                                await _unitOfWork.ProjectLogging.AddAsync(logging);
                            }

                        }
                    }
                }
                else
                {
                    re.Status = RegistrationStatusConstant.Rejected;
                }
                _unitOfWork.Registration.Update(re);

                //Create notification
                var classRegistration = clasRegis.ClassCode;
                var prjRegistration = clasRegis.Project.Title;
                string messageNotification = null!;
                if (answerRegistrationDTO.Type.Equals("Approve"))
                {
                    messageNotification = RegistrationNotification.AnswerRegistrationNotification("được duyệt", classRegistration, prjRegistration);
                }
                else
                {
                    messageNotification = RegistrationNotification.AnswerRegistrationNotification("bị từ chối", classRegistration, prjRegistration);
                }
                await _notificationService.CreateNotification(accountId, messageNotification);


                //End of create notification
                var ans = await _unitOfWork.SaveChangeAsync();
                if (ans)
                {
                    return new ResponseDTO("Trả lời đơn đăng ký thành công", 200, true);

                }
            }
            return new ResponseDTO("Trả lời đơn đăng ký thất bại", 500, true);
        }

        private async Task<ResponseDTO> CheckAnswer(AnswerRegistrationDTO answerRegistrationDTO)
        {
            var re = await _unitOfWork.Registration.GetByCondition(r => r.RegistrationId.Equals(answerRegistrationDTO.RegistrationId));
            if (re == null)
            {
                return new ResponseDTO("Đơn đăng ký không tồn tại", 404, false);
            }
            if (re.Status != RegistrationStatusConstant.Processing)
            {
                return new ResponseDTO("Không thể trả lời đơn đăng ký với trạng thái là: " + re.Status, 400, false);
            }
            if (answerRegistrationDTO.Type != "Approve" && answerRegistrationDTO.Type != "Deny")
            {
                return new ResponseDTO("Phần trả lời đơn đăng ký không hợp lệ", 400, false);
            }
            var classOfRe = await _unitOfWork.Class.GetByCondition(c => c.ClassId.Equals(re.ClassId));
            if (classOfRe == null)
            {
                return new ResponseDTO("Lớp đăng ký vào đã bị lỗi", 400, false);
            }
            var project = await _unitOfWork.Project.GetByCondition(p => p.ProjectId.Equals(classOfRe.ProjectId));
            if (project == null)
            {
                return new ResponseDTO("Dự án đăng ký vào không tồn tại", 400, false);
            }
            if (project.Status != ProjectStatusConstant.UpComing || project.ApplicationEndDate < DateTime.Now)
            {
                return new ResponseDTO("Dự án đã quá hạn trả lời", 400, false);
            }

            return new ResponseDTO("Thông tin trả lời đơn đăng ký hợp lệ", 200, true, re);
        }

        public async Task<ResponseDTO> GetAllSentRegistrations(Guid accountId, string search, int? rowsPerPage, int? pageNumber)
        {
            List<SentRegistDTO>? listDTO = new List<SentRegistDTO>();
            var check = await CheckAccountToGetRegistrations(accountId);
            if (!check.IsSuccess)
            {
                return check;
            }
            var regis = _unitOfWork.Registration.GetAllByCondition(r => r.AccountId.Equals(accountId)).Include(r => r.Class).ToList();
            listDTO = _mapper.Map<List<SentRegistDTO>>(regis);
            if (listDTO == null)
            {
                return new ResponseDTO("Tài khoản không có đơn đăng ký", 404, false);
            }

            for (int i = 0; i < listDTO.Count; i++)
            {
                var projectTtile = await _unitOfWork.Project.GetByCondition(c => c.ProjectId.Equals(listDTO[i].ProjectId));
                if (projectTtile != null)
                {

                    listDTO[i].Title = projectTtile.Title;
                }
            }


            if (search.IsNullOrEmpty() && pageNumber == null && rowsPerPage == null)
            {

                return new ResponseDTO("Hiển thị thông tin các đơn đăng ký của tài khoản hợp lệ", 200, true, listDTO);
            }
            else
            {
                if (search != null)
                {
                    if (listDTO != null) // Kiểm tra null trước khi lọc
                    {
                        listDTO = listDTO.Where(c =>
                            (!string.IsNullOrEmpty(c.Title) && c.Title.ToLower().Contains(search.ToLower())) ||
                            (!string.IsNullOrEmpty(c.ClassCode) && c.ClassCode.ToLower().Contains(search.ToLower()))

                        ).ToList();
                    }
                }
                if (listDTO == null || !listDTO.Any())
                {
                    return new ResponseDTO("Không có đơn đăng ký trùng khớp", 404, false);
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


                if (pageNumber != null && rowsPerPage != null)
                {
                    var pagedList = PagedList<SentRegistDTO>.ToPagedList(listDTO.AsQueryable(), pageNumber, rowsPerPage);
                    var result = new ListSentRegisDTO
                    {
                        Registered = pagedList,
                        CurrentPage = pageNumber,
                        RowsPerPages = rowsPerPage,
                        TotalCount = listDTO.Count,
                        TotalPages = (int)Math.Ceiling(listDTO.Count / (double)rowsPerPage)
                    };
                    return new ResponseDTO("Tìm kiếm đơn đăng ký thành công", 200, true, result);
                }
                return new ResponseDTO("Tìm kiếm đơn đăng ký thành công", 200, true, listDTO);

            }
        }


        private async Task<ResponseDTO> CheckAccountToGetRegistrations(Guid accountId)
        {
            if (accountId.ToString().IsNullOrEmpty())
            {
                return new ResponseDTO("Vui lòng nhập Id của tài khoản", 400, false);
            }
            var acc = await _unitOfWork.Account.GetByCondition(a => a.AccountId.Equals(accountId));
            if (acc == null)
            {
                return new ResponseDTO("Tài khoản không tồn tại", 404, false);
            }
            if (!acc.RoleId.Equals((int)RoleEnum.Student) && !acc.RoleId.Equals((int)RoleEnum.Lecturer))
            {
                return new ResponseDTO("Tài khoản không thể nộp đơn đăng ký", 400, false);
            }
            return new ResponseDTO("Id tài khoản hợp lệ", 200, true);
        }

        public async Task<ResponseDTO> GetRegistrationsOfProject(Guid projectId, string search, int? rowsPerPage, int? pageNumber)
        {
            List<RegistrationsOfProjectDTO> listDTO = new List<RegistrationsOfProjectDTO>();
            if (projectId.ToString().IsNullOrEmpty())
            {
                return new ResponseDTO("Vui lòng nhập Id của dự án", 400, false);
            }
            var project = await _unitOfWork.Project.GetByCondition(a => a.ProjectId.Equals(projectId));
            if (project == null)
            {
                return new ResponseDTO("Dự án không tồn tại", 404, false);
            }
            List<Guid> cla = _unitOfWork.Class.GetAllByCondition(c => c.ProjectId.Equals(projectId)).Select(c => c.ClassId).ToList();
            var regis = _unitOfWork.Registration.GetAll()
                            .Include(r => r.Account).ThenInclude(a => a.Role)
                            .Include(r => r.Class).ThenInclude(c => c.Project)
                            .ToList();
            regis = regis.Where(r => cla.Contains(r.ClassId)).ToList();
            listDTO = _mapper.Map<List<RegistrationsOfProjectDTO>>(regis);
            if (listDTO == null)
            {
                return new ResponseDTO("Dự án không có đơn đăng ký", 404, false);
            }
            if (search.IsNullOrEmpty() && pageNumber == null && rowsPerPage == null)
            {

                return new ResponseDTO("Hiển thị thông tin các đơn đăng ký của dự án hợp lệ", 200, true, listDTO);
            }
            else
            {
                if (search != null)
                {
                    if (listDTO != null) // Kiểm tra null trước khi lọc
                    {
                        listDTO = listDTO.Where(c =>
        ((!string.IsNullOrEmpty(c.ClassCode) && c.ClassCode.ToLower().Contains(search.ToLower())) ||
        (!string.IsNullOrEmpty(c.FullName) && c.FullName.ToLower().Contains(search.ToLower())) ||
        (!string.IsNullOrEmpty(c.Email) && c.Email.ToLower().Contains(search.ToLower())) ||
        (!string.IsNullOrEmpty(c.AccountCode) && c.AccountCode.ToLower().Contains(search.ToLower())) ||
        (!string.IsNullOrEmpty(c.Title) && c.Title.ToLower().Contains(search.ToLower())) // Project Title
    )
).ToList();
                    }
                }
                if (listDTO == null || !listDTO.Any())
                {
                    return new ResponseDTO("Không có đơn đăng ký trùng khớp", 404, false);
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
                if (pageNumber != null && rowsPerPage != null)
                {
                    var pagedList = PagedList<RegistrationsOfProjectDTO>.ToPagedList(listDTO.AsQueryable(), pageNumber, rowsPerPage);
                    var result = new ListRegistrationOfProjectDTO
                    {
                        Registrations = pagedList,
                        CurrentPage = pageNumber,
                        RowsPerPages = rowsPerPage,
                        TotalCount = listDTO.Count,
                        TotalPages = (int)Math.Ceiling(listDTO.Count / (double)rowsPerPage)
                    };
                    return new ResponseDTO("Tìm kiếm đơn đăng ký thành công", 200, true, result);
                }
                return new ResponseDTO("Tìm kiếm đơn đăng ký thành công", 200, true, listDTO);
            }
        }
    }
}
