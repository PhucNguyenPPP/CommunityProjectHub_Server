using System;
using System.Collections.Generic;

namespace CPH.DAL.Entities;

public partial class Material
{
    public Guid MaterialId { get; set; }

    public string Title { get; set; } = null!;

    public string MaterialUrl { get; set; } = null!;

    public DateTime UploadedAt { get; set; }

    public Guid ProjectId { get; set; }

    public virtual Project Project { get; set; } = null!;
}
