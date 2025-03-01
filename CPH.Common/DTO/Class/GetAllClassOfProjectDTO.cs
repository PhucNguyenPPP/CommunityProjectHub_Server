using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Class
{
    public class GetAllClassOfProjectDTO
    {
        public Guid ClassId { get; set; }

        public string ClassCode { get; set; } = null!;

        public string? ReportContent { get; set; }

        public DateTime? ReportCreatedDate { get; set; }

        public Guid ProjectId { get; set; }

        public Guid? LecturerId { get; set; }

        public string? LecturerName { get; set; }

        public string? LecturerPhone { get; set; }

        public int LecturerSlotAvailable { get; set; } 

        public int? StudentSlotAvailable { get; set; }

        public int TotalTrainee {  get; set; }
    }
}
