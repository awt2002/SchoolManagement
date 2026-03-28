using SMS.Domain.Enums;

namespace SMS.Application.Features.Announcements.DTOs
{
    public class CreateAnnouncementDto
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public AnnouncementScope Scope { get; set; }
        public Guid? ClassId { get; set; }
    }

    public class AnnouncementDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public Guid? ClassId { get; set; }
        public string? ClassName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }

    public class UnreadCountDto
    {
        public int Count { get; set; }
    }
}
