using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPH.Common.DTO.Answer;

namespace CPH.Common.DTO.Question
{
    public class GetAllQuestionDTO
    {
        public Guid QuestionId { get; set; }

        public string QuestionContent { get; set; } = null!;

        public List<GetAllAnswerDTO> AnwserList { get; set; }
    }
}
