using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartInventory.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetPhotoPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoPath",
                table: "Assets",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate", "MaintenanceDueDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 5, 18, 20, 41, 44, 502, DateTimeKind.Utc).AddTicks(5430), new DateTime(2025, 11, 18, 20, 41, 44, 502, DateTimeKind.Utc).AddTicks(5449), new DateTime(2026, 4, 18, 20, 41, 44, 502, DateTimeKind.Utc).AddTicks(5461), new DateTime(2026, 7, 18, 20, 41, 44, 502, DateTimeKind.Utc).AddTicks(5462), null });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "InstallDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 5, 18, 20, 41, 44, 502, DateTimeKind.Utc).AddTicks(5469), new DateTime(2026, 1, 18, 20, 41, 44, 502, DateTimeKind.Utc).AddTicks(5474), null });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate", "MaintenanceDueDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 5, 18, 20, 41, 44, 502, DateTimeKind.Utc).AddTicks(5480), new DateTime(2025, 9, 18, 20, 41, 44, 502, DateTimeKind.Utc).AddTicks(5485), new DateTime(2026, 3, 18, 20, 41, 44, 502, DateTimeKind.Utc).AddTicks(5486), new DateTime(2026, 6, 18, 20, 41, 44, 502, DateTimeKind.Utc).AddTicks(5487), null });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "InstallDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 5, 18, 20, 41, 44, 502, DateTimeKind.Utc).AddTicks(5493), new DateTime(2025, 5, 18, 20, 41, 44, 502, DateTimeKind.Utc).AddTicks(5495), null });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 5, 18, 20, 41, 44, 502, DateTimeKind.Utc).AddTicks(5529), new DateTime(2026, 2, 18, 20, 41, 44, 502, DateTimeKind.Utc).AddTicks(5532), new DateTime(2026, 5, 4, 20, 41, 44, 502, DateTimeKind.Utc).AddTicks(5533), null });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "InstallDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 5, 18, 20, 41, 44, 502, DateTimeKind.Utc).AddTicks(5551), new DateTime(2025, 12, 18, 20, 41, 44, 502, DateTimeKind.Utc).AddTicks(5554), null });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate", "MaintenanceDueDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 5, 18, 20, 41, 44, 502, DateTimeKind.Utc).AddTicks(5560), new DateTime(2025, 7, 18, 20, 41, 44, 502, DateTimeKind.Utc).AddTicks(5562), new DateTime(2026, 4, 18, 20, 41, 44, 502, DateTimeKind.Utc).AddTicks(5563), new DateTime(2026, 8, 18, 20, 41, 44, 502, DateTimeKind.Utc).AddTicks(5564), null });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "InstallDate", "MaintenanceDueDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 5, 18, 20, 41, 44, 502, DateTimeKind.Utc).AddTicks(5570), new DateTime(2026, 3, 18, 20, 41, 44, 502, DateTimeKind.Utc).AddTicks(5572), new DateTime(2026, 9, 18, 20, 41, 44, 502, DateTimeKind.Utc).AddTicks(5573), null });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "PhotoPath" },
                values: new object[] { new DateTime(2026, 5, 18, 20, 41, 44, 502, DateTimeKind.Utc).AddTicks(5579), null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoPath",
                table: "Assets");

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 15, 15, 5, 46, 783, DateTimeKind.Utc).AddTicks(5888), new DateTime(2025, 11, 15, 15, 5, 46, 783, DateTimeKind.Utc).AddTicks(5908), new DateTime(2026, 4, 15, 15, 5, 46, 783, DateTimeKind.Utc).AddTicks(5918), new DateTime(2026, 7, 15, 15, 5, 46, 783, DateTimeKind.Utc).AddTicks(5919) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "InstallDate" },
                values: new object[] { new DateTime(2026, 5, 15, 15, 5, 46, 783, DateTimeKind.Utc).AddTicks(5926), new DateTime(2026, 1, 15, 15, 5, 46, 783, DateTimeKind.Utc).AddTicks(5928) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 15, 15, 5, 46, 783, DateTimeKind.Utc).AddTicks(5934), new DateTime(2025, 9, 15, 15, 5, 46, 783, DateTimeKind.Utc).AddTicks(5936), new DateTime(2026, 3, 15, 15, 5, 46, 783, DateTimeKind.Utc).AddTicks(5937), new DateTime(2026, 6, 15, 15, 5, 46, 783, DateTimeKind.Utc).AddTicks(5938) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "InstallDate" },
                values: new object[] { new DateTime(2026, 5, 15, 15, 5, 46, 783, DateTimeKind.Utc).AddTicks(5943), new DateTime(2025, 5, 15, 15, 5, 46, 783, DateTimeKind.Utc).AddTicks(5945) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate" },
                values: new object[] { new DateTime(2026, 5, 15, 15, 5, 46, 783, DateTimeKind.Utc).AddTicks(5950), new DateTime(2026, 2, 15, 15, 5, 46, 783, DateTimeKind.Utc).AddTicks(5952), new DateTime(2026, 5, 1, 15, 5, 46, 783, DateTimeKind.Utc).AddTicks(5952) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "InstallDate" },
                values: new object[] { new DateTime(2026, 5, 15, 15, 5, 46, 783, DateTimeKind.Utc).AddTicks(5972), new DateTime(2025, 12, 15, 15, 5, 46, 783, DateTimeKind.Utc).AddTicks(5974) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 15, 15, 5, 46, 783, DateTimeKind.Utc).AddTicks(5979), new DateTime(2025, 7, 15, 15, 5, 46, 783, DateTimeKind.Utc).AddTicks(5981), new DateTime(2026, 4, 15, 15, 5, 46, 783, DateTimeKind.Utc).AddTicks(5982), new DateTime(2026, 8, 15, 15, 5, 46, 783, DateTimeKind.Utc).AddTicks(5982) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "InstallDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 15, 15, 5, 46, 783, DateTimeKind.Utc).AddTicks(5988), new DateTime(2026, 3, 15, 15, 5, 46, 783, DateTimeKind.Utc).AddTicks(5990), new DateTime(2026, 9, 15, 15, 5, 46, 783, DateTimeKind.Utc).AddTicks(5990) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 15, 15, 5, 46, 783, DateTimeKind.Utc).AddTicks(5996));
        }
    }
}
