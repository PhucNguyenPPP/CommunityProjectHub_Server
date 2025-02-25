using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CPH.BLL.Interfaces;
using CPH.Common.DTO.General;
using CPH.DAL.Entities;
using CPH.DAL.UnitOfWork;

namespace CPH.BLL.Services
{
    public class TraineeService : ITraineeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TraineeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDTO> GetAllTraineeOfClass(Guid classId, string? searchValue, int? pageNumber, int? rowsPerPage, string? filterField, string? filterOrder)
        {
            var classes = _unitOfWork.Class.GetAllByCondition(c => c.ClassId == classId);
            if (!classes.Any())
            {
                return new ResponseDTO("Lớp không tồn tại", 400, false);
            }

            IQueryable<Trainee> trainees = _unitOfWork.Trainee.GetAllByCondition(c => c.ClassId == classId);
            
            return new ResponseDTO("Lấy danh sách học viên thất bại", 400, false);
        }
    }
}
