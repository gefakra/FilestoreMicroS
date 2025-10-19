using FilestoreMicroS.Data;
using FilestoreMicroS.Services;
using FilestoreMicroS.Services.Interface;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// SQLite connection
builder.Services.AddDbContext<FileStoreContext>(options =>
options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite") ?? "Data Source=filestore.db"));

builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));

builder.Services.AddScoped<IFileProcessingService, ChannelFileProcessingService>();
builder.Services.AddScoped<IStorageService, FileSystemStorageService>();
builder.Services.AddScoped<IFileRepository, EfFileRepository>();
builder.Services.AddScoped<IFileService, FileService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FileStoreContext>();
    db.Database.EnsureCreated();
}

// Swagger in dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();
