using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace SoundCaseOpener.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class V2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "chance",
                schema: "SoundCaseOpener",
                table: "case_item",
                newName: "weight");

            migrationBuilder.AddColumn<Instant>(
                name: "last_time_used",
                schema: "SoundCaseOpener",
                table: "sounds",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_time_used",
                schema: "SoundCaseOpener",
                table: "sounds");

            migrationBuilder.RenameColumn(
                name: "weight",
                schema: "SoundCaseOpener",
                table: "case_item",
                newName: "chance");
        }
    }
}
