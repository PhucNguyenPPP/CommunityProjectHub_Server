using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPH.Common.DTO.Class;
using CPH.Common.DTO.Lesson;

namespace CPH.Common.DTO.Project
{
    public class ProjectDetailDTO
    {
        public Guid ProjectId { get; set; }

        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Address { get; set; } = null!;

        public int NumberLesson { get; set; }

        public int NumberTraineeEachGroup { get; set; }

        public DateTime ApplicationStartDate { get; set; }

        public DateTime ApplicationEndDate { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? MaxAbsentPercentage { get; set; }

        public decimal? FailingScore { get; set; }

        public string Status { get; set; }

        public Guid ProjectManagerId { get; set; }

        public string ProjectManagerName { get; set; }

        public Guid AssociateId { get; set; }

        public string? AssociateName { get; set; }

        public int TotalNumberLecturer { get; set; }

        public int TotalNumberTrainee { get; set; }

        public List<GetAllLessonDTO> Lessons { get; set; }

        public List<GetAllClassDTO> Classes { get; set; }
        public List<Guid> LecturerIds { get; set; } = new List<Guid>();
        public List<Guid> MemberIds { get; set; } = new List<Guid>();
    }
}
