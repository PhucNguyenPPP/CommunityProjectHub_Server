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

        public decimal? Score { get; set; }

        public bool? Result { get; set; }

        public int? TotalPresentSlot { get; set; }

        public int? TotalSlot { get; set; }

        public int GroupNo { get; set; }

        public string? FeedbackContent { get; set; }

        public DateTime? FeedbackCreatedDate { get; set; }

        public string? ReportContent { get; set; }

        public DateTime? ReportCreatedDate { get; set; }

        public Guid ClassId { get; set; }

        public Guid AccountId { get; set; }

        public AccountResponseDTO Account { get; set; }
    }
}
