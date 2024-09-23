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

var app = builder.Build();


app.MapPost("/admin/upload-song", async (IFormFile audioFile,[FromForm]string token, [FromForm] string songname, AudioProcessor audioProcessor, SpazamDbContext db) =>
{
    var supersecrettoken = app.Configuration.GetValue<string>("Settings:Token");
    if(token != supersecrettoken )
        return Results.Unauthorized();
    if (audioFile == null || audioFile.Length == 0)
        return Results.BadRequest("No audio file provided.");

    var filePath = Path.Combine("uploads", audioFile.FileName);
    using (var stream = File.Create(filePath))
    {
        await audioFile.CopyToAsync(stream);
    }


    var song = new Song
    {
        Id = Guid.NewGuid(),
        Title = songname,
        FilePath = filePath, //would likely use a storage service in a real app
        UploadedAt = DateTime.UtcNow
    };

    var chunks = audioProcessor.BreakIntoChunks(filePath, 5);
    foreach (var chunk in chunks)
    {
        var chunkHash = audioProcessor.HashFingerprint(chunk.Data);
        db.ChunkHashes.Add(new ChunkHash
        {
            Id = Guid.NewGuid(),
            SongId = song.Id,
            Hash = chunkHash,
            ChunkIndex = chunk.Index
        });
    }

    db.Songs.Add(song);
    await db.SaveChangesAsync();

    return Results.Ok(new { songId = song.Id, status = "Song uploaded and hashed." });
}).DisableAntiforgery().WithName("UploadSong")
.WithTags("Admin");
;

app.MapPost("/identify-chunk", async (IFormFile audioChunk, AudioProcessor audioProcessor, SpazamDbContext db) =>
{
    if (audioChunk == null || audioChunk.Length == 0)
        return Results.BadRequest("No audio chunk provided.");
    var filePath = Path.Combine("uploads", audioChunk.FileName);
    using (var stream = File.Create(filePath))
    {
        await audioChunk.CopyToAsync(stream);
    }
    var chunks = audioProcessor.BreakIntoChunks(filePath, 5);
    var match = new ChunkHash();
    foreach (var chunk in chunks)
    {
        var chunkHash = audioProcessor.HashFingerprint(chunk.Data);
        var find = db.ChunkHashes.Include(x => x.Song).FirstOrDefault(x => x.Hash == chunkHash);
        if (find != null)
        {
            match = find;
            break;
        }
    }

    if (match != null)
    {
        var songDetails = match.Song;
        return Results.Ok(new { status = "match",name = songDetails.Title });
    }

    return Results.Ok(new { status = "no match" });
}).DisableAntiforgery()
.WithName("IdentifyChunk")
.WithTags("Identification");

app.UseSwagger();
app.UseSwaggerUI();
app.Run();
