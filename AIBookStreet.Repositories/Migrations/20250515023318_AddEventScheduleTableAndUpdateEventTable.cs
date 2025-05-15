using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIBookStreet.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddEventScheduleTableAndUpdateEventTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Events");

            migrationBuilder.AddColumn<bool>(
                name: "IsApprove",
                table: "Events",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "Events",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdateForEventId",
                table: "Events",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Events",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "EventSchedule",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventDate = table.Column<DateOnly>(type: "date", maxLength: 20, nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", maxLength: 10, nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", maxLength: 10, nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventSchedule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventSchedule_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventSchedule_EventId",
                table: "EventSchedule",
                column: "EventId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventSchedule");

            migrationBuilder.DropColumn(
                name: "IsApprove",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "UpdateForEventId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Events");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Events",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Events",
                type: "datetime2",
                nullable: true);
        }
    }
}
