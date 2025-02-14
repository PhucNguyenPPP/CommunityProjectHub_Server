using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Notification
{
    public class NotificationResponseDTO
    {
        public Guid NotificationId { get; set; }

        public string MessageContent { get; set; } = null!;

        public bool IsRead { get; set; }

        public DateTime CreatedDate { get; set; }

        public Guid AccountId { get; set; }
    }
}
