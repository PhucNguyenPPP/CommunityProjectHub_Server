using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Project
{
    public class UpdateAbsentPercentageFailingScoreRequestDTO
    {
        [Required]
        public Guid ProjectId { get; set; }

        [Required]
        public int MaxAbsentPercentage { get; set; }

        [Required]
        public decimal FailingScore { get; set; }
    }
}
