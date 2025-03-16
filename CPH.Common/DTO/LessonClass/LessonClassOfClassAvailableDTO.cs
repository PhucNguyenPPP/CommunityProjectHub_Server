using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.LessonClass
{
    public class LessonClassOfClassAvailableDTO
    {
        public Guid LessonClassId { get; set; }

        public string? Room { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public Guid LessonId { get; set; }
    }
}
