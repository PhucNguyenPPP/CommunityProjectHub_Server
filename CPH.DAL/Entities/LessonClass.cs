using System;
using System.Collections.Generic;

namespace CPH.DAL.Entities;

public partial class LessonClass
{
    public Guid LessonClassId { get; set; }

    public string? Room { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public Guid ClassId { get; set; }

    public Guid LessonId { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual Class Class { get; set; } = null!;

    public virtual Lesson Lesson { get; set; } = null!;
}
