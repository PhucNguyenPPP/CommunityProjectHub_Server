using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Answer
{
    public class UpdateAnswerDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập ID câu hỏi")]
        public Guid AnswerId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung câu hỏi")]
        public string AnswerContent { get; set; } = null!;
    }
}
