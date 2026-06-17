using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaintShop.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToProductUserAndStockMovementNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "StockMovements",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Products");
        }
    }
}
