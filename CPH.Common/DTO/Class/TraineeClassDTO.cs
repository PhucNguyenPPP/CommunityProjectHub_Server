using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Class
{
    public class TraineeClassDTO
    {
        public Guid ClassId { get; set; }
        public string ClassCode { get; set; }
        public Guid? LecturerId { get; set; }
        public string? LecturerName { get; set; }
    }
}
