using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Feedback
{
    public class GetAllFeedbackOfProjectResponseDTO
    {
        public Guid TraineeId { get; set; }
        public string TraineeAccountCode { get; set; } = null!;
        public string TraineeName { get; set; } = null!;
        public string? FeedbackContent { get; set; }
        public DateTime? FeedbackCreatedDate { get; set; }
        public List<TraineeFeedbackAnswer> traineeFeedbackAnswers { get; set; } = new List<TraineeFeedbackAnswer>();
    }
}
