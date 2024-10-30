using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Telegram.Bot.YouTuber.Webhook.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedFormatFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AudioFormat",
                table: "Sessions",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VideoFormat",
                table: "Sessions",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudioFormat",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "VideoFormat",
                table: "Sessions");
        }
    }
}
