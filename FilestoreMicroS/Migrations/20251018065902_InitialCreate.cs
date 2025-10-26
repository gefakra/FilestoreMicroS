using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilestoreMicroS.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    Hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CompressedData = table.Column<byte[]>(type: "BLOB", nullable: false),
                    OriginalSize = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Hash);
                });

            migrationBuilder.CreateTable(
                name: "Owners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Owners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FileOwners",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileHash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    OwnerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AddedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileOwners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileOwners_Files_FileHash",
                        column: x => x.FileHash,
                        principalTable: "Files",
                        principalColumn: "Hash",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FileOwners_Owners_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Owners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileOwners_FileHash",
                table: "FileOwners",
                column: "FileHash");

            migrationBuilder.CreateIndex(
                name: "IX_FileOwners_OwnerId",
                table: "FileOwners",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_Hash",
                table: "Files",
                column: "Hash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileOwners");

            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropTable(
                name: "Owners");
        }
    }
}
