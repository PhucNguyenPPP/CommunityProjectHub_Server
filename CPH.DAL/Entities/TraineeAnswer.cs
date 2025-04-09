using System;
using System.Collections.Generic;

namespace CPH.DAL.Entities;

public partial class TraineeAnswer
{
    public Guid TraineeAnswerId { get; set; }

    public Guid TraineeId { get; set; }

    public Guid AnswerId { get; set; }

    public virtual Answer Answer { get; set; } = null!;

    public virtual Trainee Trainee { get; set; } = null!;
}
