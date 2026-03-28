namespace SMS.Application.Features.Students.Queries
{
    public class GetStudentsQuery
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public int? GradeLevel { get; set; }
        public Guid? ClassId { get; set; }
        public Guid? AcademicYearId { get; set; }
        public bool IncludeInactive { get; set; }
        public bool InactiveOnly { get; set; }
    }
}
