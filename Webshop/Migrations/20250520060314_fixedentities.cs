using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Webshop.Migrations
{
    /// <inheritdoc />
    public partial class fixedentities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "zipCode",
                table: "Orders",
                newName: "ZipCode");

            migrationBuilder.RenameColumn(
                name: "invoicezipCode",
                table: "Orders",
                newName: "InvoiceZipCode");

            migrationBuilder.RenameColumn(
                name: "invoiceCity",
                table: "Orders",
                newName: "InvoiceCity");

            migrationBuilder.RenameColumn(
                name: "invoiceAddress",
                table: "Orders",
                newName: "InvoiceAddress");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "Customers");

            migrationBuilder.RenameColumn(
                name: "ZipCode",
                table: "Orders",
                newName: "zipCode");

            migrationBuilder.RenameColumn(
                name: "InvoiceZipCode",
                table: "Orders",
                newName: "invoicezipCode");

            migrationBuilder.RenameColumn(
                name: "InvoiceCity",
                table: "Orders",
                newName: "invoiceCity");

            migrationBuilder.RenameColumn(
                name: "InvoiceAddress",
                table: "Orders",
                newName: "invoiceAddress");
        }
    }
}
