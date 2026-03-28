using Microsoft.EntityFrameworkCore;
using SMS.Domain.Entities;

namespace SMS.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<ParentContact> ParentContacts { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<AcademicYear> AcademicYears { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<GradeCategory> GradeCategories { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<GradeAuditLog> GradeAuditLogs { get; set; }
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<ExamResult> ExamResults { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<AnnouncementReadStatus> AnnouncementReadStatuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Username).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Role).HasConversion<int>();
            });

            // RefreshToken
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Token).IsUnique();
                entity.Property(e => e.Token).IsRequired();
                entity.HasOne(e => e.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Ignore(e => e.IsRevoked);
            });

            // Student
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.PhotoUrl).HasMaxLength(500);
                entity.HasOne(e => e.User)
                    .WithOne(u => u.Student)
                    .HasForeignKey<Student>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ParentContact
            modelBuilder.Entity<ParentContact>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.HasOne(e => e.Student)
                    .WithOne(s => s.ParentContact)
                    .HasForeignKey<ParentContact>(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Teacher
            modelBuilder.Entity<Teacher>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.HasOne(e => e.User)
                    .WithOne(u => u.Teacher)
                    .HasForeignKey<Teacher>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Class)
                    .WithOne(c => c.Teacher)
                    .HasForeignKey<Teacher>(e => e.ClassId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // AcademicYear
            modelBuilder.Entity<AcademicYear>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
            });

            // Class
            modelBuilder.Entity<Class>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.HasOne(e => e.AcademicYear)
                    .WithMany(a => a.Classes)
                    .HasForeignKey(e => e.AcademicYearId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Enrollment
            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Student)
                    .WithMany(s => s.Enrollments)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Class)
                    .WithMany(c => c.Enrollments)
                    .HasForeignKey(e => e.ClassId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.AcademicYear)
                    .WithMany()
                    .HasForeignKey(e => e.AcademicYearId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Subject
            modelBuilder.Entity<Subject>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.HasOne(e => e.Class)
                    .WithMany(c => c.Subjects)
                    .HasForeignKey(e => e.ClassId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // GradeCategory
            modelBuilder.Entity<GradeCategory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Weight).HasPrecision(5, 2);
                entity.HasOne(e => e.Subject)
                    .WithMany(s => s.GradeCategories)
                    .HasForeignKey(e => e.SubjectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Grade
            modelBuilder.Entity<Grade>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Score).HasPrecision(8, 2);
                entity.HasOne(e => e.Student)
                    .WithMany(s => s.Grades)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.GradeCategory)
                    .WithMany(gc => gc.Grades)
                    .HasForeignKey(e => e.GradeCategoryId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.EnteredByUser)
                    .WithMany()
                    .HasForeignKey(e => e.EnteredBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // GradeAuditLog
            modelBuilder.Entity<GradeAuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OldScore).HasPrecision(8, 2);
                entity.Property(e => e.NewScore).HasPrecision(8, 2);
                entity.HasOne(e => e.Grade)
                    .WithMany(g => g.AuditLogs)
                    .HasForeignKey(e => e.GradeId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.ChangedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.ChangedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // AttendanceRecord
            modelBuilder.Entity<AttendanceRecord>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.StudentId, e.AbsenceDate }).IsUnique();
                entity.HasOne(e => e.Student)
                    .WithMany(s => s.AttendanceRecords)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Class)
                    .WithMany(c => c.AttendanceRecords)
                    .HasForeignKey(e => e.ClassId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.RecordedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.RecordedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Exam
            modelBuilder.Entity<Exam>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                entity.Property(e => e.MaxScore).HasPrecision(8, 2);
                entity.Property(e => e.PassingThreshold).HasPrecision(5, 2);
                entity.HasOne(e => e.Subject)
                    .WithMany(s => s.Exams)
                    .HasForeignKey(e => e.SubjectId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ExamResult
            modelBuilder.Entity<ExamResult>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Score).HasPrecision(8, 2);
                entity.HasOne(e => e.Exam)
                    .WithMany(ex => ex.ExamResults)
                    .HasForeignKey(e => e.ExamId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Student)
                    .WithMany(s => s.ExamResults)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.EnteredByUser)
                    .WithMany()
                    .HasForeignKey(e => e.EnteredBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Announcement
            modelBuilder.Entity<Announcement>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Body).IsRequired();
                entity.Property(e => e.Scope).HasConversion<int>();
                entity.HasOne(e => e.Class)
                    .WithMany(c => c.Announcements)
                    .HasForeignKey(e => e.ClassId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.Author)
                    .WithMany(u => u.Announcements)
                    .HasForeignKey(e => e.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // AnnouncementReadStatus
            modelBuilder.Entity<AnnouncementReadStatus>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.AnnouncementId, e.UserId }).IsUnique();
                entity.HasOne(e => e.Announcement)
                    .WithMany(a => a.ReadStatuses)
                    .HasForeignKey(e => e.AnnouncementId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.User)
                    .WithMany(u => u.AnnouncementReadStatuses)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
