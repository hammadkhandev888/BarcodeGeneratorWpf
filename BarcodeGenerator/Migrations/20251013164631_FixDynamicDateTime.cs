using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarcodeGenerator.Migrations
{
    /// <inheritdoc />
    public partial class FixDynamicDateTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "LabelTemplates",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "LabelTemplates",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "LabelTemplates",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "LabelTemplates",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 10, 13, 21, 43, 56, 617, DateTimeKind.Local).AddTicks(3969));

            migrationBuilder.UpdateData(
                table: "LabelTemplates",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 10, 13, 21, 43, 56, 619, DateTimeKind.Local).AddTicks(2733));

            migrationBuilder.UpdateData(
                table: "LabelTemplates",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2025, 10, 13, 21, 43, 56, 619, DateTimeKind.Local).AddTicks(2750));
        }
    }
}
