using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Telegram.Bot.YouTuber.Webhook.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class MediaEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudioFormat",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "AudioQuality",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "AudioUrl",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "Extension",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "VideoFormat",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "VideoQuality",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "VideoUrl",
                table: "Sessions");

            migrationBuilder.AddColumn<Guid>(
                name: "AudioId",
                table: "Sessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VideoId",
                table: "Sessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Media",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    InternalUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Quality = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Format = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Extension = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Media", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Media_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Media_SessionId",
                table: "Media",
                column: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Media");

            migrationBuilder.DropColumn(
                name: "AudioId",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "VideoId",
                table: "Sessions");

            migrationBuilder.AddColumn<string>(
                name: "AudioFormat",
                table: "Sessions",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AudioQuality",
                table: "Sessions",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AudioUrl",
                table: "Sessions",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Extension",
                table: "Sessions",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Sessions",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VideoFormat",
                table: "Sessions",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VideoQuality",
                table: "Sessions",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VideoUrl",
                table: "Sessions",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);
        }
    }
}
