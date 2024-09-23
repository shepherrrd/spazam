using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ShazamRemake.Data;
public class Song
{
    [Key]
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string FilePath { get; set; }
    public DateTime UploadedAt { get; set; }

    public ICollection<ChunkHash> ChunkHashes { get; set; }
}

public class ChunkHash
{
    [Key]
    public Guid Id { get; set; }
    public Guid SongId { get; set; }
    public string Hash { get; set; }
    public int ChunkIndex { get; set; }
    public string KeyPoints { get; set; }
    public Song Song { get; set; }
}

public class SpazamDbContext : DbContext
{
    public SpazamDbContext(DbContextOptions<SpazamDbContext> options) : base(options) { }

    public DbSet<Song> Songs { get; set; }
    public DbSet<ChunkHash> ChunkHashes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Song>()
            .HasMany(s => s.ChunkHashes)
            .WithOne(ch => ch.Song)
            .HasForeignKey(ch => ch.SongId);

        base.OnModelCreating(modelBuilder);
    }
}
