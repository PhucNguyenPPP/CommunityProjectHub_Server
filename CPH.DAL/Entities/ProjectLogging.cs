using System;
using System.Collections.Generic;

namespace CPH.DAL.Entities;

public partial class ProjectLogging
{
    public Guid ProjectNoteId { get; set; }

    public DateTime ActionDate { get; set; }

    public string ActionContent { get; set; } = null!;

    public string? NoteContent { get; set; }

    public Guid AccountId { get; set; }

    public Guid ProjectId { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Project Project { get; set; } = null!;
}
