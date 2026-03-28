using Microsoft.EntityFrameworkCore;
using SMS.API.Extensions;
using SMS.API.Middleware;
using SMS.Infrastructure.Data;
using SMS.Infrastructure.Seed;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

// Register application services (DB, Auth, JWT, etc.)
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// Ensure database is created and seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        if (await db.Database.CanConnectAsync())
        {
            var applied = await db.Database.GetAppliedMigrationsAsync();
            if (applied.Any())
            {
                var pending = await db.Database.GetPendingMigrationsAsync();
                if (pending.Any())
                {
                    await db.Database.MigrateAsync();
                }
            }
        }
        else
        {
            await db.Database.MigrateAsync();
        }
    }
    catch
    {
        // Database already exists or migrations cannot be applied
    }

    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
