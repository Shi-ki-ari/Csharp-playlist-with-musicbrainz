using FirstRESTApi.Data;
using FirstRESTApi.Services;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
var dbPath = Path.Combine(folder, "playlist.db");

builder.Services.AddDbContext<ApiContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

builder.Services.AddHttpClient<IMusicBrainzService, MusicBrainzService>();

builder.WebHost.UseUrls("http://localhost:5000");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApiContext>();
    db.Database.Migrate();
}

app.UseCors("AllowAll");

app.UseDefaultFiles();

app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

var url = "http://localhost:5000/mainpage.html";

try
{
    using var process = new System.Diagnostics.Process();
    process.StartInfo.FileName = url;
    process.StartInfo.UseShellExecute = true;
    process.Start();
}
catch (Exception ex)
{
    Console.WriteLine("Failed to launch browser: " + ex.Message);
}



app.Run();
