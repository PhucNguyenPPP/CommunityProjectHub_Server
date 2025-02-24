using System;
using System.Collections.Generic;

namespace CPH.DAL.Entities;

public partial class Account
{
    public Guid AccountId { get; set; }

    public string AccountCode { get; set; } = null!;

    public string AccountName { get; set; } = null!;

    public byte[] Salt { get; set; } = null!;

    public byte[] PasswordHash { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? AvatarLink { get; set; }

    public string Phone { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateTime DateOfBirth { get; set; }

    public string Gender { get; set; } = null!;

    public int? OtpCode { get; set; }

    public DateTime? OtpExpiredTime { get; set; }

    public bool Status { get; set; }

    public int RoleId { get; set; }

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    public virtual ICollection<Member> Members { get; set; } = new List<Member>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<ProjectNote> ProjectNotes { get; set; } = new List<ProjectNote>();

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<Trainee> Trainees { get; set; } = new List<Trainee>();
}
