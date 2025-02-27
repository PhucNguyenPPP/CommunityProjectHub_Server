using CPH.Common.DTO.General;
using CPH.Common.DTO.Lecturer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.BLL.Interfaces
{
    public interface ILecturerService
    {
        List<LecturerResponseDTO> SearchLecturer(string? searchValue);

        Task<ResponseDTO> GetAllLecturerOfProject(Guid projectId, string? searchValue, int? pageNumber, int? rowsPerPage);
        Task<ResponseDTO> RemoveLecturerFromProject(Guid lecturerId, Guid classId);

    }
}
