using SMS.Domain.Enums;

namespace SMS.Domain.Entities
{
    public class Announcement
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public AnnouncementScope Scope { get; set; }
        public Guid? ClassId { get; set; }
        public Guid AuthorId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Class? Class { get; set; }
        public User Author { get; set; } = null!;
        public List<AnnouncementReadStatus> ReadStatuses { get; set; } = new List<AnnouncementReadStatus>();
    }
}
