using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPH.Common.DTO.Account;

namespace CPH.Common.DTO.Trainee
{
    public class GetAllTraineeOfClassDTO
    {
        public Guid TraineeId { get; set; }

        public bool? Score { get; set; }

        public int GroupNo { get; set; }

        public string? FeedbackContent { get; set; }

        public DateTime? FeedbackCreatedDate { get; set; }

        public Guid ClassId { get; set; }

        public Guid AccountId { get; set; }

        public AccountResponseDTO Account { get; set; }
    }
}
