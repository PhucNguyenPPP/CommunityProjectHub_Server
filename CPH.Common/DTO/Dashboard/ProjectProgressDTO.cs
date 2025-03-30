using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Dashboard
{
    public class ProjectProgressDTO
    {
        public string ProjectName { get; set; } = null!;
        public double Percentage { get; set; }
        public string ProjectStatus { get; set; } = null!;
    }
}
