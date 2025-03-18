using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Member;
using CPH.Common.DTO.Trainee;
using Microsoft.AspNetCore.Http;

namespace CPH.BLL.Interfaces
{
    public interface ITraineeService
    {
        Task<ResponseDTO> GetAllTraineeOfClass (Guid classId, string? searchValue, int? pageNumber, int? rowsPerPage, string? filterField, string? filterOrder);
        Task<ResponseDTO> UpdateScoreTrainee(ScoreTraineeRequestDTO model);
        Task<ResponseDTO> GetScoreTraineeList(Guid classId);
        Task<ResponseDTO> RemoveTrainee(Guid classId, Guid accountId, string? reason);
        Task<ResponseDTO> ImportTraineeScoreByExcel(IFormFile file, Guid classId);
        MemoryStream ExportTraineeListExcel(Guid classId);
        Task<ResponseDTO> AddTraineeHadAccount(AddTraineeHadAccountDTO addTraineeHadAccountDTO);
        Task<ResponseDTO> CheckValidationTrainee(SignUpRequestOfTraineeDTO model);
        Task<ResponseDTO> AddTraineeNoAccount(SignUpRequestOfTraineeDTO model);
        Task<ResponseDTO> UpdateReport(Guid accountId, Guid classId, IFormFile file);
        Task<string> StoreFileAndGetLink(IFormFile file, string folderName);
        List<MemberResponseDTO> SearchTraineeToAddToClass(string? searchValue);
        Task<ResponseDTO> MoveTraineeClass(MoveTraineeClassDTO moveTraineeClassDTO);
        Task<ResponseDTO> GetAvailableGroupOfClass(Guid currentClassId, Guid accountId);
    }
}