using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.LessonClass
{
    public class GetAllLessonClassForScheduleDTO
    {
        public string ClassCode { get; set; }
        public string LessonName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int LessonNo { get; set; }
        public string? Room { get; set; }
        public string ProjectName { get; set; }
    }
}
