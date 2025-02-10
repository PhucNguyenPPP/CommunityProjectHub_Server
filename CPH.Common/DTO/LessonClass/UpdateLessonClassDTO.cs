using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.LessonClass
{
    public class UpdateLessonClassDTO
    {
        [Required]
        public Guid LessonClassId { get; set; }

        [Required]
        public string Room { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }
    }
}
