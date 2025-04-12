using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIBookStreet.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class RenameCodeToISBNInBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Code",
                table: "Books",
                newName: "ISBN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ISBN",
                table: "Books",
                newName: "Code");
        }
    }
}
