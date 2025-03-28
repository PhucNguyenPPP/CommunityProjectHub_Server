using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPH.Common.DTO.Member;
using CPH.Common.DTO.Trainee;

namespace CPH.Common.DTO.Class
{
    public class ClassDetailDTO
    {
        public Guid ClassId { get; set; }

        public string ClassCode { get; set; } = null!;

        public string? ReportContent { get; set; }

        public DateTime? ReportCreatedDate { get; set; }

        public Guid ProjectId { get; set; }

        public Guid ProjectManagerId { get; set; }

        public string? ProjectTitle { get; set; }

        public string? ProjectStatus { get; set; }

        public Guid? LecturerId { get; set; }

        public string? LecturerName { get; set; }

        public string? LecturerPhone { get; set; }

        public int LecturerSlotAvailable { get; set; }

        public int? StudentSlotAvailable { get; set; }

        public int TotalTrainee { get; set; }

        public List<GetMemberOfClassDTO> getMemberOfClassDTOs {  get; set; }
        public List<GetTraineeOfClassDTO> getTraineeOfClassDTOs { get; set; }

    }
}
