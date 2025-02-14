using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPH.Common.DTO.LessonClass;

namespace CPH.Common.DTO.Lesson
{
    public class UpdateLessonOfProjectDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập Id của dự án")]
        public Guid ProjectId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập bài học của dự án")]
        public required List<string> LessonOfProject { get; set; }
    }
}
