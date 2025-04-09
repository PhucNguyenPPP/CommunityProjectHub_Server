using System;
using System.Collections.Generic;

namespace CPH.DAL.Entities;

public partial class Question
{
    public Guid QuestionId { get; set; }

    public string QuestionContent { get; set; } = null!;

    public Guid FormId { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual Form Form { get; set; } = null!;
}
