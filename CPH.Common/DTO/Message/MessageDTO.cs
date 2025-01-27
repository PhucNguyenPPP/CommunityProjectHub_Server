using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Message
{
    public class MessageDTO
    {
        [Required]
        public string Content { get; set; } = null!;

        [Required]
        public Guid AccountId { get; set; }

        [Required]
        public Guid ClassId { get; set; }
    }
}
