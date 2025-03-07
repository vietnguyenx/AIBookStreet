using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIBookStreet.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDB2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_BookStores_EntityId",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_BookStores_BookStoreId",
                table: "Inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Books_BookId",
                table: "Inventories");

            migrationBuilder.DropTable(
                name: "BookStores");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Inventories",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "BookId",
                table: "Inventories");

            migrationBuilder.RenameColumn(
                name: "BookStoreId",
                table: "Inventories",
                newName: "StoreId");

            migrationBuilder.RenameIndex(
                name: "IX_Inventories_BookStoreId",
                table: "Inventories",
                newName: "IX_Inventories_StoreId");

            migrationBuilder.RenameColumn(
                name: "isOpen",
                table: "Events",
                newName: "IsOpen");

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "Zones",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "Zones",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BaseImgUrl",
                table: "Users",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BaseImgUrl",
                table: "Publishers",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EntityId",
                table: "Inventories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BaseImgUrl",
                table: "Books",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Inventories",
                table: "Inventories",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Souvenirs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SouvenirName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    BaseImgUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Souvenirs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Stores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OpeningTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosingTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BaseImgUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ZoneId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stores_Users_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Stores_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_EntityId",
                table: "Inventories",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_ManagerId",
                table: "Stores",
                column: "ManagerId",
                unique: true,
                filter: "[ManagerId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_ZoneId",
                table: "Stores",
                column: "ZoneId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Images_Souvenirs_EntityId",
            //    table: "Images",
            //    column: "EntityId",
            //    principalTable: "Souvenirs",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Images_Stores_EntityId",
            //    table: "Images",
            //    column: "EntityId",
            //    principalTable: "Stores",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Inventories_Books_EntityId",
            //    table: "Inventories",
            //    column: "EntityId",
            //    principalTable: "Books",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Inventories_Souvenirs_EntityId",
            //    table: "Inventories",
            //    column: "EntityId",
            //    principalTable: "Souvenirs",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Stores_StoreId",
                table: "Inventories",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_Souvenirs_EntityId",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_Stores_EntityId",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Books_EntityId",
                table: "Inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Souvenirs_EntityId",
                table: "Inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Stores_StoreId",
                table: "Inventories");

            migrationBuilder.DropTable(
                name: "Souvenirs");

            migrationBuilder.DropTable(
                name: "Stores");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Inventories",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_EntityId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Zones");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Zones");

            migrationBuilder.DropColumn(
                name: "BaseImgUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BaseImgUrl",
                table: "Publishers");

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "BaseImgUrl",
                table: "Books");

            migrationBuilder.RenameColumn(
                name: "StoreId",
                table: "Inventories",
                newName: "BookStoreId");

            migrationBuilder.RenameIndex(
                name: "IX_Inventories_StoreId",
                table: "Inventories",
                newName: "IX_Inventories_BookStoreId");

            migrationBuilder.RenameColumn(
                name: "IsOpen",
                table: "Events",
                newName: "isOpen");

            migrationBuilder.AddColumn<Guid>(
                name: "BookId",
                table: "Inventories",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Inventories",
                table: "Inventories",
                columns: new[] { "BookId", "BookStoreId" });

            migrationBuilder.CreateTable(
                name: "BookStores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ZoneId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BookStoreName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ClosingTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OpeningTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookStores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookStores_Users_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BookStores_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookStores_ManagerId",
                table: "BookStores",
                column: "ManagerId",
                unique: true,
                filter: "[ManagerId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BookStores_ZoneId",
                table: "BookStores",
                column: "ZoneId");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_BookStores_EntityId",
                table: "Images",
                column: "EntityId",
                principalTable: "BookStores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_BookStores_BookStoreId",
                table: "Inventories",
                column: "BookStoreId",
                principalTable: "BookStores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Books_BookId",
                table: "Inventories",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
