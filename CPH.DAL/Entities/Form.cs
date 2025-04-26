using System;
using System.Collections.Generic;

namespace CPH.DAL.Entities;

public partial class Form
{
    public Guid FormId { get; set; }

    public string FormName { get; set; } = null!;

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
