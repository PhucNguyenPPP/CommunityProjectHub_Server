using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPH.Common.DTO.Class;

namespace CPH.Common.DTO.Project
{
    public class GetProjectByTraineeDTO
    {
        public Guid ProjectId { get; set; }
        public string Title { get; set; } 
        public TraineeClassDTO TraineeClass { get; set; }
    }
}
