using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Telegram.Bot.YouTuber.Webhook.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class NumField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Num",
                table: "Media",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Num",
                table: "Media");
        }
    }
}
