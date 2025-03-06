using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Project
{
    public class GetAllProjectDTO
    {
        public Guid ProjectId { get; set; }

        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Address { get; set; } = null!;

        public int NumberLesson { get; set; }

        public DateTime ApplicationStartDate { get; set; }

        public DateTime ApplicationEndDate { get; set; }

        public DateTime CreatedDate { get; set; }

        public string Status { get; set; }

        public Guid? ProjectManagerId { get; set; }

        public string? ProjectManagerName { get; set; }
    }
}
