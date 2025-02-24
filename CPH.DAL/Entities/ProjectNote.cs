using System;
using System.Collections.Generic;

namespace CPH.DAL.Entities;

public partial class ProjectNote
{
    public Guid ProjectNoteId { get; set; }

    public DateTime ExitedDate { get; set; }

    public string Reason { get; set; } = null!;

    public Guid AccountId { get; set; }

    public Guid ProjectId { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Project Project { get; set; } = null!;
}
