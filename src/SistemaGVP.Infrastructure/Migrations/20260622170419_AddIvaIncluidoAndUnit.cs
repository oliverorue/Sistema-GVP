using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaGVP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIvaIncluidoAndUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "SaleDetails",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "pz");

            migrationBuilder.AddColumn<bool>(
                name: "IvaIncluido",
                table: "Companies",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_CompanyId_IsActive_Username",
                table: "Users",
                columns: new[] { "CompanyId", "IsActive", "Username" });

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_CompanyId_IsActive_Name",
                table: "Suppliers",
                columns: new[] { "CompanyId", "IsActive", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CompanyId_IsActive_Name",
                table: "Products",
                columns: new[] { "CompanyId", "IsActive", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CompanyId_IsActive_Name",
                table: "Customers",
                columns: new[] { "CompanyId", "IsActive", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CompanyId_IsActive_Name",
                table: "Categories",
                columns: new[] { "CompanyId", "IsActive", "Name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_CompanyId_IsActive_Username",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_CompanyId_IsActive_Name",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Products_CompanyId_IsActive_Name",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Customers_CompanyId_IsActive_Name",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Categories_CompanyId_IsActive_Name",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "SaleDetails");

            migrationBuilder.DropColumn(
                name: "IvaIncluido",
                table: "Companies");
        }
    }
}
