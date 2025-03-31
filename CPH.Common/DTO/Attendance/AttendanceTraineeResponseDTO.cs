using CPH.Common.DTO.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Attendance
{
    public class AttendanceTraineeResponseDTO
    {
        public Guid TraineeId { get; set; }

        public decimal? Score { get; set; }

        public int GroupNo { get; set; }

        public string? FeedbackContent { get; set; }

        public DateTime? FeedbackCreatedDate { get; set; }

        public string? ReportContent { get; set; }

        public DateTime? ReportCreatedDate { get; set; }

        public int TotalPresentLesson {  get; set; }
       
        public int TotalLesson { get; set; }

        public Guid ClassId { get; set; }

        public Guid AccountId { get; set; }

        public AccountResponseDTO Account { get; set; }
    }
}
