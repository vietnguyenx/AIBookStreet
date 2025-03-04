using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIBookStreet.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Streets_StreetId",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "StreetId",
                table: "Events",
                newName: "ZoneId");

            migrationBuilder.RenameIndex(
                name: "IX_Events_StreetId",
                table: "Events",
                newName: "IX_Events_ZoneId");

            migrationBuilder.AddColumn<string>(
                name: "BaseImgUrl",
                table: "Streets",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "Streets",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "Streets",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BaseImgUrl",
                table: "Events",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VideoLink",
                table: "Events",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isOpen",
                table: "Events",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "BaseImgUrl",
                table: "Authors",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Zones_ZoneId",
                table: "Events",
                column: "ZoneId",
                principalTable: "Zones",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Zones_ZoneId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "BaseImgUrl",
                table: "Streets");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Streets");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Streets");

            migrationBuilder.DropColumn(
                name: "BaseImgUrl",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "VideoLink",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "isOpen",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "BaseImgUrl",
                table: "Authors");

            migrationBuilder.RenameColumn(
                name: "ZoneId",
                table: "Events",
                newName: "StreetId");

            migrationBuilder.RenameIndex(
                name: "IX_Events_ZoneId",
                table: "Events",
                newName: "IX_Events_StreetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Streets_StreetId",
                table: "Events",
                column: "StreetId",
                principalTable: "Streets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
