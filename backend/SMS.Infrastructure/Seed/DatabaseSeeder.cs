using Microsoft.EntityFrameworkCore;
using SMS.Domain.Entities;
using SMS.Domain.Enums;
using SMS.Infrastructure.Data;
using SMS.Infrastructure.Services;

namespace SMS.Infrastructure.Seed
{
    public class DatabaseSeeder
    {
        private readonly AppDbContext _context;

        public DatabaseSeeder(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (await _context.Users.AnyAsync())
            {
                return; // Already seeded
            }

            // Academic Year
            var academicYear = new AcademicYear
            {
                Id = Guid.NewGuid(),
                Name = "2025-2026",
                StartDate = new DateOnly(2025, 9, 1),
                EndDate = new DateOnly(2026, 6, 30),
                IsActive = true
            };
            _context.AcademicYears.Add(academicYear);

            // Admin user
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "admin",
                PasswordHash = AuthService.HashPassword("Admin123"),
                Email = "admin@school.com",
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(adminUser);

            // Classes
            var classes = new List<Class>();
            var classNames = new[]
            {
                ("Grade 1 - A", 1),
                ("Grade 2 - A", 2),
                ("Grade 3 - A", 3),
                ("Grade 4 - A", 4),
                ("Grade 5 - A", 5),
                ("Grade 6 - A", 6),
                ("Grade 7 - A", 7),
                ("Grade 8 - A", 8),
                ("Grade 9 - A", 9),
                ("Grade 10 - A", 10),
                ("Grade 11 - A", 11),
                ("Grade 12 - A", 12)
            };

            foreach (var (name, level) in classNames)
            {
                var cls = new Class
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    GradeLevel = level,
                    AcademicYearId = academicYear.Id
                };
                classes.Add(cls);
                _context.Classes.Add(cls);
            }

            // Teachers
            var teacherNames = new[]
            {
                ("Sarah Johnson", "sjohnson", "sjohnson@school.com"),
                ("Michael Brown", "mbrown", "mbrown@school.com"),
                ("Emily Davis", "edavis", "edavis@school.com"),
                ("Robert Wilson", "rwilson", "rwilson@school.com"),
                ("Lisa Anderson", "landerson", "landerson@school.com")
            };

            var teachers = new List<Teacher>();
            for (int i = 0; i < teacherNames.Length; i++)
            {
                var (fullName, username, email) = teacherNames[i];

                var teacherUser = new User
                {
                    Id = Guid.NewGuid(),
                    Username = username,
                    PasswordHash = AuthService.HashPassword("Teacher123"),
                    Email = email,
                    Role = UserRole.Teacher,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Users.Add(teacherUser);

                var teacher = new Teacher
                {
                    Id = Guid.NewGuid(),
                    UserId = teacherUser.Id,
                    FullName = fullName,
                    ClassId = i < classes.Count ? classes[i].Id : null
                };
                teachers.Add(teacher);
                _context.Teachers.Add(teacher);

                // Link class to teacher
                if (i < classes.Count)
                {
                    classes[i].TeacherId = teacher.Id;
                }
            }

            // Subjects per class
            var subjectNames = new[] { "Mathematics", "English", "Science", "History" };
            var allSubjects = new List<Subject>();

            foreach (var cls in classes)
            {
                foreach (var subjectName in subjectNames)
                {
                    var subject = new Subject
                    {
                        Id = Guid.NewGuid(),
                        Name = subjectName,
                        ClassId = cls.Id
                    };
                    allSubjects.Add(subject);
                    _context.Subjects.Add(subject);

                    // Grade categories for each subject
                    var categories = new[]
                    {
                        ("Homework", 20m),
                        ("Quizzes", 20m),
                        ("Midterm", 30m),
                        ("Final Exam", 30m)
                    };

                    foreach (var (catName, weight) in categories)
                    {
                        var category = new GradeCategory
                        {
                            Id = Guid.NewGuid(),
                            SubjectId = subject.Id,
                            Name = catName,
                            Weight = weight
                        };
                        _context.GradeCategories.Add(category);
                    }
                }
            }

            // Students
            var studentData = new[]
            {
                ("Alice Smith", 0), ("Bob Taylor", 0), ("Charlie White", 0),
                ("Diana Green", 0), ("Edward Black", 0), ("Fiona Clark", 0),
                ("George Hall", 0), ("Hannah King", 1), ("Ian Wright", 1),
                ("Julia Adams", 1), ("Kevin Lee", 1), ("Laura Baker", 1),
                ("Mason Hill", 1), ("Nina Scott", 1), ("Oscar Turner", 2),
                ("Penny Young", 2), ("Quinn Martin", 2), ("Rachel Moore", 2),
                ("Samuel Evans", 2), ("Tina Walker", 2), ("Ulrich Gray", 3),
                ("Vera Collins", 3), ("William Fox", 3), ("Xena Diaz", 3),
                ("Yuri Patel", 3), ("Zoe Murphy", 3), ("Aaron Reed", 0),
                ("Bella Cooper", 1), ("Caleb Ross", 2), ("Daisy Ward", 3)
            };

            var students = new List<Student>();
            var random = new Random(42);

            for (int i = 0; i < studentData.Length; i++)
            {
                var (fullName, classIndex) = studentData[i];
                var username = fullName.Replace(" ", "").ToLower();

                var studentUser = new User
                {
                    Id = Guid.NewGuid(),
                    Username = username,
                    PasswordHash = AuthService.HashPassword("Student123"),
                    Email = $"{username}@school.com",
                    Role = UserRole.Student,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Users.Add(studentUser);

                var student = new Student
                {
                    Id = Guid.NewGuid(),
                    UserId = studentUser.Id,
                    FullName = fullName,
                    DateOfBirth = new DateOnly(2010 - classIndex * 2, random.Next(1, 12), random.Next(1, 28)),
                    Address = $"{random.Next(100, 999)} Main Street, City",
                    EnrollmentYear = 2025
                };
                students.Add(student);
                _context.Students.Add(student);

                // Parent contact
                var parentContact = new ParentContact
                {
                    Id = Guid.NewGuid(),
                    StudentId = student.Id,
                    FullName = $"Parent of {fullName}",
                    Email = $"parent.{username}@email.com"
                };
                _context.ParentContacts.Add(parentContact);

                // Enrollment
                var enrollment = new Enrollment
                {
                    Id = Guid.NewGuid(),
                    StudentId = student.Id,
                    ClassId = classes[classIndex].Id,
                    AcademicYearId = academicYear.Id,
                    EnrolledAt = DateTime.UtcNow
                };
                _context.Enrollments.Add(enrollment);
            }

            await _context.SaveChangesAsync();

            // Now add grades, attendance, exams with saved data
            var allCategories = await _context.GradeCategories.ToListAsync();
            var allStudents = await _context.Students
                .Include(s => s.Enrollments)
                .ToListAsync();

            // Grades
            foreach (var student in allStudents)
            {
                var studentClassId = student.Enrollments.FirstOrDefault()?.ClassId;
                if (studentClassId == null) continue;

                var classSubjects = allSubjects.Where(s => s.ClassId == studentClassId).ToList();

                foreach (var subject in classSubjects)
                {
                    var categories = allCategories.Where(c => c.SubjectId == subject.Id).ToList();
                    foreach (var category in categories)
                    {
                        var score = random.Next(60, 100);
                        var grade = new Grade
                        {
                            Id = Guid.NewGuid(),
                            StudentId = student.Id,
                            GradeCategoryId = category.Id,
                            Score = score,
                            EnteredBy = adminUser.Id,
                            EnteredAt = DateTime.UtcNow
                        };
                        _context.Grades.Add(grade);
                    }
                }
            }

            // Attendance - at least one absence per student
            foreach (var student in allStudents)
            {
                var studentClassId = student.Enrollments.FirstOrDefault()?.ClassId;
                if (studentClassId == null) continue;

                var absenceCount = random.Next(1, 5);
                for (int j = 0; j < absenceCount; j++)
                {
                    var daysAgo = random.Next(1, 90);
                    var absenceDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-daysAgo));

                    // Make sure no duplicate
                    var exists = await _context.AttendanceRecords
                        .AnyAsync(a => a.StudentId == student.Id && a.AbsenceDate == absenceDate);
                    if (exists) continue;

                    var record = new AttendanceRecord
                    {
                        Id = Guid.NewGuid(),
                        StudentId = student.Id,
                        ClassId = studentClassId.Value,
                        AbsenceDate = absenceDate,
                        RecordedBy = adminUser.Id,
                        RecordedAt = DateTime.UtcNow
                    };
                    _context.AttendanceRecords.Add(record);
                }
            }

