using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Message
{
    public class ClassChatDTO
    {
        public Guid ClassId { get; set; }
        public string ClassCode { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectTitle { get; set; }
        public string Content { get; set; }
    }
}
