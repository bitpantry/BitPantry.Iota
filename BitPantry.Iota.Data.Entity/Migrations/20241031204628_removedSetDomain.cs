using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BitPantry.Iota.Data.Entity.Migrations
{
    /// <inheritdoc />
    public partial class removedSetDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CardVerse");

            migrationBuilder.RenameColumn(
                name: "Thumbprint",
                table: "Cards",
                newName: "Address");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastReviewedOn",
                table: "Cards",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "BibleId",
                table: "Cards",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "EndVerseId",
                table: "Cards",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "StartVerseId",
                table: "Cards",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Cards_BibleId",
                table: "Cards",
                column: "BibleId");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_EndVerseId",
                table: "Cards",
                column: "EndVerseId");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_StartVerseId",
                table: "Cards",
                column: "StartVerseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cards_Bibles_BibleId",
                table: "Cards",
                column: "BibleId",
                principalTable: "Bibles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cards_Verses_EndVerseId",
                table: "Cards",
                column: "EndVerseId",
                principalTable: "Verses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Cards_Verses_StartVerseId",
                table: "Cards",
                column: "StartVerseId",
                principalTable: "Verses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cards_Bibles_BibleId",
                table: "Cards");

            migrationBuilder.DropForeignKey(
                name: "FK_Cards_Verses_EndVerseId",
                table: "Cards");

            migrationBuilder.DropForeignKey(
                name: "FK_Cards_Verses_StartVerseId",
                table: "Cards");

            migrationBuilder.DropIndex(
                name: "IX_Cards_BibleId",
                table: "Cards");

            migrationBuilder.DropIndex(
                name: "IX_Cards_EndVerseId",
                table: "Cards");

            migrationBuilder.DropIndex(
                name: "IX_Cards_StartVerseId",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "BibleId",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "EndVerseId",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "StartVerseId",
                table: "Cards");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Cards",
                newName: "Thumbprint");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastReviewedOn",
                table: "Cards",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "CardVerse",
                columns: table => new
                {
                    CardId = table.Column<long>(type: "bigint", nullable: false),
                    VerseId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardVerse", x => new { x.CardId, x.VerseId });
                    table.ForeignKey(
                        name: "FK_CardVerse_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CardVerse_Verses_VerseId",
                        column: x => x.VerseId,
                        principalTable: "Verses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CardVerse_VerseId",
                table: "CardVerse",
                column: "VerseId");
        }
    }
}
