using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BitPantry.Iota.Data.Entity.Migrations
{
    /// <inheritdoc />
    public partial class replacedCardOrderWithFractionalOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cards_UserId_Tab_Order",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Cards");

            migrationBuilder.AddColumn<double>(
                name: "FractionalOrder",
                table: "Cards",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateIndex(
                name: "IX_Cards_UserId_Tab_Order",
                table: "Cards",
                columns: new[] { "UserId", "Tab", "FractionalOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cards_UserId_Tab_Order",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "FractionalOrder",
                table: "Cards");

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Cards",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Cards_UserId_Tab_Order",
                table: "Cards",
                columns: new[] { "UserId", "Tab", "Order" });
        }
    }
}
