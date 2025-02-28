using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPH.Common.DTO.Class;
using CPH.Common.DTO.General;

namespace CPH.BLL.Interfaces
{
    public interface IClassService
    {
        Task<bool> CheckClassIdExist(Guid classId);
        Task<ResponseDTO> DevideGroupOfClass(DevideGroupOfClassDTO devideGroupOfClassDTO);
        Task<ResponseDTO> GetAllClassOfProject(Guid projectId, string? searchValue, int? pageNumber, int? rowsPerPage);
        Task<ResponseDTO> GetClassDetail(Guid classId);
    }
}
