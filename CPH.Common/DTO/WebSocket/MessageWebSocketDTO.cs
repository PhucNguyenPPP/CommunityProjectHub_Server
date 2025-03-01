using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.WebSocket
{
    public class MessageWebSocketDTO
    {
        public Guid MessageId { get; set; }

        public DateTime CreatedDate { get; set; }

        public string Content { get; set; } = null!;

        public Guid ClassId { get; set; }

        public Guid SendAccountId { get; set; }
        public string SendAccountName { get; set; }
        public string Type { get; set; } = null!;
    }
}
