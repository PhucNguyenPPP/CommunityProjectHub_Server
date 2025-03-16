using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPH.BLL.Interfaces;
using CPH.Common.DTO.General;
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

            if(account.RoleId == 5)
            {
                int traineeAmount = _unitOfWork.Trainee.GetAll().Count();
                return new ResponseDTO("Lấy tổng số học viên thành công", 200, true, traineeAmount);
            }

            if(account.RoleId == 6)
            {
                return new ResponseDTO("Chưa làm nhưng mà thành công", 200, true);
            }

            return new ResponseDTO("Lấy tổng số học viên không thành công", 400, false);
        }


    }
}
