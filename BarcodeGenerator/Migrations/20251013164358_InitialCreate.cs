using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BarcodeGenerator.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BarcodeRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BarcodeType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "CODE128"),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    LastPrintedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastExportedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    IsExported = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    BarcodeText = table.Column<string>(type: "TEXT", maxLength: 35, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    TotalPrintCount = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    DefaultLabelCount = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    LastLabelWidth = table.Column<double>(type: "REAL", nullable: true),
                    LastLabelHeight = table.Column<double>(type: "REAL", nullable: true),
                    LastBarcodeWidth = table.Column<double>(type: "REAL", nullable: true),
                    LastBarcodeHeight = table.Column<double>(type: "REAL", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Comment = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Filename = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BarcodeRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LabelTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    TemplateName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LabelWidth = table.Column<double>(type: "REAL", nullable: false),
                    LabelHeight = table.Column<double>(type: "REAL", nullable: false),
                    BarcodeWidth = table.Column<double>(type: "REAL", nullable: false),
                    BarcodeHeight = table.Column<double>(type: "REAL", nullable: false),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabelTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrintHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BarcodeRecordId = table.Column<int>(type: "INTEGER", nullable: false),
                    PrintedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    LabelWidth = table.Column<double>(type: "REAL", nullable: false),
                    LabelHeight = table.Column<double>(type: "REAL", nullable: false),
                    BarcodeWidth = table.Column<double>(type: "REAL", nullable: false),
                    BarcodeHeight = table.Column<double>(type: "REAL", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    QuantityPrinted = table.Column<int>(type: "INTEGER", nullable: false),
                    PrinterName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Success = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrintHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrintHistory_BarcodeRecords_BarcodeRecordId",
                        column: x => x.BarcodeRecordId,
                        principalTable: "BarcodeRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "LabelTemplates",
                columns: new[] { "Id", "BarcodeHeight", "BarcodeWidth", "CreatedDate", "IsDefault", "LabelHeight", "LabelWidth", "TemplateName" },
                values: new object[] { 1, 20.0, 80.0, new DateTime(2025, 10, 13, 21, 43, 56, 617, DateTimeKind.Local).AddTicks(3969), true, 50.0, 100.0, "Standard 100x50mm" });

            migrationBuilder.InsertData(
                table: "LabelTemplates",
                columns: new[] { "Id", "BarcodeHeight", "BarcodeWidth", "CreatedDate", "LabelHeight", "LabelWidth", "TemplateName" },
                values: new object[,]
                {
                    { 2, 15.0, 60.0, new DateTime(2025, 10, 13, 21, 43, 56, 619, DateTimeKind.Local).AddTicks(2733), 25.0, 75.0, "Small 75x25mm" },
                    { 3, 30.0, 120.0, new DateTime(2025, 10, 13, 21, 43, 56, 619, DateTimeKind.Local).AddTicks(2750), 75.0, 150.0, "Large 150x75mm" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BarcodeRecords_BarcodeText",
                table: "BarcodeRecords",
                column: "BarcodeText");

            migrationBuilder.CreateIndex(
                name: "IX_BarcodeRecords_CreatedDate",
                table: "BarcodeRecords",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_BarcodeRecords_IsActive",
                table: "BarcodeRecords",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_BarcodeRecords_LastPrintedDate",
                table: "BarcodeRecords",
                column: "LastPrintedDate");

            migrationBuilder.CreateIndex(
                name: "IX_LabelTemplates_IsDefault",
                table: "LabelTemplates",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "IX_LabelTemplates_TemplateName",
                table: "LabelTemplates",
                column: "TemplateName");

            migrationBuilder.CreateIndex(
                name: "IX_PrintHistory_BarcodeRecordId",
                table: "PrintHistory",
                column: "BarcodeRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_PrintHistory_PrintedDate",
                table: "PrintHistory",
                column: "PrintedDate");

            migrationBuilder.CreateIndex(
                name: "IX_PrintHistory_Success",
                table: "PrintHistory",
                column: "Success");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabelTemplates");

            migrationBuilder.DropTable(
                name: "PrintHistory");

            migrationBuilder.DropTable(
                name: "BarcodeRecords");
        }
    }
}
