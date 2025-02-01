using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPH.Common.DTO.General;
using CPH.DAL.Entities;

namespace CPH.BLL.Interfaces
{
    public interface IProjectService
    {
        Task<ResponseDTO> CheckProjectExisted(Guid projectID);
        Task<ResponseDTO> GetAllProject(string? searchValue, int? pageNumber, int? rowsPerPage, string? filterField, string? filterOrder );
        Task<ResponseDTO> GetAllRelatedProject(string? searchValue, int? pageNumber, int? rowsPerPage, string? filterField, string? filterOrder, Guid userId);
        Task<ResponseDTO> GetProjectDetail(Guid projectId);

        Task<ResponseDTO> InActivateProject(Guid projectID);
    }
}
