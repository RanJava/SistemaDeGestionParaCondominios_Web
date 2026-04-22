using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CondoAdmin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Correcciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "SalePrice",
                table: "Sales",
                type: "decimal(12,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Sales",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<string>(
                name: "MethodOfPayment",
                table: "Sales",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Units_UnitId1",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_Sales_UnitId1",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "MethodOfPayment",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "UnitId1",
                table: "Sales");

            migrationBuilder.AlterColumn<decimal>(
                name: "SalePrice",
                table: "Sales",
                type: "decimal(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Sales",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
