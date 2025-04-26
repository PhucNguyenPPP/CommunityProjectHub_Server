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

    public virtual DbSet<Answer> Answers { get; set; }

    public virtual DbSet<Associate> Associates { get; set; }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<Form> Forms { get; set; }

    public virtual DbSet<GlobalConstant> GlobalConstants { get; set; }

    public virtual DbSet<Lesson> Lessons { get; set; }

    public virtual DbSet<LessonClass> LessonClasses { get; set; }

    public virtual DbSet<Material> Materials { get; set; }

    public virtual DbSet<Member> Members { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<ProjectLogging> ProjectLoggings { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Registration> Registrations { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Trainee> Trainees { get; set; }

    public virtual DbSet<TraineeAnswer> TraineeAnswers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;uid=SA;pwd=12345;database=CPH_DB;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__Account__349DA5A6EDDB18B2");

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

        modelBuilder.Entity<Answer>(entity =>
        {
            entity.HasKey(e => e.AnswerId).HasName("PK__Answer__D4825004334E40B4");

            entity.ToTable("Answer");

            entity.Property(e => e.AnswerId).ValueGeneratedNever();
            entity.Property(e => e.AnswerContent).HasMaxLength(100);

            entity.HasOne(d => d.Question).WithMany(p => p.Answers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Answer__Question__628FA481");
        });

        modelBuilder.Entity<Associate>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__Associat__349DA5A6262B6C36");

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
            entity.HasKey(e => e.AttendanceId).HasName("PK__Attendan__8B69261CCE668BA6");

            entity.ToTable("Attendance");

            entity.Property(e => e.AttendanceId).ValueGeneratedNever();

            entity.HasOne(d => d.LessonClass).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.LessonClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Attendanc__Lesso__4CA06362");

            entity.HasOne(d => d.Trainee).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.TraineeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Attendanc__Train__4D94879B");
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.ClassId).HasName("PK__Class__CB1927C085C2D63E");

            entity.ToTable("Class");

            entity.Property(e => e.ClassId).ValueGeneratedNever();
            entity.Property(e => e.ClassCode).HasMaxLength(20);

            entity.HasOne(d => d.Lecturer).WithMany(p => p.Classes)
                .HasForeignKey(d => d.LecturerId)
                .HasConstraintName("FK__Class__LecturerI__33D4B598");

            entity.HasOne(d => d.Project).WithMany(p => p.Classes)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Class__ProjectId__32E0915F");
        });

        modelBuilder.Entity<Form>(entity =>
        {
            entity.HasKey(e => e.FormId).HasName("PK__Form__FB05B7DDB5632219");

            entity.ToTable("Form");

            entity.Property(e => e.FormId).ValueGeneratedNever();
            entity.Property(e => e.FormName).HasMaxLength(100);
        });

        modelBuilder.Entity<GlobalConstant>(entity =>
        {
            entity.HasKey(e => e.GlobalConstantId).HasName("PK__GlobalCo__BAC057D8B2F50E6E");

            entity.ToTable("GlobalConstant");

            entity.HasIndex(e => e.GlobalConstantName, "UQ__GlobalCo__536CBD1DEA52A60A").IsUnique();

            entity.Property(e => e.GlobalConstantId).ValueGeneratedNever();
            entity.Property(e => e.GlobalConstantName).HasMaxLength(100);
            entity.Property(e => e.GlobalConstantValue).HasMaxLength(100);
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.LessonId).HasName("PK__Lesson__B084ACD038C370A9");

            entity.ToTable("Lesson");

            entity.Property(e => e.LessonId).ValueGeneratedNever();

            entity.HasOne(d => d.Project).WithMany(p => p.Lessons)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Lesson__ProjectI__3E52440B");
        });

        modelBuilder.Entity<LessonClass>(entity =>
        {
            entity.HasKey(e => e.LessonClassId).HasName("PK__LessonCl__8CD4294808C95DB2");

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
            entity.HasKey(e => e.MaterialId).HasName("PK__Material__C50610F7AAF7DC26");

            entity.ToTable("Material");

            entity.Property(e => e.MaterialId).ValueGeneratedNever();
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.UploadedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Project).WithMany(p => p.Materials)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Material__Projec__48CFD27E");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.Materials)
                .HasForeignKey(d => d.UpdatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Material__Update__49C3F6B7");
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(e => e.MemberId).HasName("PK__Member__0CF04B182338C5EA");

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
            entity.HasKey(e => e.MessageId).HasName("PK__Message__C87C0C9C5826A738");

            entity.ToTable("Message");

            entity.Property(e => e.MessageId).ValueGeneratedNever();
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.Messages)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Message__Account__5165187F");

            entity.HasOne(d => d.Class).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Message__ClassId__5070F446");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E1248B95E00");

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
            entity.HasKey(e => e.ProjectId).HasName("PK__Project__761ABEF09B7FE9A8");

            entity.ToTable("Project");

            entity.Property(e => e.ProjectId).ValueGeneratedNever();
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.ApplicationEndDate).HasColumnType("datetime");
            entity.Property(e => e.ApplicationStartDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.FailingScore).HasColumnType("decimal(5, 1)");
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
            entity.HasKey(e => e.ProjectNoteId).HasName("PK__ProjectL__EB837EF1A80B3F64");

            entity.ToTable("ProjectLogging");

            entity.Property(e => e.ProjectNoteId).ValueGeneratedNever();
            entity.Property(e => e.ActionDate).HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.ProjectLoggings)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProjectLo__Accou__59FA5E80");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectLoggings)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProjectLo__Proje__5AEE82B9");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__Question__0DC06FAC28BAE3DA");

            entity.ToTable("Question");

            entity.Property(e => e.QuestionId).ValueGeneratedNever();

            entity.HasOne(d => d.Form).WithMany(p => p.Questions)
                .HasForeignKey(d => d.FormId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Question__FormId__5FB337D6");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.RefreshTokenId).HasName("PK__RefreshT__F5845E3900D63F1F");

            entity.ToTable("RefreshToken");

            entity.Property(e => e.RefreshTokenId).ValueGeneratedNever();
            entity.Property(e => e.ExpiredAt).HasColumnType("datetime");
            entity.Property(e => e.RefreshToken1).HasColumnName("RefreshToken");

            entity.HasOne(d => d.Account).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RefreshTo__Accou__571DF1D5");
        });

        modelBuilder.Entity<Registration>(entity =>
        {
            entity.HasKey(e => e.RegistrationId).HasName("PK__Registra__6EF58810FEB19F78");

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
            entity.HasKey(e => e.RoleId).HasName("PK__Role__8AFACE1A7B043C9A");

            entity.ToTable("Role");

            entity.Property(e => e.RoleId).ValueGeneratedNever();
            entity.Property(e => e.RoleName).HasMaxLength(100);
        });

        modelBuilder.Entity<Trainee>(entity =>
        {
            entity.HasKey(e => e.TraineeId).HasName("PK__Trainee__3BA911CAA41F0DCA");

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

        modelBuilder.Entity<TraineeAnswer>(entity =>
        {
            entity.HasKey(e => e.TraineeAnswerId).HasName("PK__TraineeA__7B8ED3F6FA00BF8D");

            entity.ToTable("TraineeAnswer");

            entity.Property(e => e.TraineeAnswerId).ValueGeneratedNever();

            entity.HasOne(d => d.Answer).WithMany(p => p.TraineeAnswers)
                .HasForeignKey(d => d.AnswerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TraineeAn__Answe__66603565");

            entity.HasOne(d => d.Trainee).WithMany(p => p.TraineeAnswers)
                .HasForeignKey(d => d.TraineeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TraineeAn__Train__656C112C");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
