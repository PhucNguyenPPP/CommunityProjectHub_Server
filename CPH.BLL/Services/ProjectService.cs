using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Execution;
using CPH.BLL.Interfaces;
using CPH.Common.Constant;
using CPH.Common.DTO.Account;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Lesson;
using CPH.Common.DTO.Paging;
using CPH.Common.DTO.Project;
using CPH.Common.Enum;
using CPH.Common.Notification;
using CPH.DAL.Entities;
using CPH.DAL.UnitOfWork;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart.ChartEx;

namespace CPH.BLL.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAccountService _accountService;
        private readonly INotificationService _notificationService;
        public ProjectService(IUnitOfWork unitOfWork, IMapper mapper, IAccountService accountService,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _accountService = accountService;
            _notificationService = notificationService;
        }

        public async Task<ResponseDTO> CheckProjectExisted(Guid projectID)
        {
            try
            {
                var project = await _unitOfWork.Project
                    .GetByCondition(c => c.ProjectId.Equals(projectID));
                /*c => c.Status != ProjectStatusConstant.Cancelled && c.Status!=ProjectStatusConstant.InProgress && c.Status!=ProjectStatusConstant.Completed && 
                                var project = await _unitOfWork.Project
                                    .GetByCondition(c => c.ProjectId.Equals(projectID));*/
                if (project == null)
                {
                    return new ResponseDTO("Dự án cộng đồng không tồn tại", 404, false);
                }
                if (project.Status.Equals(ProjectStatusConstant.Planning) || project.Status.Equals(ProjectStatusConstant.UpComing))
                {
                    return new ResponseDTO("Dự án cộng đồng có thể huỷ", 200, true, project);
                }
                return new ResponseDTO("Dự án cộng đồng không thể huỷ", 400, false);
            }
            catch (Exception ex)
            {
                return new ResponseDTO(ex.Message, 500, false);
            }

        }

        public async Task<ResponseDTO> CreateProject(NewProjectDTO projectDTO)
        {
            try
            {
                ResponseDTO responseDTO = await CheckNewProject(projectDTO);
                if (!responseDTO.IsSuccess)
                {
                    return responseDTO;
                }
                Guid projectId = Guid.NewGuid();
                List<Lesson> lessons = new List<Lesson>();
                Project project = _mapper.Map<Project>(projectDTO);
                project.ProjectId = projectId;
                project.NumberLesson = projectDTO.LessonList.Count;
                project.Status = ProjectStatusConstant.Planning;
                project.CreatedDate = DateTime.Now;
                await _unitOfWork.Project.AddAsync(project);
                for (int i = 0; i < projectDTO.LessonList.Count; i++)
                {
                    var ls = new Lesson
                    {
                        LessonId = Guid.NewGuid(),
                        LessonContent = projectDTO.LessonList[i],
                        ProjectId = projectId,
                        LessonNo = i + 1
                    };
                    lessons.Add(ls);
                }
                await _unitOfWork.Lesson.AddRangeAsync(lessons);
              /*// List<ImportTraineeDTO> importTraineeDTOs = (List<ImportTraineeDTO>)responseDTO.Result;
                List<string> classCodes = importTraineeDTOs.Select(c => c.ClassCode).Distinct().ToList();
                List<Trainee> trainees = _mapper.Map<List<Trainee>>(importTraineeDTOs);
                List<Guid> classId = new List<Guid>();
                List<Class> list = new List<Class>();
                for (var i = 0; i < classCodes.Count; i++)
                {
                    var c = new DAL.Entities.Class();
                    c.ClassId = Guid.NewGuid();
                    c.ProjectId = projectId;
                    c.ClassCode = classCodes[i];
                    list.Add(c);
                    classId.Add(c.ClassId);
                }
                await _unitOfWork.Class.AddRangeAsync(list);
                for (var i = 0; i < trainees.Count; i++)
                {
                    var c = list.Where(c => c.ClassCode.Equals(importTraineeDTOs[i].ClassCode)).FirstOrDefault();
                    trainees[i].ClassId = c.ClassId;
                    trainees[i].TraineeId = Guid.NewGuid();
                }

                await _unitOfWork.Class.AddRangeAsync(list);
                List<LessonClass> lessonClasses = new List<LessonClass>();
                foreach (var cl in list)
                {
                    foreach (var l in lessons)
                    {
                        LessonClass lessonClass = new LessonClass
                        {
                            LessonClassId = Guid.NewGuid(),
                            LessonId = l.LessonId,
                            ClassId = cl.ClassId,
                        };
                        lessonClasses.Add(lessonClass);
                    }
                }
                await _unitOfWork.LessonClass.AddRangeAsync(lessonClasses); */
                /*
                for (var i = 0; i < classId.Count; i++)
                {
                    int temp = 0;
                    int groupNo = 1;
                    List<Trainee> traineeClass = trainees.Where(t => t.ClassId.Equals(classId[i])).ToList();
                    for (var j = 0; j < traineeClass.Count(); j++)
                    {
                        if (temp < projectDTO.NumberTraineeEachGroup)
                        {
                            traineeClass[j].GroupNo = groupNo;
                            temp++;
                        }
                        else
                        {
                            temp = 1;
                            groupNo++;
                            traineeClass[j].GroupNo = groupNo;
                        }
                        foreach (var t in trainees)
                        {

                            if (t.TraineeId.Equals(traineeClass[j].TraineeId))
                            {
                                t.GroupNo = traineeClass[j].GroupNo;
                                break;
                            }
                        }
                    }
                

                }
                
                await _unitOfWork.Trainee.AddRangeAsync(trainees); */
                if (projectDTO.ProjectManagerId != null)
                {
                    var projectManager = await _unitOfWork.Account.GetByCondition(a => a.AccountId == projectDTO.ProjectManagerId && a.RoleId.Equals((int)RoleEnum.Lecturer));
                    ProjectLogging logging = new ProjectLogging()
                    {
                        ProjectNoteId = Guid.NewGuid(),
                        ActionDate = DateTime.Now,
                        ProjectId = projectId,
                        ActionContent = $"{projectManager.FullName} được bổ nhiệm thành quản lý của dự án {projectDTO.Title}",
                        AccountId = (Guid)projectDTO.ProjectManagerId,

                    };
                    await _unitOfWork.ProjectLogging.AddAsync(logging);
                }
                var r = await _unitOfWork.SaveChangeAsync();
                if (!r)
                {
                    return new ResponseDTO("Tạo dự án thất bại", 500, false);
                }

                return new ResponseDTO("Tạo dự án thành công", 201, true);
            }
            catch (Exception ex)
            {
                return new ResponseDTO(ex.Message.ToString(), 500, false);
            }
        }

        private async Task<ResponseDTO> CheckNewProject(NewProjectDTO projectDTO)
        {
            try
            {
                List<string> errors = new List<string>();
                if (projectDTO.StartDate <= projectDTO.ApplicationEndDate)
                {
                    errors.Add("Thời gian bắt đầu của dự án phải sau khi kết thúc thời gian ứng tuyển");
                }
                if (projectDTO.EndDate < projectDTO.StartDate)
                {
                    errors.Add("Thời gian kết thúc phải xa hơn thời gian bắt đầu");
                }
                if (projectDTO.ApplicationStartDate < DateTime.Now)
                {
                    errors.Add("Thời gian bắt đầu ứng tuyển vào dự án phải ở tương lai");
                }
                if (projectDTO.ApplicationEndDate < projectDTO.ApplicationStartDate)
                {
                    errors.Add("Thời gian hết hạn ứng tuyển phải xa hơn thời gian bắt đầu ứng tuyển");
                }
                /*   if (projectDTO.ApplicationEndDate > projectDTO.EndDate)
                   {
                       errors.Add("Thời gian hết hạn ứng tuyển không được xa hơn thời gian kết thúc dự án");
                   }*/
                var projectName = await _unitOfWork.Project.GetByCondition(c => c.Title == projectDTO.Title);
                if (projectName != null)
                {
                    errors.Add("Tên dự án đã tồn tại");
                }
                if (projectDTO.ProjectManagerId != null)
                {
                    var projectManager = await _unitOfWork.Account.GetByCondition(a => a.AccountId == projectDTO.ProjectManagerId && a.RoleId.Equals((int)RoleEnum.Lecturer));
                    if (projectManager == null)
                    {
                        errors.Add("Thông tin người quản lý dự án không hợp lệ");
                    }
                }
                var associate = await _unitOfWork.Associate.GetByCondition(a=>a.AccountId.Equals(projectDTO.AssociateId));
                if (associate == null)
                {
                    errors.Add("Thông tin đối tác dự án không hợp lệ");
                }
                if(projectDTO.MinLessonTime>projectDTO.MaxLessonTime)
                {
                    errors.Add("Thời lượng tối thiểu tối đa không hợp lệ");
                }    
                /*
                var response = await _accountService.ImportTraineeFromExcel(projectDTO.Trainees);
                if (!response.IsSuccess)
                {

                    //  {
                    /*     var c = listTrainee.Select(t => t.ClassCode).Distinct().ToList();
                        for (int i = 0; i < c.Count; i++)
                        {
                            var numOfTraineeClass = listTrainee.Where(t => t.ClassCode.Equals(c[i])).Count();
                            if (numOfTraineeClass < projectDTO.NumberTraineeEachGroup)
                            {
                                errors.Add("Số lượng học viên mỗi nhóm không thể lớn hơn tổng số học viên ở từng lớp");
                            }

                        }
                    }
                    else
                    {
                    if(response.Result!=null)
                    {
                        List<string> strings= (List<string>) response.Result;
                        foreach (var item in strings)
                        {
                           errors.Add(item.ToString());
                        }
                    }
                    else
                    {
                        errors.Add(response.Message.ToString());
                    }
                }
                var listTrainee = (List<ImportTraineeDTO>)response.Result;
            */
                for (int i = 0; i < projectDTO.LessonList.Count; i++)
                {
                    for (int j = 0; j < projectDTO.LessonList.Count; j++)
                    {
                        if (projectDTO.LessonList[i] == projectDTO.LessonList[j] && i != j)
                        {
                            errors.Add("Có 2 bài học nội dung" + projectDTO.LessonList.ToString() + " trùng nhau");
                        }
                    }
                }
                if (errors.Count > 0)
                {
                    return new ResponseDTO("Thông tin dự án không hợp lệ", 400, false, errors);
                }
                return new ResponseDTO("Thông tin dự án hợp lệ", 200, true);

            }
            catch (Exception ex)
            {
                return new ResponseDTO(ex.Message.ToString(), 500, false);
            }
        }



        public async Task<ResponseDTO> GetAllProject(string? searchValue, int? pageNumber, int? rowsPerPage, string? filterField, string? filterOrder)
        {
            try
            {
                //IQueryable<Project> list = _unitOfWork.Project
                //    .GetAllByCondition(c => c.Status == true)
                //    .Include(c => c.Classes).ThenInclude(c => c.Lecturer)
                //    .Include(c => c.ProjectManager);

                IQueryable<Project> list = _unitOfWork.Project
                    .GetAll()
                    .Include(c => c.Classes).ThenInclude(c => c.Lecturer)
                    .Include(c => c.ProjectManager);
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
                    if (pageNumber != null && rowsPerPage != null)
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


        public async Task<ResponseDTO> GetAllRelatedProject(string? searchValue, int? pageNumber, int? rowsPerPage, string? filterField, string? filterOrder, Guid userId)
        {
            try
            {
                var account = _unitOfWork.Account.GetAllByCondition(c => c.AccountId == userId).FirstOrDefault();
                if(account == null)
                {
                    return new ResponseDTO("Người dùng không tồn tại", 400, false);
                }

                IQueryable<Project> list = _unitOfWork.Project
                    .GetAllByCondition(c =>
                        (c.ProjectManagerId == userId ||
                        c.AssociateId == userId ||
                        c.Classes.Any(cl => cl.LecturerId == userId) ||
                        c.Classes.Any(c => c.Members.Any(mem => mem.AccountId == userId)) ||
                        c.Classes.Any(c => c.Trainees.Any(tra => tra.AccountId == userId))))
                    .Include(c => c.Classes).ThenInclude(c => c.Lecturer)
                    .Include(c => c.ProjectManager);

                if (!list.Any())
                {
                    return new ResponseDTO("Không có dự án trùng khớp", 400, false);
                }
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
                    if (pageNumber != null && rowsPerPage != null)
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

        public async Task<ResponseDTO> GetProjectDetail(Guid projectId)
        {
            var project = _unitOfWork.Project
                .GetAllByCondition(c => c.ProjectId == projectId)
                .Include(pm => pm.ProjectManager)
                .Include(ass => ass.Associate)
                    .ThenInclude(c => c.Associate)
                .Include(cl => cl.Classes)
                    .ThenInclude(tr => tr.Trainees)
                .Include(cl => cl.Classes)
                    .ThenInclude(c => c.Members)
                .Include(cl => cl.Classes)
                    .ThenInclude(tc => tc.Lecturer)
                .Include(l => l.Lessons)
                    .ThenInclude(lcl => lcl.LessonClasses)
                .FirstOrDefault();

            if (project == null)
            {
                return new ResponseDTO("Không tìm thấy dự án tương ứng", 400, false);
            }
            var projectDTO = _mapper.Map<ProjectDetailDTO>(project);

            var classList = project.Classes.ToList();
            foreach (var classItem in classList)
            {
                if (classItem.LecturerId != null)
                {
                    if (!projectDTO.LecturerIds.Contains((Guid)classItem.LecturerId))
                    {
                        projectDTO.LecturerIds.Add((Guid)classItem.LecturerId);
                    }
                }
                var memberList = classItem.Members.ToList();
                foreach (var member in memberList)
                {
                    if (!projectDTO.MemberIds.Contains(member.AccountId))
                    {
                        projectDTO.MemberIds.Add(member.AccountId);
                    }
                }
            }

            return new ResponseDTO("Lấy thông tin dự án thành công", 200, true, projectDTO);
        }

        public async Task<ResponseDTO> InActivateProject(Guid projectID)
        {
            try
            {
                var check = await CheckProjectExisted(projectID);
                if (!check.IsSuccess)
                {
                    return check;
                }
                var project = (Project)check.Result;
                project.Status = ProjectStatusConstant.Cancelled;
                _unitOfWork.Project.Update(project);
                var updated = await _unitOfWork.SaveChangeAsync();
                if (!updated)
                {
                    return new ResponseDTO("Vô hiệu hoá dự án thất bại", 500, false);
                }
                return new ResponseDTO("Vô hiệu hoá dự án thành công", 200, true);
            }
            catch (Exception ex)
            {
                return new ResponseDTO(ex.Message, 500, false);
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
                    : list.OrderBy(c => c.CreatedDate)
            };
        }

        public async Task<ResponseDTO> UpdateProject(UpdateProjectDTO projectDTO)
        {
            try
            {
                var check = await CheckProjectExisted(projectDTO.ProjectId);
                if (check.IsSuccess == false)
                {
                    return new ResponseDTO("Dự án không tồn tại", 404, false);
                }
                ResponseDTO responseDTO = await CheckUpdateProject(projectDTO);
                if (!responseDTO.IsSuccess)
                {
                    return responseDTO;
                }
                Project project = (Project)check.Result;
                project.Title = projectDTO.Title;
                project.Description = projectDTO.Description;
                project.Address = projectDTO.Address;
                project.AssociateId = projectDTO.AssociateId;
                var cl = _unitOfWork.Class.GetAllByCondition(c => c.ProjectId.Equals(projectDTO.ProjectId)).Select(c => c.ClassId).Distinct().ToList();
                if (cl == null)
                {
                    return new ResponseDTO("Lớp của dự án bị lỗi", 500, false);
                }

                //if (project.NumberTraineeEachGroup != projectDTO.NumberTraineeEachGroup)
                //{
                //    for (var i = 0; i < cl.Count; i++)
                //    {
                //        int temp = 0;
                //        int groupNo = 1;
                //        List<Trainee> traineeClass = _unitOfWork.Trainee.GetAllByCondition(t => t.ClassId.Equals(cl[i])).ToList();
                //        for (var j = 0; j < traineeClass.Count(); j++)
                //        {
                //            if (temp < projectDTO.NumberTraineeEachGroup)
                //            {
                //                traineeClass[j].GroupNo = groupNo;
                //                temp++;
                //            }
                //            else
                //            {
                //                temp = 1;
                //                groupNo++;
                //                traineeClass[j].GroupNo = groupNo;
                //            }
                //        }
                //        _unitOfWork.Trainee.UpdateRange(traineeClass);
                //    }
                //    project.NumberTraineeEachGroup = projectDTO.NumberTraineeEachGroup;
                //}
                project.StartDate = projectDTO.StartDate;
                project.EndDate = projectDTO.EndDate;
                project.ApplicationStartDate = projectDTO.ApplicationStartDate;
                project.ApplicationEndDate = projectDTO.ApplicationEndDate;
                /*       for (var i = 0; i < projectDTO.LessonList.Count; i++)
                       {
                           Lesson lessonToUpdate = await _unitOfWork.Lesson.GetByCondition(l => l.LessonNo.Equals(i + 1) && !l.LessonContent.Equals(projectDTO.LessonList[i]) && l.ProjectId.Equals(project.ProjectId));
                           Lesson lessonCorrected = await _unitOfWork.Lesson.GetByCondition(l => l.LessonNo.Equals(i + 1) && l.LessonContent.Equals(projectDTO.LessonList[i]) && l.ProjectId.Equals(project.ProjectId));
                           if (lessonToUpdate != null)
                           {
                               lessonToUpdate.LessonContent = projectDTO.LessonList[(int)i];
                               _unitOfWork.Lesson.Update(lessonToUpdate);
                           }
                           else if (lessonCorrected == null)
                           {
                               var lessonid = Guid.NewGuid();
                               var ls = new Lesson
                               {
                                   LessonId = lessonid,
                                   LessonContent = projectDTO.LessonList[i],
                                   ProjectId = projectDTO.ProjectId,
                                   LessonNo = i + 1
                               };
                               await _unitOfWork.Lesson.AddAsync(ls);
                               for (var j = 0; j < cl.Count; j++)
                               {
                                   var lsc = new LessonClass
                                   {
                                       LessonClassId = Guid.NewGuid(),
                                       LessonId = lessonid,
                                       ClassId = cl[j],
                                   };
                                   await _unitOfWork.LessonClass.AddAsync(lsc);
                               }

                           }
                       }
                       var lessonToDel = _unitOfWork.Lesson.GetAllByCondition(l => l.ProjectId.Equals(project.ProjectId) && l.LessonNo > projectDTO.LessonList.Count).ToList();
                       if (lessonToDel.Count > 0)
                       {
                           foreach (var l in lessonToDel)
                           {
                               var lsclass = _unitOfWork.LessonClass.GetAllByCondition(lsc => lsc.LessonId.Equals(l.LessonId)).ToList();   
                               foreach (var lsc in lsclass)
                               {
                                   _unitOfWork.LessonClass.Delete(lsc);
                               }
                               _unitOfWork.Lesson.Delete(l);
                           }

                       }*/
                _unitOfWork.Project.Update(project);
                var updated = await _unitOfWork.SaveChangeAsync();
                if (updated == false)
                {
                    return new ResponseDTO("Chỉnh sửa thất bại", 500, false);
                }
                return new ResponseDTO("Chỉnh sửa thành công", 200, true);
            }
            catch (Exception ex)
            {
                return new ResponseDTO(ex.Message, 500, false);
            }

            //   return new ResponseDTO("", 500, false);
        }

        private async Task<ResponseDTO> CheckUpdateProject(UpdateProjectDTO projectDTO)
        {
            try
            {
                var project = await _unitOfWork.Project.GetByCondition(p => p.ProjectId.Equals(projectDTO.ProjectId));
                if (!project.Status.Equals(ProjectStatusConstant.Planning))
                {
                    return new ResponseDTO("Dự án hiện đang ở giai đoạn " + project.Status.ToString() + " nên không thể chỉnh sửa", 400, false);
                }

                List<string> errors = new List<string>();
                if (projectDTO.StartDate < projectDTO.ApplicationEndDate)
                {
                    errors.Add("Thời gian bắt đầu của dự án phải sau khi kết thúc thời gian ứng tuyển");
                }
                if (projectDTO.EndDate < projectDTO.StartDate)
                {
                    errors.Add("Thời gian kết thúc phải xa hơn thời gian bắt đầu");
                }
                // fix để không phải sửa DB trong lúc demo
                //if (projectDTO.ApplicationStartDate < DateTime.Now)
                //{
                //    errors.Add("Thời gian bắt đầu ứng tuyển vào dự án phải ở tương lai");
                //}
                if (projectDTO.ApplicationEndDate < projectDTO.ApplicationStartDate)
                {
                    errors.Add("Thời gian hết hạn ứng tuyển phải xa hơn thời gian bắt đầu ứng tuyển");
                }
                var projectName = await _unitOfWork.Project.GetByCondition(c => c.Title == projectDTO.Title && c.ProjectId != projectDTO.ProjectId);
                if (projectName != null)
                {
                    errors.Add("Tên dự án đã tồn tại");
                }
                var associate = await _unitOfWork.Associate.GetByCondition(a => a.AccountId.Equals(projectDTO.AssociateId));
                if (associate == null)
                {
                    errors.Add("Thông tin đối tác dự án không hợp lệ");
                }
                var c = _unitOfWork.Class.GetAllByCondition(t => t.ProjectId.Equals(projectDTO.ProjectId)).Select(t => t.ClassId).Distinct().ToList();
                //for (int i = 0; i < c.Count; i++)
                //{
                //    var numOfTraineeClass = _unitOfWork.Trainee.GetAllByCondition(t => t.ClassId.Equals(c[i])).Count();
                //    if (numOfTraineeClass < projectDTO.NumberTraineeEachGroup)
                //    {
                //        errors.Add("Số lượng học viên mỗi nhóm không thể lớn hơn tổng số học viên ở từng lớp");
                //    }
                //}
                if (errors.Count > 0)
                {
                    return new ResponseDTO("Thông tin dự án không hợp lệ", 400, false, errors);
                }
                return new ResponseDTO("Thông tin dự án hợp lệ", 200, true);

            }
            catch (Exception ex)
            {
                return new ResponseDTO(ex.Message.ToString(), 500, false);
            }
        }

        public async Task<ResponseDTO> GetAvailableProject(Guid userId, string? searchValue, int? pageNumber, int? rowsPerPage, string? filterField, string? filterOrder)
        {
            try
            {
                var role = _unitOfWork.Account.GetAllByCondition(c => c.AccountId == userId).Select(c => c.Role.RoleName).FirstOrDefault();
                IQueryable<Project> list;

                if (role.IsNullOrEmpty())
                {
                    return new ResponseDTO("Người dùng không tồn tại", 400, false);
                }

                else if (role == RoleEnum.Student.ToString() || role == RoleEnum.Lecturer.ToString())
                {
                    if (role == RoleEnum.Lecturer.ToString())
                    {
                        list = _unitOfWork.Project.GetAllByCondition(c => c.Status == ProjectStatusConstant.UpComing
                            && c.ProjectManagerId != userId
                            && !c.Classes.Any(cls => cls.LecturerId == userId)
                            && c.Classes.Any(cls => cls.LecturerId == null))
                            .Include(c => c.Classes).ThenInclude(c => c.Lecturer)
                            .Include(c => c.ProjectManager)
                            .Where(c => c.ApplicationStartDate <= DateTime.Now
                                && c.ApplicationEndDate >= DateTime.Now);
                    }
                    else
                    {
                        list = _unitOfWork.Project.GetAllByCondition(c => c.Status == ProjectStatusConstant.UpComing
                            && !c.Classes.All(c => c.NumberGroup == null)
                            && !c.Classes.Any(cls => cls.Members != null && cls.Members.Any(m => m.AccountId == userId))
                            && c.Classes.Any(cls => cls.Members == null || cls.Members.Count() < cls.NumberGroup))
                            .Include(c => c.Classes).ThenInclude(c => c.Lecturer)
                            .Include(c => c.ProjectManager)
                            .Where(c => c.ApplicationStartDate <= DateTime.Now
                                && c.ApplicationEndDate >= DateTime.Now);
                    }
                }
                else
                {
                    return new ResponseDTO("Không có quyền truy cập", 400, false);
                }

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
                    if (pageNumber != null && rowsPerPage != null)
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

        public async Task UpdateProjectsStatusToInProgress()
        {
            List<Project> projectsToUpdate = await GetProjectsWithStartDateNow();
            if (projectsToUpdate.Count > 0)
            {
                foreach (var item in projectsToUpdate)
                {
                    item.Status = ProjectStatusConstant.InProgress;
                    var classesOfProject = await _unitOfWork.Class.GetAllByCondition(c => c.ProjectId.Equals(item.ProjectId)).ToListAsync();

                    // Kiểm tra xem có lớp học nào không đáp ứng điều kiện hay không
                    bool hasInvalidClass = classesOfProject.Any(c => !c.LecturerId.HasValue || _unitOfWork.Member.GetAllByCondition(m => m.ClassId.Equals(c.ClassId)).Count() < c.NumberGroup);

                    if (!hasInvalidClass)
                    {
                        var classIdsOfProject = classesOfProject.Select(c => c.ClassId).ToList();
                        // Tối ưu hóa truy vấn regis bằng cách thêm điều kiện lọc vào truy vấn ban đầu
                        var registrations = await _unitOfWork.Registration.GetAllByCondition(r => classIdsOfProject.Contains(r.ClassId) && r.Status.Equals(RegistrationStatusConstant.Processing)).ToListAsync();

                        foreach (var reg in registrations)
                        {
                            reg.Status = RegistrationStatusConstant.Rejected;
                        }
                    }
                    else
                    {
                        item.Status = ProjectStatusConstant.Cancelled;
                    }

                    _unitOfWork.Project.UpdateRange(new List<Project> { item }); // Cập nhật từng dự án riêng biệt
                    bool saveChanges = await _unitOfWork.SaveChangeAsync();

                    if (!saveChanges)
                    {
                        throw new Exception($"Không thể cập nhật trạng thái dự án {item.ProjectId}.");
                    }

                }
            }
        }
        private async Task<List<Project>> GetProjectsWithStartDateNow()
        {
            var today = DateTime.Now; // Sử dụng UTC để tránh vấn đề múi giờ
            return await _unitOfWork.Project
                .GetAllByCondition(p => p.StartDate <= today && p.Status.Equals(ProjectStatusConstant.UpComing))
                .ToListAsync();
        }

        public async Task<ResponseDTO> UpdateProjectStatusUpcoming(Guid projectId)
        {
            var project = _unitOfWork.Project
                .GetAllByCondition(c => c.ProjectId == projectId)
                .Include(c => c.Lessons)
                .ThenInclude(c => c.LessonClasses)
                .Include(c => c.Classes)
                .FirstOrDefault();

            if (project == null)
            {
                return new ResponseDTO("Dự án không tồn tại", 400, false);
            }

            if (project.Status != ProjectStatusConstant.Planning)
            {
                return new ResponseDTO("Dự án phải đang ở trạng thái Lên kế hoạch", 400, false);
            }

            if (project.ProjectManagerId == null)
            {
                return new ResponseDTO("Dự án chưa có người quản lý", 400, false);
            }

            var lessonIds = project.Lessons.Select(l => l.LessonId).ToList();
            var filteredLessonClasses = _unitOfWork.LessonClass
                .GetAllByCondition(lc => lessonIds.Contains(lc.LessonId))
                .Include(c => c.Class)
                .ToList();

            foreach (var i in filteredLessonClasses)
            {
                if (i.StartTime == null || i.EndTime == null || i.Room == null)
                {
                    return new ResponseDTO($"Lớp {i.Class.ClassCode} chưa được xếp lịch", 400, false);
                }
            }

            //var classList = project.Classes.ToList();
            //foreach (var i in classList)
            //{
            //    if (i.NumberGroup == null)
            //    {
            //        return new ResponseDTO($"Lớp {i.ClassCode} chưa được tạo nhóm", 400, false);
            //    }
            //}

            if(project.MaxAbsentPercentage == null)
            {
                return new ResponseDTO("Dự án chưa được cập nhật phần trăm vắng mặt tối đa", 400, false);
            }

            if (project.FailingScore == null)
            {
                return new ResponseDTO("Dự án chưa được cập nhật điểm liệt", 400, false);
            }

            if (project.StartDate < DateTime.Now)
            {
                return new ResponseDTO("Đã quá ngày bắt đầu của dự án", 400, false);
            }

            //if (project.ApplicationStartDate < DateTime.Now)
            //{
            //    return new ResponseDTO("Đã quá ngày bắt đầu mở đăng ký của dự án", 400, false);
            //}

            project.Status = ProjectStatusConstant.UpComing;
            _unitOfWork.Project.Update(project);

            var result = await _unitOfWork.SaveChangeAsync();
            if (result)
            {
                return new ResponseDTO("Dự án đã chuyển sang giai đoạn Sắp diễn ra", 200, true);
            }
            return new ResponseDTO("Cập nhật dự án thất bại", 500, false);
        }

        public async Task<ResponseDTO> UpdateProjectStatusInProgress(Guid projectId)
        {
            var project = _unitOfWork.Project
                .GetAllByCondition(c => c.ProjectId == projectId)
                .FirstOrDefault();

            if (project == null)
            {
                return new ResponseDTO("Dự án không tồn tại", 400, false);
            }

            if (project.Status != ProjectStatusConstant.UpComing)
            {
                return new ResponseDTO("Dự án phải đang ở trạng thái Sắp diễn ra", 400, false);
            }

            project.Status = ProjectStatusConstant.InProgress;
            _unitOfWork.Project.Update(project);

            var result = await _unitOfWork.SaveChangeAsync();
            if (result)
            {
                return new ResponseDTO("Dự án đã chuyển sang giai đoạn Đang diễn ra", 200, true);
            }
            return new ResponseDTO("Cập nhật dự án thất bại", 500, false);
        }

        public async Task<ResponseDTO> UpdateProjectStatusEnd(Guid projectId)
        {
            var project = _unitOfWork.Project
                .GetAllByCondition(c => c.ProjectId == projectId)
                .FirstOrDefault();

            if (project == null)
            {
                return new ResponseDTO("Dự án không tồn tại", 400, false);
            }

            if (project.Status != ProjectStatusConstant.InProgress)
            {
                return new ResponseDTO("Dự án phải đang ở trạng thái Đang diễn ra", 400, false);
            }

            project.Status = ProjectStatusConstant.Completed;
            _unitOfWork.Project.Update(project);

            var result = await _unitOfWork.SaveChangeAsync();
            if (result)
            {
                return new ResponseDTO("Dự án đã chuyển sang giai đoạn Kết thúc", 200, true);
            }
            return new ResponseDTO("Cập nhật dự án thất bại", 500, false);
        }

        public async Task<ResponseDTO> AssignPMToProject(Guid projectId, Guid accountId)
        {
            var project = _unitOfWork.Project.GetAllByCondition(c => c.ProjectId == projectId)
                .Include(c => c.ProjectManager)
                .FirstOrDefault();
            if (project == null)
            {
                return new ResponseDTO("Dự án không tồn tại", 400, false);
            }

            var projectManager = _unitOfWork.Account.GetAllByCondition(c => c.AccountId == accountId)
                .FirstOrDefault();

            if (projectManager == null)
            {
                return new ResponseDTO("Giảng viên không tồn tại", 400, false);
            }

            if (projectManager.RoleId != (int)RoleEnum.Lecturer)
            {
                return new ResponseDTO("Chỉ giảng viên mới có thể được bổ nhiệm làm quản lý dự án", 400, false);
            }

            if (project.Status == ProjectStatusConstant.Completed || project.Status == ProjectStatusConstant.Cancelled)
            {
                return new ResponseDTO("Dự án đã kết thúc", 400, false);
            }

            if (project.ProjectManagerId == accountId)
            {
                return new ResponseDTO("Giảng viên được chọn đang là quản lý của dự án", 400, false);
            }

            List<ProjectLogging> projectLoggings = new List<ProjectLogging>();

            if (project.ProjectManagerId != null)
            {
                var messageNotificationRemovePM = RemovePMFromProjectNotification.SendRemovePMFromProjectNotification(project.Title);
                await _notificationService.CreateNotification((Guid)project.ProjectManagerId, messageNotificationRemovePM);

                ProjectLogging loggingRemovePM = new ProjectLogging()
                {
                    ProjectNoteId = Guid.NewGuid(),
                    ActionDate = DateTime.Now,
                    ProjectId = projectId,
                    ActionContent = $"{project.ProjectManager!.FullName} không còn là quản lý của dự án {project.Title}",
                    AccountId = (Guid)project.ProjectManagerId,
                };
                projectLoggings.Add(loggingRemovePM);
            }


            var messageNotification = AssignPMToProjectNotification.SendAssignPMToProjectNotification(project.Title);
            await _notificationService.CreateNotification(accountId, messageNotification);

            ProjectLogging loggingAssignPM = new ProjectLogging()
            {
                ProjectNoteId = Guid.NewGuid(),
                ActionDate = DateTime.Now,
                ProjectId = projectId,
                ActionContent = $"{projectManager.FullName} được bổ nhiệm thành quản lý của dự án {project.Title}",
                AccountId = accountId,
            };
            projectLoggings.Add(loggingAssignPM);

            await _unitOfWork.ProjectLogging.AddRangeAsync(projectLoggings);

            project.ProjectManagerId = accountId;
            _unitOfWork.Project.Update(project);
            var result = await _unitOfWork.SaveChangeAsync();
            if (result)
            {
                return new ResponseDTO("Bổ nhiệm quản lý dự án thành công", 200, true);
            }
            return new ResponseDTO("Bổ nhiệm quản lý dự án thất bại", 400, false);
        }

        public async Task UpdateProjectsStatusToCompleted()
        {
            List<Project> projectsToUpdate = await GetProjectsWithEndDateNow();
            if (projectsToUpdate.Count > 0)
            {
                foreach (var item in projectsToUpdate)
                {
                    item.Status = ProjectStatusConstant.InProgress;
                    var classesOfProject = await _unitOfWork.Class.GetAllByCondition(c => c.ProjectId.Equals(item.ProjectId)).Select(C=>C.ClassId).ToListAsync();

                    // Kiểm tra xem có lớp học nào không đáp ứng điều kiện hay không
                    var trainee = _unitOfWork.Trainee.GetAllByCondition(t => classesOfProject.Contains(t.ClassId) && !t.Score.HasValue);
                    if (trainee.Count()>0)
                    {

                        _unitOfWork.Project.UpdateRange(new List<Project> { item }); // Cập nhật từng dự án riêng biệt
                        bool saveChanges = await _unitOfWork.SaveChangeAsync();

                        if (!saveChanges)
                        {
                            throw new Exception($"Không thể cập nhật trạng thái dự án {item.ProjectId}.");
                        }
                    }
                }
            }
        }

        private async Task<List<Project>> GetProjectsWithEndDateNow()
        {
            var today = DateTime.Now; // Sử dụng UTC để tránh vấn đề múi giờ
            return await _unitOfWork.Project
                .GetAllByCondition(p => p.EndDate <= today && p.Status.Equals(ProjectStatusConstant.InProgress))
                .ToListAsync();
        }

        public MemoryStream ExportFinalReportOfProjectExcel(Guid projectId)
        {
            var classList = _unitOfWork.Class.GetAllByCondition(c => c.ProjectId == projectId).ToList();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                foreach (var classItem in classList)
                {
                    var worksheet = package.Workbook.Worksheets.Add($"{classItem.ClassCode}");

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

                    var traineeList = _unitOfWork.Trainee.GetAllByCondition(c => c.ClassId == classItem.ClassId)
                        .Include(c => c.Account)
                        .OrderBy(c => c.GroupNo)
                        .ToList();
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
                }


                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                return stream;
            }
        }

        public async Task<bool> CheckProjectIdExisted(Guid projectId)
        {
            var project = await _unitOfWork.Project.GetByCondition(c => c.ProjectId == projectId);
            if (project == null)
            {
                return false;
            }
            return true;
        }

        public async Task<ResponseDTO> UpdateMaxAbsentPercentageAndFailingScore(UpdateAbsentPercentageFailingScoreRequestDTO model)
        {
            var project = await _unitOfWork.Project.GetByCondition(c => c.ProjectId == model.ProjectId);
            if(project == null)
            {
                return new ResponseDTO("Dự án không tồn tại", 400, false);
            }

            if(project.Status != ProjectStatusConstant.Planning)
            {
                return new ResponseDTO("Chỉ có thể chỉnh sửa khi dự án đang trong giai đoạn Lên kế hoạch", 400, false);
            }

            if(model.MaxAbsentPercentage > 100 || model.MaxAbsentPercentage < 0)
            {
                return new ResponseDTO("Phần trăm vắng mặt tối đa phải trong khoảng 0 - 100%", 400, false);
            }

            if (model.FailingScore >= 10)
            {
                return new ResponseDTO("Điểm liệt phải nhỏ hơn 10", 400, false);
            }

            if (model.FailingScore < 0)
            {
                return new ResponseDTO("Điểm liệt phải lớn hơn 0", 400, false);
            }

            project.MaxAbsentPercentage = model.MaxAbsentPercentage;
            project.FailingScore = Math.Round(model.FailingScore, 1);
            _unitOfWork.Project.Update(project);
            var result = await _unitOfWork.SaveChangeAsync();
            if (result)
            {
                return new ResponseDTO("Lưu thông tin thành công", 200, true);
            }
            return new ResponseDTO("Lưu thông tin thất bại", 400, false);
        }
    }
}
