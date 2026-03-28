namespace SMS.Domain.Entities
{
    public class AcademicYear
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public bool IsActive { get; set; }
        public List<Class> Classes { get; set; } = new List<Class>();
    }
}
