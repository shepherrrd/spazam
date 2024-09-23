using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShazamRemake.Migrations
{
    /// <inheritdoc />
    public partial class CHunck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KeyPoints",
                table: "ChunkHashes",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KeyPoints",
                table: "ChunkHashes");
        }
    }
}
