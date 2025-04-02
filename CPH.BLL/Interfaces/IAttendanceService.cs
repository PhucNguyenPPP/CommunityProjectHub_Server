using CPH.Common.DTO.General;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.BLL.Interfaces
{
    public interface IAttendanceService
    {
        Task<ResponseDTO> ImportAttendanceFile(IFormFile file, Guid classId);
        ResponseDTO GetAttendanceClass(Guid classId);
        MemoryStream ExportAttendanceTraineeExcel(Guid classId);
    }
}
