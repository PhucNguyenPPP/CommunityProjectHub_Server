using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Registration;

namespace CPH.BLL.Interfaces
{
    public interface IRegistrationService
    {
        Task<ResponseDTO> AnswerRegistration(AnswerRegistrationDTO answerRegistrationDTO);
        Task<ResponseDTO> CancelRegistration(Guid cancelRegistrationId);
        Task<ResponseDTO> GetAllSentRegistrations(Guid accountId, string search, int? rowsPerPage, int? pageNumber);
        Task<ResponseDTO> SendRegistration(SendRegistrationDTO registrationDTO);
    }
}
