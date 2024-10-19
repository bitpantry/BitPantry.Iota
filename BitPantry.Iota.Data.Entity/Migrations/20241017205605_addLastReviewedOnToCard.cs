using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BitPantry.Iota.Data.Entity.Migrations
{
    /// <inheritdoc />
    public partial class addLastReviewedOnToCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastReviewedOn",
                table: "Cards",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastReviewedOn",
                table: "Cards");
        }
    }
}
