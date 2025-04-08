using System;
using System.Collections.Generic;

namespace CPH.DAL.Entities;

public partial class Trainee
{
    public Guid TraineeId { get; set; }

    public decimal? Score { get; set; }

    public bool? Result { get; set; }

    public int? GroupNo { get; set; }

    public string? FeedbackContent { get; set; }

    public DateTime? FeedbackCreatedDate { get; set; }

    public string? ReportContent { get; set; }

    public DateTime? ReportCreatedDate { get; set; }

    public Guid ClassId { get; set; }

    public Guid AccountId { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual Class Class { get; set; } = null!;
}
