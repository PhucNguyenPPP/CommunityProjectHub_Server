using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CPH.BLL.Interfaces;
using CPH.Common.Constant;
using CPH.Common.DTO.General;
using CPH.DAL.UnitOfWork;

namespace CPH.BLL.Services
{
    public class GlobalConstantService: IGlobalConstantService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        public GlobalConstantService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDTO> GetMaxTimeForFeedback()
        {
            try
            {
                var max = await _unitOfWork.GlobalConstant.GetByCondition(gc => gc.GlobalConstantName.Equals(GlobalValueConstant.MaximumTimeForFeedbackConstantName));
                if (max == null)
                {
                    return new ResponseDTO("Hệ thống chưa khai báo thời hạn feedback",404,false);
                }
                return new ResponseDTO("Lấy thông tin thời hạn feedback thành công",200,false,int.Parse(max.ToString()));

            }
            catch (Exception ex)
            {
                return new ResponseDTO(ex.ToString(),500,false);    
            }
        }
    }
}
