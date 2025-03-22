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

    public int NumberGroup { get; set; }

    public DateTime ApplicationStartDate { get; set; }

    public DateTime ApplicationEndDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public string Status { get; set; } = null!;

    public Guid? ProjectManagerId { get; set; }

    public Guid AssociateId { get; set; }

    public virtual Account Associate { get; set; } = null!;

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    public virtual ICollection<Material> Materials { get; set; } = new List<Material>();

    public virtual ICollection<ProjectLogging> ProjectLoggings { get; set; } = new List<ProjectLogging>();

    public virtual Account? ProjectManager { get; set; }
}
