using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using FluentValidation;
using SMS.Application.Features.Attendance.Commands;
using SMS.Application.Features.Attendance.DTOs;
using SMS.Application.Features.Attendance.Queries;
using SMS.Application.Features.Attendance.Validators;
using SMS.Application.Features.Grades.DTOs;
using SMS.Application.Features.Grades.Validators;
using SMS.Application.Features.Students.Commands;
using SMS.Application.Features.Students.DTOs;
using SMS.Application.Features.Students.Queries;
using SMS.Application.Features.Students.Validators;
using SMS.Application.Interfaces;
using SMS.Infrastructure.Data;
using SMS.Infrastructure.Seed;
using SMS.Infrastructure.Services;
using SMS.API.Middleware;

namespace SMS.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Database
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("Default"),
                    sqlOptions => sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

            // Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<ITeacherService, TeacherService>();
            services.AddScoped<IClassService, ClassService>();
            services.AddScoped<IAcademicYearService, AcademicYearService>();
            services.AddScoped<IAttendanceService, AttendanceService>();
            services.AddScoped<ISubjectService, SubjectService>();
            services.AddScoped<IGradeService, GradeService>();
            services.AddScoped<IExamService, ExamService>();
            services.AddScoped<IAnnouncementService, AnnouncementService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ICurrentUser, CurrentUser>();

            // Students CQRS handlers
            services.AddScoped<CreateStudentCommandHandler>();
            services.AddScoped<UpdateStudentCommandHandler>();
            services.AddScoped<GetStudentsQueryHandler>();
            services.AddScoped<GetStudentByIdQueryHandler>();
            services.AddScoped<CreateAttendanceCommandHandler>();
            services.AddScoped<DeleteAttendanceCommandHandler>();
            services.AddScoped<GetAttendanceQueryHandler>();
            services.AddScoped<GetStudentAttendanceSummaryQueryHandler>();

            // Validators
            services.AddScoped<IValidator<CreateStudentDto>, CreateStudentDtoValidator>();
            services.AddScoped<UpdateStudentCommandValidator>();
            services.AddScoped<IValidator<CreateGradeDto>, CreateGradeDtoValidator>();
            services.AddScoped<IValidator<CreateAttendanceDto>, CreateAttendanceDtoValidator>();
            services.AddScoped<DatabaseSeeder>();

            // JWT Authentication
            var jwtSecret = configuration["Jwt:Secret"] ?? "SuperSecretKeyThatIsLongEnoughForHmacSha256Algorithm!";
            var key = Encoding.UTF8.GetBytes(jwtSecret);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"] ?? "SMS",
                    ValidAudience = configuration["Jwt:Audience"] ?? "SMS",
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };
            });

            services.AddAuthorization();

            return services;
        }
    }
}
