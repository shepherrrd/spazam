using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShazamRemake.Data;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;
var parentPath = Directory.GetParent(env.ContentRootPath)?.FullName ?? "";
builder.Configuration
    .AddJsonFile("appsettings.json", true);
builder.Configuration.AddEnvironmentVariables();
builder.Services.AddDbContext<SpazamDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

builder.Services.AddSingleton<AudioProcessor>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
var app = builder.Build();


app.MapPost("/admin/upload-song", async (IFormFile audioFile, AudioProcessor audioProcessor, SpazamDbContext db) =>
{
    if (audioFile == null || audioFile.Length == 0)
        return Results.BadRequest("No audio file provided.");

    db.ChunkHashes.ExecuteDelete();
    var tempFilePath = Path.GetTempFileName();

    try
    {
        await using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
        {
            await audioFile.CopyToAsync(fileStream);
        }

        var songId = Guid.NewGuid();
        var chunks = audioProcessor.BreakIntoChunks(tempFilePath, 5);

        foreach (var chunk in chunks)
        {
            var keyPoints = audioProcessor.ExtractKeyPoints(chunk.Data);
            var hash = audioProcessor.HashKeyPoints(keyPoints);
            db.ChunkHashes.Add(new ChunkHash
            {
                Id = Guid.NewGuid(),
                SongId = songId,
                Hash = hash,
                KeyPoints = string.Join(",", keyPoints.Select(kp => $"{kp.Item1}:{kp.Item2}")),
                ChunkIndex = chunk.Index
            });
        }

        db.Songs.Add(new Song
        {
            Id = songId,
            Title = audioFile.FileName,
            FilePath = "InMemory",  
            UploadedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
    }
    finally
    {
        if (File.Exists(tempFilePath))
        {
            File.Delete(tempFilePath);
        }
    }

    return Results.Ok(new { status = true, message = "Song processed and chunks stored." });

}).DisableAntiforgery().WithName("UploadSong")
.WithTags("Admin");


app.MapPost("/identify-chunk", async (IFormFile audioChunk, AudioProcessor audioProcessor, SpazamDbContext db) =>
{
    if (audioChunk == null || audioChunk.Length == 0)
        return Results.BadRequest("No audio chunk provided.");

    await using (var memoryStream = new MemoryStream())
    {
        await audioChunk.CopyToAsync(memoryStream);
        memoryStream.Position = 0; 
        var chunks = audioProcessor.BreakIntoChunks(memoryStream, 5);
        foreach (var chunk in chunks)
        {
            var keyPoints = audioProcessor.ExtractKeyPoints(chunk.Data);
            var hash = audioProcessor.HashKeyPoints(keyPoints);
            var match = await db.ChunkHashes
                .Include(c => c.Song)
                .FirstOrDefaultAsync(c => c.Hash == hash);
            if (match != null)
            {
                return Results.Ok(new { status = true, songdetails = match.Song.Title });
            }
        }
    }

    return Results.NotFound(new { status = false, message = "No matching song found." });
}).AllowAnonymous().DisableAntiforgery()
.WithName("IdentifyChunk")
.WithTags("Identification");
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");
app.Run();
