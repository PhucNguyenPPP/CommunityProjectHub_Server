using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Notification
{
    public class UpdateNotificationRequestDTO
    {
        [Required]
        public List<Guid> NotificationIds { get; set; } = null!;
    }
}
