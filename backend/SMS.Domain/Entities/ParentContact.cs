namespace SMS.Domain.Entities
{
    public class ParentContact
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public Student Student { get; set; } = null!;
    }
}
