using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Trainee;

namespace CPH.BLL.Interfaces
{
    public interface ITraineeService
    {
        Task<ResponseDTO> GetAllTraineeOfClass (Guid classId, string? searchValue, int? pageNumber, int? rowsPerPage, string? filterField, string? filterOrder);
        Task<ResponseDTO> UpdateScoreTrainee(ScoreTraineeRequestDTO model);
        Task<ResponseDTO> GetScoreTraineeList(Guid classId);
        Task<ResponseDTO> RemoveTrainee(Guid classId, Guid accountId, string? reason);
        Task<ResponseDTO> AddTraineeHadAccount(AddTraineeHadAccountDTO addTraineeHadAccountDTO);
    }
}