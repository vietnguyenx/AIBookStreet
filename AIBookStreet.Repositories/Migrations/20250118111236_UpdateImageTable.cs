using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIBookStreet.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class UpdateImageTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_Authors_EntityId",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_BookStores_EntityId",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_Books_EntityId",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_Events_EntityId",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_Publishers_EntityId",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_Streets_EntityId",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_Users_EntityId",
                table: "Images");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {           
            migrationBuilder.AddForeignKey(
                name: "FK_Images_Authors_EntityId",
                table: "Images",
                column: "EntityId",
                principalTable: "Authors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_BookStores_EntityId",
                table: "Images",
                column: "EntityId",
                principalTable: "BookStores",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Books_EntityId",
                table: "Images",
                column: "EntityId",
                principalTable: "Books",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Events_EntityId",
                table: "Images",
                column: "EntityId",
                principalTable: "Events",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Publishers_EntityId",
                table: "Images",
                column: "EntityId",
                principalTable: "Publishers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Streets_EntityId",
                table: "Images",
                column: "EntityId",
                principalTable: "Streets",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Users_EntityId",
                table: "Images",
                column: "EntityId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
