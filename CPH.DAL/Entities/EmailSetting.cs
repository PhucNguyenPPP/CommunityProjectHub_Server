using System;
using System.Collections.Generic;

namespace CPH.DAL.Entities;

public partial class EmailSetting
{
    public Guid EmailId { get; set; }

    public string EmailSettingName { get; set; } = null!;

    public decimal TimeToSend { get; set; }
}
