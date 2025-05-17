using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIBookStreet.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddContractFileToUserStoreTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContractFileUrl",
                table: "UserStores",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractFileUrl",
                table: "UserStores");
        }
    }
}
