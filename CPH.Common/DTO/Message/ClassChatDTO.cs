using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Message
{
    public class ClassChatDTO
    {
        public Guid ClassId { get; set; }
        public string ClassCode { get; set; } = null!;
        public Guid ProjectId { get; set; }
        public string ProjectTitle { get; set; } = null!;
        public string? Content { get; set; }
        public DateTime? ContentTimestamp { get; set; }
        public string? ContentSender { get; set; }
    }
}
