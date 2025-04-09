using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIBookStreet.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class event_ads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowAds",
                table: "Events",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowAds",
                table: "Events");
        }
    }
}
