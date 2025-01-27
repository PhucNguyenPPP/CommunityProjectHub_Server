using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Class
{
    public class GetAllClassDTO
    {
        public Guid ClassId { get; set; }

        public string ClassCode { get; set; } = null!;

        public string ClassName { get; set; } = null!;

        public string? ReportContent { get; set; }

        public DateTime? ReportCreatedDate { get; set; }

        public Guid ProjectId { get; set; }

        public Guid LecturerId { get; set; }
    }
}
