using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BitPantry.Iota.Data.Entity.Migrations
{
    /// <inheritdoc />
    public partial class addedReviewCountToCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReviewCount",
                table: "Cards",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReviewCount",
                table: "Cards");
        }
    }
}
