using System;
using System.Collections.Generic;
using CPH.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CPH.DAL.Context;

public partial class CphDbContext : DbContext
{
    public CphDbContext()
    {
    }

    public CphDbContext(DbContextOptions<CphDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Associate> Associates { get; set; }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<EmailSetting> EmailSettings { get; set; }

    public virtual DbSet<Lesson> Lessons { get; set; }

    public virtual DbSet<LessonClass> LessonClasses { get; set; }

    public virtual DbSet<Material> Materials { get; set; }

    public virtual DbSet<Member> Members { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<ProjectLogging> ProjectLoggings { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Registration> Registrations { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Trainee> Trainees { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;uid=SA;pwd=12345;database=CPH_DB;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__Account__349DA5A698DC703D");

            entity.ToTable("Account");

            entity.Property(e => e.AccountId).ValueGeneratedNever();
            entity.Property(e => e.AccountCode).HasMaxLength(20);
            entity.Property(e => e.AccountName).HasMaxLength(100);
            entity.Property(e => e.DateOfBirth).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(20);
            entity.Property(e => e.OtpExpiredTime).HasColumnType("datetime");
            entity.Property(e => e.Phone).HasMaxLength(20);

            entity.HasOne(d => d.Role).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Account__RoleId__267ABA7A");
        });

        modelBuilder.Entity<Associate>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__Associat__349DA5A6BBEDFA60");

            entity.ToTable("Associate");

            entity.Property(e => e.AccountId).ValueGeneratedNever();
            entity.Property(e => e.AssociateName).HasMaxLength(200);

            entity.HasOne(d => d.Account).WithOne(p => p.Associate)
                .HasForeignKey<Associate>(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Associate__Accou__29572725");
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId).HasName("PK__Attendan__8B69261CBE75148C");

            entity.ToTable("Attendance");

            entity.Property(e => e.AttendanceId).ValueGeneratedNever();

            entity.HasOne(d => d.LessonClass).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.LessonClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Attendanc__Lesso__4BAC3F29");

            entity.HasOne(d => d.Trainee).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.TraineeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Attendanc__Train__4CA06362");
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.ClassId).HasName("PK__Class__CB1927C018233746");

            entity.ToTable("Class");

            entity.Property(e => e.ClassId).ValueGeneratedNever();
            entity.Property(e => e.ClassCode).HasMaxLength(20);
            entity.Property(e => e.ReportCreatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Lecturer).WithMany(p => p.Classes)
                .HasForeignKey(d => d.LecturerId)
                .HasConstraintName("FK__Class__LecturerI__33D4B598");

            entity.HasOne(d => d.Project).WithMany(p => p.Classes)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Class__ProjectId__32E0915F");
        });

