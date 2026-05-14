using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartInventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInStockStatusAndGestionnaireRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 8, 10, 13, 30, 958, DateTimeKind.Utc).AddTicks(107), new DateTime(2025, 11, 8, 10, 13, 30, 958, DateTimeKind.Utc).AddTicks(127), new DateTime(2026, 4, 8, 10, 13, 30, 958, DateTimeKind.Utc).AddTicks(137), new DateTime(2026, 7, 8, 10, 13, 30, 958, DateTimeKind.Utc).AddTicks(138) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "InstallDate" },
                values: new object[] { new DateTime(2026, 5, 8, 10, 13, 30, 958, DateTimeKind.Utc).AddTicks(144), new DateTime(2026, 1, 8, 10, 13, 30, 958, DateTimeKind.Utc).AddTicks(147) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 8, 10, 13, 30, 958, DateTimeKind.Utc).AddTicks(268), new DateTime(2025, 9, 8, 10, 13, 30, 958, DateTimeKind.Utc).AddTicks(271), new DateTime(2026, 3, 8, 10, 13, 30, 958, DateTimeKind.Utc).AddTicks(272), new DateTime(2026, 6, 8, 10, 13, 30, 958, DateTimeKind.Utc).AddTicks(273) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "InstallDate" },
                values: new object[] { new DateTime(2026, 5, 8, 10, 13, 30, 958, DateTimeKind.Utc).AddTicks(278), new DateTime(2025, 5, 8, 10, 13, 30, 958, DateTimeKind.Utc).AddTicks(279) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate" },
                values: new object[] { new DateTime(2026, 5, 8, 10, 13, 30, 958, DateTimeKind.Utc).AddTicks(284), new DateTime(2026, 2, 8, 10, 13, 30, 958, DateTimeKind.Utc).AddTicks(287), new DateTime(2026, 4, 24, 10, 13, 30, 958, DateTimeKind.Utc).AddTicks(288) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "InstallDate" },
                values: new object[] { new DateTime(2026, 5, 8, 10, 13, 30, 958, DateTimeKind.Utc).AddTicks(306), new DateTime(2025, 12, 8, 10, 13, 30, 958, DateTimeKind.Utc).AddTicks(308) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 8, 10, 13, 30, 958, DateTimeKind.Utc).AddTicks(313), new DateTime(2025, 7, 8, 10, 13, 30, 958, DateTimeKind.Utc).AddTicks(315), new DateTime(2026, 4, 8, 10, 13, 30, 958, DateTimeKind.Utc).AddTicks(316), new DateTime(2026, 8, 8, 10, 13, 30, 958, DateTimeKind.Utc).AddTicks(317) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "InstallDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 8, 10, 13, 30, 958, DateTimeKind.Utc).AddTicks(321), new DateTime(2026, 3, 8, 10, 13, 30, 958, DateTimeKind.Utc).AddTicks(323), new DateTime(2026, 9, 8, 10, 13, 30, 958, DateTimeKind.Utc).AddTicks(324) });

            migrationBuilder.InsertData(
                table: "Assets",
                columns: new[] { "Id", "AssetTag", "Category", "CreatedAt", "CurrentRoomCode", "DeletedAt", "Description", "DetectedRoomCode", "InstallDate", "LastMaintenanceDate", "LastSeen", "LastServiceDate", "MaintenanceDueDate", "Manufacturer", "Model", "Name", "RfidTagId", "SerialNumber", "Status", "UpdatedAt" },
                values: new object[] { new Guid("99999999-9999-9999-9999-999999999999"), "AST-INV-001", "Display", new DateTime(2026, 5, 8, 10, 13, 30, 958, DateTimeKind.Utc).AddTicks(329), "STOCK", null, "Nouveau moniteur en stock, pas encore déployé", null, null, null, null, null, null, "Dell", "E2222H", "Dell Monitor 22inch", null, "DL-MON-INV-001", 5, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"));

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 4, 22, 12, 51, 0, 812, DateTimeKind.Utc).AddTicks(8883), new DateTime(2025, 10, 22, 12, 51, 0, 812, DateTimeKind.Utc).AddTicks(8909), new DateTime(2026, 3, 22, 12, 51, 0, 812, DateTimeKind.Utc).AddTicks(8920), new DateTime(2026, 6, 22, 12, 51, 0, 812, DateTimeKind.Utc).AddTicks(8922) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "InstallDate" },
                values: new object[] { new DateTime(2026, 4, 22, 12, 51, 0, 812, DateTimeKind.Utc).AddTicks(8934), new DateTime(2025, 12, 22, 12, 51, 0, 812, DateTimeKind.Utc).AddTicks(8943) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 4, 22, 12, 51, 0, 812, DateTimeKind.Utc).AddTicks(8954), new DateTime(2025, 8, 22, 12, 51, 0, 812, DateTimeKind.Utc).AddTicks(8960), new DateTime(2026, 2, 22, 12, 51, 0, 812, DateTimeKind.Utc).AddTicks(8961), new DateTime(2026, 5, 22, 12, 51, 0, 812, DateTimeKind.Utc).AddTicks(8962) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "InstallDate" },
                values: new object[] { new DateTime(2026, 4, 22, 12, 51, 0, 812, DateTimeKind.Utc).AddTicks(8974), new DateTime(2025, 4, 22, 12, 51, 0, 812, DateTimeKind.Utc).AddTicks(8978) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate" },
                values: new object[] { new DateTime(2026, 4, 22, 12, 51, 0, 812, DateTimeKind.Utc).AddTicks(8989), new DateTime(2026, 1, 22, 12, 51, 0, 812, DateTimeKind.Utc).AddTicks(8994), new DateTime(2026, 4, 8, 12, 51, 0, 812, DateTimeKind.Utc).AddTicks(8996) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "InstallDate" },
                values: new object[] { new DateTime(2026, 4, 22, 12, 51, 0, 812, DateTimeKind.Utc).AddTicks(9030), new DateTime(2025, 11, 22, 12, 51, 0, 812, DateTimeKind.Utc).AddTicks(9037) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 4, 22, 12, 51, 0, 812, DateTimeKind.Utc).AddTicks(9046), new DateTime(2025, 6, 22, 12, 51, 0, 812, DateTimeKind.Utc).AddTicks(9049), new DateTime(2026, 3, 22, 12, 51, 0, 812, DateTimeKind.Utc).AddTicks(9051), new DateTime(2026, 7, 22, 12, 51, 0, 812, DateTimeKind.Utc).AddTicks(9052) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "InstallDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 4, 22, 12, 51, 0, 812, DateTimeKind.Utc).AddTicks(9061), new DateTime(2026, 2, 22, 12, 51, 0, 812, DateTimeKind.Utc).AddTicks(9064), new DateTime(2026, 8, 22, 12, 51, 0, 812, DateTimeKind.Utc).AddTicks(9066) });
        }
    }
}
