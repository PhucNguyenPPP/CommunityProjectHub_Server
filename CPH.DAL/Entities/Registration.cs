using System;
using System.Collections.Generic;

namespace CPH.DAL.Entities;

public partial class Registration
{
    public Guid RegistrationId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string Description { get; set; } = null!;

    public Guid ClassId { get; set; }

    public Guid AccountId { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Class Class { get; set; } = null!;
}
