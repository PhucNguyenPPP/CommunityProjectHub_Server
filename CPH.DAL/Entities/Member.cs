﻿using System;
using System.Collections.Generic;

namespace CPH.DAL.Entities;

public partial class Member
{
    public Guid MemberId { get; set; }

    public int GroupSupportNo { get; set; }

    public Guid ClassId { get; set; }

    public Guid AccountId { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Class Class { get; set; } = null!;
}
