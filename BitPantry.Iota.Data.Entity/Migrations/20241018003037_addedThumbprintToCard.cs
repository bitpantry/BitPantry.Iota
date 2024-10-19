using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BitPantry.Iota.Data.Entity.Migrations
{
    /// <inheritdoc />
    public partial class addedThumbprintToCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Thumbprint",
                table: "Cards",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Thumbprint",
                table: "Cards");
        }
    }
}
