using System;
using System.Collections.Generic;

namespace CPH.DAL.Entities;

public partial class Attendance
{
    public Guid AttendanceId { get; set; }

    public bool? Status { get; set; }

    public Guid LessonClassId { get; set; }

    public Guid TraineeId { get; set; }

    public virtual LessonClass LessonClass { get; set; } = null!;

    public virtual Trainee Trainee { get; set; } = null!;
}