        modelBuilder.Entity<EmailSetting>(entity =>
        {
            entity.HasKey(e => e.EmailId).HasName("PK__EmailSet__7ED91ACFAD64826C");

            entity.ToTable("EmailSetting");

            entity.Property(e => e.EmailId).ValueGeneratedNever();
            entity.Property(e => e.EmailSettingName).HasMaxLength(1);
            entity.Property(e => e.TimeToSend).HasColumnType("decimal(18, 0)");
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.LessonId).HasName("PK__Lesson__B084ACD0982132C2");

            entity.ToTable("Lesson");

            entity.Property(e => e.LessonId).ValueGeneratedNever();

            entity.HasOne(d => d.Project).WithMany(p => p.Lessons)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Lesson__ProjectI__3E52440B");
        });

        modelBuilder.Entity<LessonClass>(entity =>
        {
            entity.HasKey(e => e.LessonClassId).HasName("PK__LessonCl__8CD42948B7E7F5FB");

            entity.ToTable("LessonClass");

            entity.Property(e => e.LessonClassId).ValueGeneratedNever();
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.Room).HasMaxLength(50);
            entity.Property(e => e.StartTime).HasColumnType("datetime");

            entity.HasOne(d => d.Class).WithMany(p => p.LessonClasses)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LessonCla__Class__412EB0B6");

            entity.HasOne(d => d.Lesson).WithMany(p => p.LessonClasses)
                .HasForeignKey(d => d.LessonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LessonCla__Lesso__4222D4EF");
        });

        modelBuilder.Entity<Material>(entity =>
        {
            entity.HasKey(e => e.MaterialId).HasName("PK__Material__C50610F75FB761DC");

            entity.ToTable("Material");

            entity.Property(e => e.MaterialId).ValueGeneratedNever();
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.UploadedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Project).WithMany(p => p.Materials)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Material__Projec__48CFD27E");
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(e => e.MemberId).HasName("PK__Member__0CF04B1870048C6A");

            entity.ToTable("Member");

            entity.Property(e => e.MemberId).ValueGeneratedNever();

            entity.HasOne(d => d.Account).WithMany(p => p.Members)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Member__AccountI__37A5467C");

            entity.HasOne(d => d.Class).WithMany(p => p.Members)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Member__ClassId__36B12243");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__Message__C87C0C9CFBC2BE46");

            entity.ToTable("Message");

            entity.Property(e => e.MessageId).ValueGeneratedNever();
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.Messages)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Message__Account__5070F446");

            entity.HasOne(d => d.Class).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Message__ClassId__4F7CD00D");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E1255F362F0");

            entity.ToTable("Notification");

            entity.Property(e => e.NotificationId).ValueGeneratedNever();
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__Accou__2C3393D0");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.ProjectId).HasName("PK__Project__761ABEF072D33421");

            entity.ToTable("Project");

            entity.Property(e => e.ProjectId).ValueGeneratedNever();
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.ApplicationEndDate).HasColumnType("datetime");
            entity.Property(e => e.ApplicationStartDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(100);

            entity.HasOne(d => d.Associate).WithMany(p => p.ProjectAssociates)
                .HasForeignKey(d => d.AssociateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Project__Associa__300424B4");

            entity.HasOne(d => d.ProjectManager).WithMany(p => p.ProjectProjectManagers)
                .HasForeignKey(d => d.ProjectManagerId)
                .HasConstraintName("FK__Project__Project__2F10007B");
        });

        modelBuilder.Entity<ProjectLogging>(entity =>
        {
            entity.HasKey(e => e.ProjectNoteId).HasName("PK__ProjectL__EB837EF1134E28A4");

            entity.ToTable("ProjectLogging");

            entity.Property(e => e.ProjectNoteId).ValueGeneratedNever();
            entity.Property(e => e.ActionDate).HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.ProjectLoggings)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProjectLo__Accou__5812160E");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectLoggings)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProjectLo__Proje__59063A47");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.RefreshTokenId).HasName("PK__RefreshT__F5845E39D12F9A1B");

            entity.ToTable("RefreshToken");

            entity.Property(e => e.RefreshTokenId).ValueGeneratedNever();
            entity.Property(e => e.ExpiredAt).HasColumnType("datetime");
            entity.Property(e => e.RefreshToken1).HasColumnName("RefreshToken");

            entity.HasOne(d => d.Account).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RefreshTo__Accou__5535A963");
        });

        modelBuilder.Entity<Registration>(entity =>
        {
            entity.HasKey(e => e.RegistrationId).HasName("PK__Registra__6EF5881057C154A7");

            entity.ToTable("Registration");

            entity.Property(e => e.RegistrationId).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(100);

            entity.HasOne(d => d.Account).WithMany(p => p.Registrations)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Registrat__Accou__45F365D3");

            entity.HasOne(d => d.Class).WithMany(p => p.Registrations)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Registrat__Class__44FF419A");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Role__8AFACE1AC11F8152");

            entity.ToTable("Role");

            entity.Property(e => e.RoleId).ValueGeneratedNever();
            entity.Property(e => e.RoleName).HasMaxLength(100);
        });

        modelBuilder.Entity<Trainee>(entity =>
        {
            entity.HasKey(e => e.TraineeId).HasName("PK__Trainee__3BA911CA37279C69");

            entity.ToTable("Trainee");

            entity.Property(e => e.TraineeId).ValueGeneratedNever();
            entity.Property(e => e.FeedbackCreatedDate).HasColumnType("datetime");
            entity.Property(e => e.ReportCreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Score).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Account).WithMany(p => p.Trainees)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Trainee__Account__3B75D760");

            entity.HasOne(d => d.Class).WithMany(p => p.Trainees)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Trainee__ClassId__3A81B327");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
