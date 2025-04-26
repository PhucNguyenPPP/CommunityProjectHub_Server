using System;
using System.Collections.Generic;

namespace CPH.DAL.Entities;

public partial class GlobalConstant
{
    public Guid GlobalConstantId { get; set; }

    public string GlobalConstantName { get; set; } = null!;

    public string GlobalConstantValue { get; set; } = null!;
}
