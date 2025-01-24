using System;
using System.Collections.Generic;

namespace CPH.DAL.Entities;

public partial class Project
{
    public Guid ProjectId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Address { get; set; } = null!;

    public int NumberLesson { get; set; }

    public int NumberTraineeEachGroup { get; set; }

    public DateTime ApplicationStartDate { get; set; }

    public DateTime ApplicationEndDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public bool Status { get; set; }

    public Guid ProjectManagerId { get; set; }

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    public virtual ICollection<Material> Materials { get; set; } = new List<Material>();

    public virtual Account ProjectManager { get; set; } = null!;
}
