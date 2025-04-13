using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPH.Common.DTO.General;

namespace CPH.BLL.Interfaces
{
    public interface IFeedbackService
    {
        Task<ResponseDTO> CreateFeedback(Guid traineeId, List<Guid> answerId, string ? feedbackContent);
    }
}
