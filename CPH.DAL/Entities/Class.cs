using System;
using System.Collections.Generic;

namespace CPH.DAL.Entities;

public partial class Class
{
    public Guid ClassId { get; set; }

    public string ClassCode { get; set; } = null!;

    public string ClassName { get; set; } = null!;

    public string? ReportContent { get; set; }

    public DateTime? ReportCreatedDate { get; set; }

    public Guid ProjectId { get; set; }

    public Guid LecturerId { get; set; }

    public virtual Account Lecturer { get; set; } = null!;

    public virtual ICollection<LessonClass> LessonClasses { get; set; } = new List<LessonClass>();

    public virtual ICollection<Member> Members { get; set; } = new List<Member>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual Project Project { get; set; } = null!;

    public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();

    public virtual ICollection<Trainee> Trainees { get; set; } = new List<Trainee>();
}
