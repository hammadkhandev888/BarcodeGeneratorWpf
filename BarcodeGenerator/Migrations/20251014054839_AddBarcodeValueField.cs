using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarcodeGenerator.Migrations
{
    /// <inheritdoc />
    public partial class AddBarcodeValueField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BarcodeValue",
                table: "BarcodeRecords",
                type: "TEXT",
                maxLength: 35,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BarcodeValue",
                table: "BarcodeRecords");
        }
    }
}
