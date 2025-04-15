using System;
using System.Collections.Generic;

namespace CPH.DAL.Entities;

public partial class Lesson
{
    public Guid LessonId { get; set; }

    public int LessonNo { get; set; }

    public string LessonContent { get; set; } = null!;

    public Guid ProjectId { get; set; }

    public virtual ICollection<LessonClass> LessonClasses { get; set; } = new List<LessonClass>();

    public virtual Project Project { get; set; } = null!;
}
