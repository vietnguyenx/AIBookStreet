using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIBookStreet.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class Add_EventRegistration_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventRegistrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrantName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RegistrantEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RegistrantPhoneNumber = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RegistrantAgeRange = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RegistrantGender = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RegistrantAddress = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ReferenceSource = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventRegistrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventRegistrations_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventRegistrations_EventId",
                table: "EventRegistrations",
                column: "EventId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventRegistrations");
        }
    }
}
