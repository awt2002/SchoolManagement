namespace SMS.Domain.Entities
{
    public class AnnouncementReadStatus
    {
        public Guid Id { get; set; }
        public Guid AnnouncementId { get; set; }
        public Guid UserId { get; set; }
        public DateTime ReadAt { get; set; } = DateTime.UtcNow;
        public Announcement Announcement { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
