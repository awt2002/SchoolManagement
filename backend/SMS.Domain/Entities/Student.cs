namespace SMS.Domain.Entities
{
    public class Student
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public string Address { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public int EnrollmentYear { get; set; }
        public User User { get; set; } = null!;
        public ParentContact? ParentContact { get; set; }
        public List<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public List<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
        public List<Grade> Grades { get; set; } = new List<Grade>();
        public List<ExamResult> ExamResults { get; set; } = new List<ExamResult>();

        public Enrollment EnrollStudent(Guid classId, Guid academicYearId)
        {
            if (classId == Guid.Empty)
            {
                throw new ArgumentException("ClassId is required", nameof(classId));
            }

            if (academicYearId == Guid.Empty)
            {
                throw new ArgumentException("AcademicYearId is required", nameof(academicYearId));
            }

            var alreadyEnrolled = Enrollments.Any(e => e.ClassId == classId && e.AcademicYearId == academicYearId);
            if (alreadyEnrolled)
            {
                throw new InvalidOperationException("Student is already enrolled in this class for the selected academic year.");
            }

            var enrollment = new Enrollment
            {
                Id = Guid.NewGuid(),
                StudentId = Id,
                ClassId = classId,
                AcademicYearId = academicYearId,
                EnrolledAt = DateTime.UtcNow
            };

            Enrollments.Add(enrollment);
            return enrollment;
        }

        public AttendanceRecord MarkAttendance(Guid classId, DateOnly absenceDate, Guid recordedBy)
        {
            if (classId == Guid.Empty)
            {
                throw new ArgumentException("ClassId is required", nameof(classId));
            }

            if (recordedBy == Guid.Empty)
            {
                throw new ArgumentException("RecordedBy is required", nameof(recordedBy));
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (absenceDate > today)
            {
                throw new InvalidOperationException("Absence date cannot be in the future.");
            }

            var record = new AttendanceRecord
            {
                Id = Guid.NewGuid(),
                StudentId = Id,
                ClassId = classId,
                AbsenceDate = absenceDate,
                RecordedBy = recordedBy,
                RecordedAt = DateTime.UtcNow
            };

            AttendanceRecords.Add(record);
            return record;
        }

        public Grade AssignGrade(Guid gradeCategoryId, decimal score, Guid enteredBy)
        {
            if (gradeCategoryId == Guid.Empty)
            {
                throw new ArgumentException("GradeCategoryId is required", nameof(gradeCategoryId));
            }

            if (enteredBy == Guid.Empty)
            {
                throw new ArgumentException("EnteredBy is required", nameof(enteredBy));
            }

            if (score < 0 || score > 100)
            {
                throw new InvalidOperationException("Score must be between 0 and 100.");
            }

            var grade = new Grade
            {
                Id = Guid.NewGuid(),
                StudentId = Id,
                GradeCategoryId = gradeCategoryId,
                Score = score,
                EnteredBy = enteredBy,
                EnteredAt = DateTime.UtcNow
            };

            Grades.Add(grade);
            return grade;
        }
    }
}
