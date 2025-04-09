using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Answer
{
    public class GetAllAnswerDTO
    {
        public Guid AnswerId { get; set; }

        public string AnswerContent { get; set; } = null!;
    }
}
