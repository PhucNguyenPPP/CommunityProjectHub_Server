using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Class
{
    public class GetAllClassOfTrainee
    {
        public Guid ClassId { get; set; }

        public string ClassCode { get; set; } = null!;

        public string? ReportContent { get; set; }

        public DateTime? ReportCreatedDate { get; set; }

        public string? TraineeReportContent { get; set; }

        public DateTime? TraineeReportCreatedDate { get; set; }

        public Guid ProjectId { get; set; }

        public string ProjectTitle { get; set; } = null!;

        public DateTime ProjectStartDate { get; set; }

        public DateTime ProjectEndDate { get; set; }

        public string ProjectAddress { get; set; } = null!;

        public int ProjectNumberLesson { get; set; }

        public DateTime ProjectApplicationStartDate { get; set; }

        public DateTime ProjectApplicationEndDate { get; set; }

        public DateTime ProjectCreatedDate { get; set; }

        public string ProjectStatus { get; set; } = null!;

        public Guid? ProjectManagerId { get; set; }

        public Guid? LecturerId { get; set; }

        public string? LecturerName { get; set; }

        public string? LecturerPhone { get; set; }

        public string? LecturerEmail { get; set; }
    }
}
