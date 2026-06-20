using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaGVP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceCounter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InvoiceCounters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CompanyId = table.Column<int>(type: "INTEGER", nullable: false),
                    DatePrefix = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    LastNumber = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceCounters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceCounters_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceCounters_CompanyId_DatePrefix",
                table: "InvoiceCounters",
                columns: new[] { "CompanyId", "DatePrefix" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceCounters");
        }
    }
}
