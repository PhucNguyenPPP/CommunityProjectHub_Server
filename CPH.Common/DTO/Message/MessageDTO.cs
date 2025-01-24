using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Message
{
    public class MessageDTO
    {
        public string Content { get; set; } = null!;

        public Guid AccountId { get; set; }

        public Guid ClassId { get; set; }
    }
}
