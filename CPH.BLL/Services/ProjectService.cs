using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CPH.BLL.Interfaces;
using CPH.Common.Constant;
using CPH.Common.DTO.Account;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Lesson;
using CPH.Common.DTO.Paging;
using CPH.Common.DTO.Project;
using CPH.Common.Enum;
using CPH.DAL.Entities;
using CPH.DAL.UnitOfWork;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;

namespace CPH.BLL.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAccountService _accountService;
        public ProjectService(IUnitOfWork unitOfWork, IMapper mapper, IAccountService accountService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _accountService = accountService;
        }

        public async Task<ResponseDTO> CheckProjectExisted(Guid projectID)
        {
            try
            {
                var project = await _unitOfWork.Project
                    .GetByCondition(c => c.Status == true && c.ProjectId.Equals(projectID));
                if (project == null)
                {
                    return new ResponseDTO("Dự án cộng đồng không tồn tại", 404, false);
                }
                return new ResponseDTO("Dự án cộng đồng tồn tại", 200, true, project);
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
                project.Status = true;
                project.CreatedDate=DateTime.Now;
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
                List<ImportTraineeDTO> importTraineeDTOs = (List<ImportTraineeDTO>)responseDTO.Result;
                List<string> classCodes = importTraineeDTOs.Select(c => c.ClassCode).Distinct().ToList();
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
                await _unitOfWork.LessonClass.AddRangeAsync(lessonClasses);
                List<Trainee> trainees = _mapper.Map<List<Trainee>>(importTraineeDTOs);
                for (var i = 0; i < trainees.Count; i++)
                {
                    var c = await _unitOfWork.Class.GetByCondition(c => c.ClassCode.Equals(importTraineeDTOs[i].ClassCode));
                    trainees[i].ClassId = c.ClassId;
                    trainees[i].TraineeId = Guid.NewGuid();
                    var a = await _unitOfWork.Account.GetByCondition(a => a.Email.Equals(importTraineeDTOs[i].Email));
                    trainees[i].AccountId = a.AccountId;
                }                
                for (var i = 0; i < classId.Count; i++)
                {
                    int temp = 0;
                    int groupNo = 1;
                    List<Trainee> traineeClass = trainees.Where(t => t.ClassId.Equals(classId[i])).ToList();
                    for (var j = 0;j<traineeClass.Count();j++)
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
                        foreach(var t in trainees)
                        {
     
                            if (t.TraineeId.Equals(traineeClass[j].TraineeId))
                            {
                                t.GroupNo = traineeClass[j].GroupNo;
                                break;
                            }    
                        }    
                    }
                   
                }
                await _unitOfWork.Trainee.AddRangeAsync(trainees);
                var r = await _unitOfWork.SaveChangeAsync();
                if (!r)
                {
                    return new ResponseDTO("Tạo project thất bại", 500, false);
                }
               
                return new ResponseDTO("Tạo project thành công", 200, true);
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
                if (projectDTO.StartDate < DateTime.Now)
                {
                    errors.Add("Thời gian bắt đầu của dự án phải ở tương lai");
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
                if (projectDTO.ApplicationEndDate > projectDTO.EndDate)
                {
                    errors.Add("Thời gian hết hạn ứng tuyển không được xa hơn thời gian kết thúc dự án");
                }
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
                var response = await _accountService.ImportTraineeFromExcel(projectDTO.Trainees);
                if (response.IsSuccess)
                {
                    var listTrainee = (List<ImportTraineeDTO>)response.Result;
                    if (listTrainee != null)
                    {
                        if (projectDTO.NumberTraineeEachGroup > listTrainee.Count)
                        {
                            errors.Add("Số học viên mỗi nhóm không thể lớn hơn tổng số học viên");
                        }
                    }
     
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
                    if(errors.Count>0)
                    {
                        return new ResponseDTO("Thông tin dự án không hợp lệ", 400, false, errors);
                    }    
                    return new ResponseDTO("Thông tin dự án hợp lệ", 200, true, listTrainee);
                }
                return response;
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
                IQueryable<Project> list = _unitOfWork.Project
                    .GetAllByCondition(c => c.Status == true)
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


        public async Task<ResponseDTO> GetAllRelatedProject(string? searchValue, int? pageNumber, int? rowsPerPage, string? filterField, string? filterOrder, Guid userId)
        {
            try
            {
                IQueryable<Project> list = _unitOfWork.Project
                    .GetAllByCondition(c => c.Status == true &&
                        (c.ProjectManagerId == userId ||
                        c.Classes.Any(cl => cl.LecturerId == userId) ||
                        c.Classes.Any(c => c.Members.Any(mem => mem.AccountId == userId))))
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
                .Include(cl => cl.Classes)
                .Include(l => l.Lessons).ThenInclude(lcl => lcl.LessonClasses).FirstOrDefault();
            if (project == null)
            {
                return new ResponseDTO("Không tìm thấy dự án tương ứng", 400, false);
            }
            var projectDTO = _mapper.Map<ProjectDetailDTO>(project);
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
                project.Status = false;
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
    }
}
