using System;
using System.Collections.Generic;

namespace CPH.DAL.Entities;

public partial class Answer
{
    public Guid AnswerId { get; set; }

    public string AnswerContent { get; set; } = null!;

    public Guid QuestionId { get; set; }

    public virtual Question Question { get; set; } = null!;

    public virtual ICollection<TraineeAnswer> TraineeAnswers { get; set; } = new List<TraineeAnswer>();
}
