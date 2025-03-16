using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPH.Common.DTO.LessonClass;

namespace CPH.Common.DTO.Class
{
    public class ClassAvailableDTO
    {
        public Guid ClassId { get; set; }

        public string ClassCode { get; set; } = null!;
        public Guid? LecturerId { get; set; }
        public string LecturerName { get; set; } = null!;
        public required List<LessonClassOfClassAvailableDTO> Lessons { get; set; }   

    }
}
