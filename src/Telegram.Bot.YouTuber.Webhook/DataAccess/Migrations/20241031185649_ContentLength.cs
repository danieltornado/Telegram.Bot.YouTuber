using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Telegram.Bot.YouTuber.Webhook.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ContentLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ContentLength",
                table: "Media",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "AudioContentLength",
                table: "Downloading",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "VideoContentLength",
                table: "Downloading",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentLength",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "AudioContentLength",
                table: "Downloading");

            migrationBuilder.DropColumn(
                name: "VideoContentLength",
                table: "Downloading");
        }
    }
}
