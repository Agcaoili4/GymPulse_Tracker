using Microsoft.EntityFrameworkCore;
using GymPulse.Api.Data;
using GymPulse.Api.Services;

var builder = WebApplication.CreateBuilder(args);

const string FrontendCorsPolicy = "FrontendDev";

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IClubService, ClubService>();

// Configure CORS to allow request from the frontend development server
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    Console.WriteLine("Development startup seed: creating scope...");
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    Console.WriteLine("Development startup seed: applying migrations...");
    await dbContext.Database.MigrateAsync();
    Console.WriteLine("Development startup seed: seeding clubs...");
    await Seeder.SeedAsync(dbContext);
    Console.WriteLine("Development startup seed: done.");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(FrontendCorsPolicy);

app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.MapControllers();

app.Run();
