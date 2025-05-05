using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CPH.BLL.Interfaces;
using CPH.Common.Constant;
using CPH.Common.DTO.General;
using CPH.DAL.Entities;
using CPH.DAL.UnitOfWork;
using OfficeOpenXml.Packaging.Ionic.Zlib;

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
                return new ResponseDTO("Lấy thông tin thời hạn feedback thành công",200,true,int.Parse(max.GlobalConstantValue.ToString()));

            }
            catch (Exception ex)
            {
                return new ResponseDTO(ex.ToString(),500,false);    
            }
        }

        public async Task<ResponseDTO> UpdateMaxTimeForFeedback(int value)
        {
            try
            {
                var max = await _unitOfWork.GlobalConstant.GetByCondition(gc => gc.GlobalConstantName.Equals(GlobalValueConstant.MaximumTimeForFeedbackConstantName));
                if (value < 1 || value > 90)
                {
                    return new ResponseDTO("Thời hạn feedback trong khoảng 1 -> 90 ngày", 400, false);
                }
                if (max == null)
                {
                    
                    GlobalConstant globalConstant = new GlobalConstant()
                    {
                        GlobalConstantId = Guid.NewGuid(),
                        GlobalConstantName = GlobalValueConstant.MaximumTimeForFeedbackConstantName,
                        GlobalConstantValue = value.ToString(),
                    };
                    await _unitOfWork.GlobalConstant.AddAsync(globalConstant);
                    var added =await _unitOfWork.SaveChangeAsync();
                    if (added == false)
                    {
                        return new ResponseDTO("Cập nhật thông tin thời hạn feedback thất bại", 500, false);
                    }
                    return new ResponseDTO("Cập nhật thông tin thời hạn feedback thành công", 200, true);
                }
                max.GlobalConstantValue = value.ToString();
                 _unitOfWork.GlobalConstant.Update(max);
                var updated = await _unitOfWork.SaveChangeAsync();
                if (updated == true)
                {
                    return new ResponseDTO("Cập nhật thông tin thời hạn feedback thành công", 200, true);
                }
                return new ResponseDTO("Cập nhật thông tin thời hạn feedback thất bại", 500, false);

            }
            catch (Exception ex)
            {
                return new ResponseDTO(ex.ToString(), 500, false);
            }
        }
    }
}
