using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BitPantry.Tabs.Data.Entity.Migrations
{
    /// <inheritdoc />
    public partial class createNumberedCardView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE VIEW NumberedCardView AS
                SELECT 
                    Id AS CardId,
                    ROW_NUMBER() OVER (PARTITION BY UserId, Tab ORDER BY UserId, Tab, FractionalOrder) AS RowNumber
                FROM Cards;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS NumberedCardView");
        }
    }
}
