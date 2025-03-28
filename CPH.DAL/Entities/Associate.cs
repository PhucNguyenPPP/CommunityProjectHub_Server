using System;
using System.Collections.Generic;

namespace CPH.DAL.Entities;

public partial class Associate
{
    public Guid AccountId { get; set; }

    public string AssociateName { get; set; } = null!;

    public virtual Account Account { get; set; } = null!;
}
