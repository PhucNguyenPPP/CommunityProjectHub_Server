using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPH.Common.DTO.General;

namespace CPH.BLL.Interfaces
{
    public interface ILessonClassService
    {
        Task<ResponseDTO> GetLessonClass(Guid classId);
    }
}
