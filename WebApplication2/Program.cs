using Microsoft.EntityFrameworkCore;
using IE3_P1.Data;
using Microsoft.Extensions.FileProviders;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<SqliteDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

//builder.Services.AddScoped<IStudentRepository, StudentRepository>();

// Allow hard-wired pages (http://localhost:8000) to call the API regardless of origin.
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI();
    app.UseSwagger();
}

// Serve hard-wired static pages. app.UseStaticFiles() BEFORE app.UseHttpsRedirection() / app.MapControllers().
app.UseDefaultFiles();                                  // app/wwwroot -> index.* etc.
app.UseStaticFiles();                                   // serves app/wwwroot if present
var repoRootWwwroot = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "wwwroot"));
if (Directory.Exists(repoRootWwwroot))
{
    // Default page (not MVC): serve repo-root wwwroot/instructions.html at "/".
    app.UseDefaultFiles(new DefaultFilesOptions
    {
        FileProvider = new PhysicalFileProvider(repoRootWwwroot),
        DefaultFileNames = new List<string> { "instructions.html", "index.html", "index.htm", "default.html", "default.htm" }
    });
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(repoRootWwwroot)   // repo-root wwwroot (this project keeps pages there)
    });
}

app.UseHttpsRedirection();

app.UseCors();                                          // hard-wired localhost:8000 pages can call API

app.UseAuthorization();

app.MapControllers();

// Minimal API (not MVC): expose repository instruction markdown files for instructions.html.
app.MapGet("/api/instructions", (IWebHostEnvironment env) =>
{
    var instructionsDir = Path.GetFullPath(Path.Combine(env.ContentRootPath, "..", "Instructions"));
    var files = new List<InstructionFile>();

    if (Directory.Exists(instructionsDir))
    {
        files = Directory.EnumerateFiles(instructionsDir, "*.md", SearchOption.TopDirectoryOnly)
            .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
            .Select(path => new InstructionFile(Path.GetFileName(path)!, File.ReadAllText(path)))
            .ToList();
    }

    return Results.Ok(files);
});

app.Run();

record InstructionFile(string Name, string Content);
