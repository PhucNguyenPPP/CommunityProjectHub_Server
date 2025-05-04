using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Feedback
{
    public class TraineeFeedbackAnswer
    {
        public Guid QuestionId { get; set; }
        public string QuestionContent { get; set; } = null!;
        public Guid AnswerId { get; set; }
        public string AnswerContent { get; set; } = null!;
    }
}