            // Exams
            foreach (var subject in allSubjects)
            {
                var exam = new Exam
                {
                    Id = Guid.NewGuid(),
                    SubjectId = subject.Id,
                    Name = $"{subject.Name} Midterm",
                    ExamDate = new DateOnly(2025, 11, random.Next(1, 28)),
                    MaxScore = 100,
                    PassingThreshold = 50,
                    CreatedBy = adminUser.Id
                };
                _context.Exams.Add(exam);

                // Get students in this class
                var classStudents = allStudents
                    .Where(s => s.Enrollments.Any(e => e.ClassId == subject.ClassId))
                    .ToList();

                foreach (var student in classStudents)
                {
                    var examResult = new ExamResult
                    {
                        Id = Guid.NewGuid(),
                        ExamId = exam.Id,
                        StudentId = student.Id,
                        Score = random.Next(40, 100),
                        EnteredBy = adminUser.Id,
                        EnteredAt = DateTime.UtcNow
                    };
                    _context.ExamResults.Add(examResult);
                }
            }

            // Announcements
            var schoolAnnouncement = new Announcement
            {
                Id = Guid.NewGuid(),
                Title = "Welcome to the 2025-2026 Academic Year!",
                Body = "We are excited to welcome all students and staff to the new academic year. Let's make this year great!",
                Scope = AnnouncementScope.SchoolWide,
                AuthorId = adminUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            };
            _context.Announcements.Add(schoolAnnouncement);

            var holidayAnnouncement = new Announcement
            {
                Id = Guid.NewGuid(),
                Title = "Holiday Schedule Update",
                Body = "Please note the updated holiday schedule for the winter break. School will close on December 20th and reopen on January 5th.",
                Scope = AnnouncementScope.SchoolWide,
                AuthorId = adminUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            };
            _context.Announcements.Add(holidayAnnouncement);

            // Class announcements
            for (int i = 0; i < classes.Count && i < teachers.Count; i++)
            {
                var classAnnouncement = new Announcement
                {
                    Id = Guid.NewGuid(),
                    Title = $"Upcoming Test in {classes[i].Name}",
                    Body = $"There will be a test next week covering chapters 5-8. Please prepare accordingly.",
                    Scope = AnnouncementScope.ClassOnly,
                    ClassId = classes[i].Id,
                    AuthorId = teachers[i].UserId,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                };
                _context.Announcements.Add(classAnnouncement);
            }

            await _context.SaveChangesAsync();
        }

        private static string HashPassword(string password)
        {
            return AuthService.HashPassword(password);
        }
    }
}
