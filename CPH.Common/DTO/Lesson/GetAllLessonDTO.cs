using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPH.Common.DTO.LessonClass;

namespace CPH.Common.DTO.Lesson
{
    public class GetAllLessonDTO
    {
        public Guid LessonId { get; set; }

        public int LessonNo { get; set; }

        public string LessonContent { get; set; } = null!;

        public Guid ProjectId { get; set; }

        public List<GetAllLessonClassDTO> LessonClasses { get; set; }
    }
}
