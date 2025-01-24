using System;
using System.Collections.Generic;

namespace CPH.DAL.Entities;

public partial class Message
{
    public Guid MessageId { get; set; }

    public DateTime CreatedDate { get; set; }

    public string Content { get; set; } = null!;

    public Guid ClassId { get; set; }

    public Guid AccountId { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Class Class { get; set; } = null!;
}
