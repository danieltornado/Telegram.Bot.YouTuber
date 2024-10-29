using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Telegram.Bot.YouTuber.Webhook.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatId = table.Column<long>(type: "bigint", nullable: true),
                    MessageId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    VideoQuality = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    VideoUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    AudioQuality = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    AudioUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Json = table.Column<string>(type: "jsonb", nullable: true),
                    JsonVideo = table.Column<string>(type: "jsonb", nullable: true),
                    JsonAudio = table.Column<string>(type: "jsonb", nullable: true),
                    Title = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Extension = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    Error = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sessions");
        }
    }
}
