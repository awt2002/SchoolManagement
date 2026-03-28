using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using SMS.Blazor;
using SMS.Blazor.Auth;
using SMS.Blazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();
builder.Services.AddScoped<JwtAuthStateProvider>(sp =>
    (JwtAuthStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());
builder.Services.AddAuthorizationCore();

// HTTP client services
builder.Services.AddScoped<AuthHttpService>();
builder.Services.AddScoped<StudentHttpService>();
builder.Services.AddScoped<TeacherHttpService>();
builder.Services.AddScoped<ClassHttpService>();
builder.Services.AddScoped<AcademicYearHttpService>();
builder.Services.AddScoped<AttendanceHttpService>();
builder.Services.AddScoped<SubjectHttpService>();
builder.Services.AddScoped<GradeHttpService>();
builder.Services.AddScoped<ExamHttpService>();
builder.Services.AddScoped<AnnouncementHttpService>();
builder.Services.AddScoped<DashboardHttpService>();

await builder.Build().RunAsync();
