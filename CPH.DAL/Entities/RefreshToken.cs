using System;
using System.Collections.Generic;

namespace CPH.DAL.Entities;

public partial class RefreshToken
{
    public Guid RefreshTokenId { get; set; }

    public string JwtId { get; set; } = null!;

    public string RefreshToken1 { get; set; } = null!;

    public DateTime ExpiredAt { get; set; }

    public bool IsValid { get; set; }

    public Guid AccountId { get; set; }

    public virtual Account Account { get; set; } = null!;
}
