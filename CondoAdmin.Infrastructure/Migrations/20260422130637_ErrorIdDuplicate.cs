using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CondoAdmin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ErrorIdDuplicate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Units_UnitId1",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_Sales_UnitId1",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "UnitId1",
                table: "Sales");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UnitId1",
                table: "Sales",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sales_UnitId1",
                table: "Sales",
                column: "UnitId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Units_UnitId1",
                table: "Sales",
                column: "UnitId1",
                principalTable: "Units",
                principalColumn: "Id");
        }
    }
}
