using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPH.Common.DTO.Answer;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Material;

namespace CPH.BLL.Interfaces
{
    public interface IQuestionService
    {
        Task<ResponseDTO> GetAllQuestion(string ? searchValue);
        Task<ResponseDTO> CreateQuestion(string questionContent, List<string> answers);
        Task<ResponseDTO> DeleteQuestion(Guid questionId);
        Task<ResponseDTO> UpdateQuestion(Guid questionId, string questionContent, List<UpdateAnswerDTO> answers);
    }
}
