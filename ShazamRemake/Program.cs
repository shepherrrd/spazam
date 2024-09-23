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
    var filePath = Path.Combine("uploads", audioFile.FileName);
    await using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
    {
        await audioFile.CopyToAsync(stream);
    }

    var songId = Guid.NewGuid();
    var chunks = audioProcessor.BreakIntoChunks(filePath, 5); 
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

    db.Songs.Add(new Song { Id = songId, Title = audioFile.FileName, FilePath = filePath, UploadedAt = DateTime.UtcNow });
    await db.SaveChangesAsync();

    return Results.Ok(new { status = "Song uploaded and processed", songId });
}).DisableAntiforgery().WithName("UploadSong")
.WithTags("Admin");


app.MapPost("/identify-chunk", async (IFormFile audioChunk, AudioProcessor audioProcessor, SpazamDbContext db) =>
{
    if (audioChunk == null || audioChunk.Length == 0)
        return Results.BadRequest("No audio chunk provided.");

    // Save the chunk and extract key points
    var filePath = Path.Combine("uploads", audioChunk.FileName);
    await using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
    {
        await audioChunk.CopyToAsync(stream);
    }

    var chunks = audioProcessor.BreakIntoChunks(filePath, 5);
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
    

    return Results.NotFound(new { status = false });
}).AllowAnonymous().DisableAntiforgery()
.WithName("IdentifyChunk")
.WithTags("Identification");
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");
app.Run();
