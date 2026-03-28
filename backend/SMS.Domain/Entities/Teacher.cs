namespace SMS.Domain.Entities
{
    public class Teacher
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public Guid? ClassId { get; set; }
        public User User { get; set; } = null!;
        public Class? Class { get; set; }
    }
}
