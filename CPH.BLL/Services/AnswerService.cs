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
    public class AnswerService : IAnswerService
    {
        private readonly IUnitOfWork _unitOfWork;
        public AnswerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDTO> DeleteAnswer(Guid answerId)
        {
            var answer = await _unitOfWork.Answer.GetByCondition(c => c.AnswerId == answerId);
            if(answer == null)
            {
                return new ResponseDTO("Câu trả lời không tồn tại", 400, false);
            }

            _unitOfWork.Answer.Delete(answer);
            var result = await _unitOfWork.SaveChangeAsync();
            if (result)
            {
                return new ResponseDTO("Xóa câu trả lời thành công", 201, true);
            }
            return new ResponseDTO("Xóa câu trả lời không thành công", 400, false);
        }
    }
}
