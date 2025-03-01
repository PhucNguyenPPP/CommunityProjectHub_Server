using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Message
{
    public class MessageResponseDTO
    {
        public Guid MessageId { get; set; }
        public string Content { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public Guid SendAccountId { get; set; }
        public string SendAccountName { get; set; } = null!;
        public Guid ClassId { get; set; }
    }
}
