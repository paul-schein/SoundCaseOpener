using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SoundCaseOpener.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "SoundCaseOpener");

            migrationBuilder.CreateSequence(
                name: "ItemSequence",
                schema: "SoundCaseOpener");

            migrationBuilder.CreateSequence(
                name: "ItemTemplateSequence",
                schema: "SoundCaseOpener");

            migrationBuilder.CreateTable(
                name: "case_templates",
                schema: "SoundCaseOpener",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "nextval('\"SoundCaseOpener\".\"ItemTemplateSequence\"')"),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    rarity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_case_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "item_templates",
                schema: "SoundCaseOpener",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "nextval('\"SoundCaseOpener\".\"ItemTemplateSequence\"')"),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    rarity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sound_file",
                schema: "SoundCaseOpener",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    file_path = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sound_file", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                schema: "SoundCaseOpener",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "case_item",
                schema: "SoundCaseOpener",
                columns: table => new
                {
                    case_template_id = table.Column<int>(type: "integer", nullable: false),
                    item_template_id = table.Column<int>(type: "integer", nullable: false),
                    chance = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_case_item", x => new { x.case_template_id, x.item_template_id });
                    table.ForeignKey(
                        name: "fk_case_item_case_templates_case_template_id",
                        column: x => x.case_template_id,
                        principalSchema: "SoundCaseOpener",
                        principalTable: "case_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sound_templates",
                schema: "SoundCaseOpener",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "nextval('\"SoundCaseOpener\".\"ItemTemplateSequence\"')"),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    rarity = table.Column<int>(type: "integer", nullable: false),
                    min_cooldown = table.Column<int>(type: "integer", nullable: false),
                    max_cooldown = table.Column<int>(type: "integer", nullable: false),
                    sound_file_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sound_templates", x => x.id);
                    table.ForeignKey(
                        name: "fk_sound_templates_sound_file_sound_file_id",
                        column: x => x.sound_file_id,
                        principalSchema: "SoundCaseOpener",
                        principalTable: "sound_file",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cases",
                schema: "SoundCaseOpener",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "nextval('\"SoundCaseOpener\".\"ItemSequence\"')"),
                    name = table.Column<string>(type: "text", nullable: false),
                    owner_id = table.Column<int>(type: "integer", nullable: false),
                    template_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cases", x => x.id);
                    table.ForeignKey(
                        name: "FK_cases_user_owner_id",
                        column: x => x.owner_id,
                        principalSchema: "SoundCaseOpener",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "items",
                schema: "SoundCaseOpener",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "nextval('\"SoundCaseOpener\".\"ItemSequence\"')"),
                    name = table.Column<string>(type: "text", nullable: false),
                    owner_id = table.Column<int>(type: "integer", nullable: false),
                    template_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_items_user_owner_id",
                        column: x => x.owner_id,
                        principalSchema: "SoundCaseOpener",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sounds",
                schema: "SoundCaseOpener",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "nextval('\"SoundCaseOpener\".\"ItemSequence\"')"),
                    name = table.Column<string>(type: "text", nullable: false),
                    owner_id = table.Column<int>(type: "integer", nullable: false),
                    template_id = table.Column<int>(type: "integer", nullable: false),
                    cooldown = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sounds", x => x.id);
                    table.ForeignKey(
                        name: "FK_sounds_user_owner_id",
                        column: x => x.owner_id,
                        principalSchema: "SoundCaseOpener",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_case_item_item_template_id",
                schema: "SoundCaseOpener",
                table: "case_item",
                column: "item_template_id");

            migrationBuilder.CreateIndex(
                name: "IX_cases_owner_id",
                schema: "SoundCaseOpener",
                table: "cases",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_cases_template_id",
                schema: "SoundCaseOpener",
                table: "cases",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "IX_items_owner_id",
                schema: "SoundCaseOpener",
                table: "items",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_items_template_id",
                schema: "SoundCaseOpener",
                table: "items",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "ix_sound_templates_sound_file_id",
                schema: "SoundCaseOpener",
                table: "sound_templates",
                column: "sound_file_id");

            migrationBuilder.CreateIndex(
                name: "IX_sounds_owner_id",
                schema: "SoundCaseOpener",
                table: "sounds",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_sounds_template_id",
                schema: "SoundCaseOpener",
                table: "sounds",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_username",
                schema: "SoundCaseOpener",
                table: "user",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "case_item",
                schema: "SoundCaseOpener");

            migrationBuilder.DropTable(
                name: "cases",
                schema: "SoundCaseOpener");

            migrationBuilder.DropTable(
                name: "item_templates",
                schema: "SoundCaseOpener");

            migrationBuilder.DropTable(
                name: "items",
                schema: "SoundCaseOpener");

            migrationBuilder.DropTable(
                name: "sound_templates",
                schema: "SoundCaseOpener");

            migrationBuilder.DropTable(
                name: "sounds",
                schema: "SoundCaseOpener");

            migrationBuilder.DropTable(
                name: "case_templates",
                schema: "SoundCaseOpener");

            migrationBuilder.DropTable(
                name: "sound_file",
                schema: "SoundCaseOpener");

            migrationBuilder.DropTable(
                name: "user",
                schema: "SoundCaseOpener");

            migrationBuilder.DropSequence(
                name: "ItemSequence",
                schema: "SoundCaseOpener");

            migrationBuilder.DropSequence(
                name: "ItemTemplateSequence",
                schema: "SoundCaseOpener");
        }
    }
}
