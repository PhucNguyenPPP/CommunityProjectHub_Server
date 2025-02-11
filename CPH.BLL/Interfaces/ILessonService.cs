using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Lesson;

namespace CPH.BLL.Interfaces
{
    public interface ILessonService
    {
        Task<ResponseDTO> UpdateLessonOfProject(UpdateLessonOfProjectDTO lessonOfProjectDTO);
    }
}
