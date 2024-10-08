﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using ShazamRemake.Data;

#nullable disable

namespace ShazamRemake.Migrations
{
    [DbContext(typeof(SpazamDbContext))]
    partial class SpazamDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ShazamRemake.Data.ChunkHash", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("ChunkIndex")
                        .HasColumnType("integer");

                    b.Property<string>("Hash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("KeyPoints")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("SongId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("SongId");

                    b.ToTable("ChunkHashes");
                });

            modelBuilder.Entity("ShazamRemake.Data.Song", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("FilePath")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("UploadedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("Songs");
                });

            modelBuilder.Entity("ShazamRemake.Data.ChunkHash", b =>
                {
                    b.HasOne("ShazamRemake.Data.Song", "Song")
                        .WithMany("ChunkHashes")
                        .HasForeignKey("SongId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Song");
                });

            modelBuilder.Entity("ShazamRemake.Data.Song", b =>
                {
                    b.Navigation("ChunkHashes");
                });
#pragma warning restore 612, 618
        }
    }
}
