using System;
using System.Collections.Generic;

namespace CPH.DAL.Entities;

public partial class Notification
{
    public Guid NotificationId { get; set; }

    public string MessageContent { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime CreatedDate { get; set; }

    public Guid AccountId { get; set; }

    public virtual Account Account { get; set; } = null!;
}
