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
            int lectureAmount = _unitOfWork.Class.GetAllByCondition(c=> c.LecturerId != null).Count();
            return new ResponseDTO("Lấy tổng số giảng viên giảng dạy thành công", 200, true, lectureAmount);
        }
        

        public async Task<ResponseDTO> GetAllNumberOfTrainee(Guid accountId)
        {
            var account = await _unitOfWork.Account.GetByCondition(c => c.AccountId == accountId);
            if (account == null)
            {
                return new ResponseDTO("Người dùng không tồn tại", 400, false);
            }

            if(account.Role.RoleName == RoleEnum.BusinessRelation.ToString() ||
               account.Role.RoleName == RoleEnum.DepartmentHead.ToString())
            {
                int traineeAmount = _unitOfWork.Trainee.GetAll().Count();
                return new ResponseDTO("Lấy tổng số học viên thành công", 200, true, traineeAmount);
            }

            if(account.Role.RoleName == RoleEnum.Associate.ToString())
            {
                return new ResponseDTO("Chưa làm nhưng mà thành công", 200, true);
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

            if (account.Role.RoleName == RoleEnum.Lecturer.ToString()) // số dự án làm dưới role PM
            {
                int projectAmount = _unitOfWork.Project.GetAllByCondition(c=> c.ProjectManagerId == accountId).Count();
                return new ResponseDTO("Lấy tổng số dự án thành công", 200, true, projectAmount);
            }

            if (account.Role.RoleName == RoleEnum.BusinessRelation.ToString() || 
                account.Role.RoleName == RoleEnum.DepartmentHead.ToString())
            {
                int projectAmount = _unitOfWork.Project.GetAll().Count();
                return new ResponseDTO("Lấy tổng số dự án thành công", 200, true, projectAmount);
            }

            if (account.Role.RoleName == RoleEnum.Associate.ToString())
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

            if (account.RoleId == 4 || account.RoleId == 6)
            {
                var project = _unitOfWork.Project.GetAll().ToList();
                var list = CompleteList(project);
                return new ResponseDTO("Lấy tổng số dự án thành công", 200, true, list);
            }


            if (account.RoleId == 5)
            {
                return new ResponseDTO("Chưa làm nhưng mà thành công", 200, true);
            }

            return new ResponseDTO("Lấy tổng số dự án không thành công", 400, false);
        }

    }
}
