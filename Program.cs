using FileStoreService.Models;
using FileStoreService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.Configure<FileStorageOptions>(builder.Configuration.GetSection("FileStorage"));

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services
builder.Services.AddScoped<IFileStoreService, FileStoreService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FileStore API", Version = "v1" });
    // optionally show xml comments etc.
});

var app = builder.Build();

// Ensure DB exists and apply migrations (for dev/demo)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment() || true)
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FileStore v1"));
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();
