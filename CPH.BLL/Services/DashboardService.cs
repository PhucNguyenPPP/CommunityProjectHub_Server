using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPH.BLL.Interfaces;
using CPH.Common.Constant;
using CPH.Common.DTO.Dashboard;
using CPH.Common.DTO.General;
using CPH.Common.Enum;
using CPH.DAL.Entities;
using CPH.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace CPH.BLL.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDTO> GetAllNumberOfStudent()
        {
            int studentAmount = _unitOfWork.Member.GetAll().Count();
            return new ResponseDTO("Lấy tổng số sinh viên hỗ trợ thành công", 200, true, studentAmount);
        }

        public async Task<ResponseDTO> GetAllNumberOfLecturer()
        {
            int lectureAmount = _unitOfWork.Class.GetAllByCondition(c => c.LecturerId != null).Count();
            return new ResponseDTO("Lấy tổng số giảng viên giảng dạy thành công", 200, true, lectureAmount);
        }


        public async Task<ResponseDTO> GetAllNumberOfTrainee(Guid accountId)
        {
            var account = await _unitOfWork.Account.GetByCondition(c => c.AccountId == accountId);
            if (account == null)
            {
                return new ResponseDTO("Người dùng không tồn tại", 400, false);
            }

            if (account.RoleId == 6 || account.RoleId == 4)
            {
                int traineeAmount = _unitOfWork.Trainee.GetAll().Count();
                return new ResponseDTO("Lấy tổng số học viên thành công", 200, true, traineeAmount);
            }

            if (account.RoleId == 5)
            {
                int traineeAmmount = _unitOfWork.Trainee.GetAllByCondition(c=> c.Class.Project.AssociateId == accountId).Count();
                return new ResponseDTO("Lấy tổng số học viên thành công", 200, true, traineeAmmount);
            }

            return new ResponseDTO("Lấy tổng số học viên không thành công", 400, false);
        }

        public async Task<ResponseDTO> GetAllNumberOfProject(Guid accountId)
        {
            var account = await _unitOfWork.Account.GetByCondition(c => c.AccountId == accountId);
            if (account == null)
            {
                return new ResponseDTO("Người dùng không tồn tại", 400, false);
            }

            if (account.RoleId == 2) // số dự án làm dưới role PM
            {
                int projectAmount = _unitOfWork.Project.GetAllByCondition(c => c.ProjectManagerId == accountId).Count();
                return new ResponseDTO("Lấy tổng số dự án thành công", 200, true, projectAmount);
            }

            if (account.RoleId == 4 || account.RoleId == 6 || account.RoleId == 7)
            {
                int projectAmount = _unitOfWork.Project.GetAll().Count();
                return new ResponseDTO("Lấy tổng số dự án thành công", 200, true, projectAmount);
            }

            if (account.RoleId == 5)
            {
                return new ResponseDTO("Chưa làm nhưng mà thành công", 200, true);
            }

            return new ResponseDTO("Lấy tổng số dự án không thành công", 400, false);
        }

        public List<NumberOfProjectWithStatusDTO> CompleteList(List<Project> project)
        {
            List<NumberOfProjectWithStatusDTO> numberOfProjectWithStatusDTOs = new List<NumberOfProjectWithStatusDTO>
            {
                new NumberOfProjectWithStatusDTO { Type = ProjectStatusConstant.Planning, Amount = project.Count(c => c.Status.Equals(ProjectStatusConstant.Planning)) },
                new NumberOfProjectWithStatusDTO { Type = ProjectStatusConstant.UpComing, Amount = project.Count(c => c.Status.Equals(ProjectStatusConstant.UpComing)) },
                new NumberOfProjectWithStatusDTO { Type = ProjectStatusConstant.InProgress, Amount = project.Count(c => c.Status.Equals(ProjectStatusConstant.InProgress)) },
                new NumberOfProjectWithStatusDTO { Type = ProjectStatusConstant.Completed, Amount = project.Count(c => c.Status.Equals(ProjectStatusConstant.Completed)) },
                new NumberOfProjectWithStatusDTO { Type = ProjectStatusConstant.Cancelled, Amount = project.Count(c => c.Status.Equals(ProjectStatusConstant.Cancelled)) }
            };

            return numberOfProjectWithStatusDTOs;
        }

        public async Task<ResponseDTO> GetAllNumberOfProjectWithStatus(Guid accountId)
        {
            var account = await _unitOfWork.Account.GetByCondition(c => c.AccountId == accountId);
            if (account == null)
            {
                return new ResponseDTO("Người dùng không tồn tại", 400, false);
            }

            if (account.RoleId == 2) // số dự án làm dưới role PM
            {
                var project = _unitOfWork.Project.GetAllByCondition(c => c.ProjectManagerId == accountId).ToList();
                var list = CompleteList(project);
                return new ResponseDTO("Lấy tổng số dự án thành công", 200, true, list);
            }

            if (account.RoleId == 4 || account.RoleId == 6 || account.RoleId == 7)
            {
                var project = _unitOfWork.Project.GetAll().ToList();
                var list = CompleteList(project);
                return new ResponseDTO("Lấy tổng số dự án thành công", 200, true, list);
            }


            if (account.RoleId == 5)
            {
                return new ResponseDTO("Chưa làm nhưng mà thành công", 200, true, new List<NumberOfProjectWithStatusDTO>());
            }

            return new ResponseDTO("Lấy tổng số dự án không thành công", 400, false);
        }

        public ResponseDTO GetAllNumberOfUser()
        {
            var numberUser = _unitOfWork.Account.GetAllByCondition(c => c.RoleId == (int)RoleEnum.Student
            || c.RoleId == (int)RoleEnum.Lecturer
            || c.RoleId == (int)RoleEnum.Trainee
            || c.RoleId == (int)RoleEnum.Associate).ToList().Count();
            return new ResponseDTO("Lấy tổng số người dùng thành công", 200, true, numberUser);
        }

        public ResponseDTO GetAllNumberOfUserByRole()
        {
            List<NumberOfUserByRoleDTO> list = new List<NumberOfUserByRoleDTO>();

            var numberStudent = _unitOfWork.Account.GetAllByCondition(c => c.RoleId == (int)RoleEnum.Student)
                .ToList()
                .Count();
            list.Add(new NumberOfUserByRoleDTO
            {
                Type = "Sinh viên",
                Amount = numberStudent,
            });

            var numberLecturer = _unitOfWork.Account.GetAllByCondition(c => c.RoleId == (int)RoleEnum.Lecturer)
                .ToList()
                .Count();
            list.Add(new NumberOfUserByRoleDTO
            {
                Type = "Giảng viên",
                Amount = numberLecturer,
            });

            var numberTrainee = _unitOfWork.Account.GetAllByCondition(c => c.RoleId == (int)RoleEnum.Trainee)
               .ToList()
               .Count();
            list.Add(new NumberOfUserByRoleDTO
            {
                Type = "Học viên",
                Amount = numberTrainee,
            });

            var numberAssociate = _unitOfWork.Account.GetAllByCondition(c => c.RoleId == (int)RoleEnum.Associate)
               .ToList()
               .Count();
            list.Add(new NumberOfUserByRoleDTO
            {
                Type = "Đối tác",
                Amount = numberAssociate,
            });

            return new ResponseDTO("Lấy tất cả người dùng theo role thành công", 200, true, list);
        }

        public async Task<ResponseDTO> GetProgressOfAllProject(Guid accountId)
        {
            var account = await _unitOfWork.Account.GetByCondition(c => c.AccountId == accountId);
            if (account == null)
            {
                return new ResponseDTO("Người dùng không tồn tại", 400, false);
            }

            if (account.RoleId == (int)RoleEnum.Lecturer)
            {
                var projectList = _unitOfWork.Project.GetAllByCondition(c => c.ProjectManagerId == accountId)
                    .Include(c => c.Classes)
                    .ThenInclude(c => c.LessonClasses)
                    .ToList();

                List<ProjectProgressDTO> list = new List<ProjectProgressDTO>();
                foreach (var project in projectList)
                {
                    var classList = projectList.SelectMany(c => c.Classes);
                    int totalLessonClass = 0;
                    int totalPassedLessonClass = 0;
                    foreach (var classObj in classList)
                    {
                        totalLessonClass += classObj.LessonClasses.Count();
                        totalPassedLessonClass += classObj.LessonClasses.Where(c => c.EndTime < DateTime.Now).Count();
                    }
                    double percentageProgress = totalPassedLessonClass * 100 / totalLessonClass;
                    list.Add(new ProjectProgressDTO
                    {
                        ProjectName = project.Title,
                        Percentage = percentageProgress,
                        ProjectStatus = project.Status,
                    });
                }
                return new ResponseDTO("Lấy tiến độ dự án thành công", 200, true, list);
            }

            if (account.RoleId == (int)RoleEnum.DepartmentHead || account.RoleId == (int)RoleEnum.BusinessRelation)
            {
                var projectList = _unitOfWork.Project.GetAll()
                   .Include(c => c.Classes)
                   .ThenInclude(c => c.LessonClasses)
                   .ToList();

                List<ProjectProgressDTO> list = new List<ProjectProgressDTO>();
                foreach (var project in projectList)
                {
                    var classList = projectList.SelectMany(c => c.Classes);
                    int totalLessonClass = 0;
                    int totalPassedLessonClass = 0;
                    foreach (var classObj in classList)
                    {
                        totalLessonClass += classObj.LessonClasses.Count();
                        totalPassedLessonClass += classObj.LessonClasses.Where(c => c.EndTime < DateTime.Now).Count();
                    }
                    double percentageProgress = totalPassedLessonClass * 100 / totalLessonClass;
                    list.Add(new ProjectProgressDTO
                    {
                        ProjectName = project.Title,
                        Percentage = percentageProgress,
                        ProjectStatus = project.Status,
                    });
                }
                return new ResponseDTO("Lấy tiến độ dự án thành công", 200, true, list);
            }

            if (account.RoleId == (int)RoleEnum.Associate)
            {
                var projectList = _unitOfWork.Project.GetAllByCondition(c => c.AssociateId == accountId)
                    .Include(c => c.Classes)
                    .ThenInclude(c => c.LessonClasses)
                    .ToList();

                List<ProjectProgressDTO> list = new List<ProjectProgressDTO>();
                foreach (var project in projectList)
                {
                    var classList = projectList.SelectMany(c => c.Classes);
                    int totalLessonClass = 0;
                    int totalPassedLessonClass = 0;
                    foreach (var classObj in classList)
                    {
                        totalLessonClass += classObj.LessonClasses.Count();
                        totalPassedLessonClass += classObj.LessonClasses.Where(c => c.EndTime < DateTime.Now).Count();
                    }
                    double percentageProgress = totalPassedLessonClass * 100 / totalLessonClass;
                    list.Add(new ProjectProgressDTO
                    {
                        ProjectName = project.Title,
                        Percentage = percentageProgress,
                        ProjectStatus = project.Status,
                    });
                }
                return new ResponseDTO("Lấy tiến độ dự án thành công", 200, true, list);
            }

            return new ResponseDTO("Lấy tổng số dự án không thành công", 400, false);
        }
    }
}
