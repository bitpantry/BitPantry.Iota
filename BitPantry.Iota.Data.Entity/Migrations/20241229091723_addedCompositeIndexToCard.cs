using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BitPantry.Iota.Data.Entity.Migrations
{
    /// <inheritdoc />
    public partial class addedCompositeIndexToCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cards_UserId",
                table: "Cards");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_UserId_Tab_Order",
                table: "Cards",
                columns: new[] { "UserId", "Tab", "Order" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cards_UserId_Tab_Order",
                table: "Cards");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_UserId",
                table: "Cards",
                column: "UserId");
        }
    }
}
