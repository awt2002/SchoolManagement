namespace SMS.Application.Features.Dashboard.DTOs
{
    public class AdminDashboardDto
    {
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalClasses { get; set; }
        public int AbsencesToday { get; set; }
        public int UnreadAnnouncements { get; set; }
        public List<DashboardAnnouncementDto> RecentAnnouncements { get; set; } = new List<DashboardAnnouncementDto>();
    }

    public class TeacherDashboardDto
    {
        public int ClassStudentCount { get; set; }
        public int AbsencesTodayInClass { get; set; }
        public List<DashboardExamDto> UpcomingExams { get; set; } = new List<DashboardExamDto>();
        public int UnreadAnnouncements { get; set; }
        public List<DashboardAnnouncementDto> RecentAnnouncements { get; set; } = new List<DashboardAnnouncementDto>();
    }

    public class StudentDashboardDto
    {
        public int TotalAbsencesThisYear { get; set; }
        public decimal Gpa { get; set; }
        public DashboardExamDto? NextExam { get; set; }
        public int UnreadAnnouncements { get; set; }
        public List<DashboardAnnouncementDto> RecentAnnouncements { get; set; } = new List<DashboardAnnouncementDto>();
    }

    public class DashboardAnnouncementDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }

    public class DashboardExamDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public DateOnly ExamDate { get; set; }
    }
}
