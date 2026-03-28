using SMS.Domain.Enums;

namespace SMS.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public Student? Student { get; set; }
        public Teacher? Teacher { get; set; }
        public List<Announcement> Announcements { get; set; } = new List<Announcement>();
        public List<AnnouncementReadStatus> AnnouncementReadStatuses { get; set; } = new List<AnnouncementReadStatus>();
    }
}
