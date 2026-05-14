using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartInventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeRoleNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Role",
                table: "Users",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 8, 11, 14, 22, 381, DateTimeKind.Utc).AddTicks(6452), new DateTime(2025, 11, 8, 11, 14, 22, 381, DateTimeKind.Utc).AddTicks(6473), new DateTime(2026, 4, 8, 11, 14, 22, 381, DateTimeKind.Utc).AddTicks(6484), new DateTime(2026, 7, 8, 11, 14, 22, 381, DateTimeKind.Utc).AddTicks(6485) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "InstallDate" },
                values: new object[] { new DateTime(2026, 5, 8, 11, 14, 22, 381, DateTimeKind.Utc).AddTicks(6492), new DateTime(2026, 1, 8, 11, 14, 22, 381, DateTimeKind.Utc).AddTicks(6496) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 8, 11, 14, 22, 381, DateTimeKind.Utc).AddTicks(6501), new DateTime(2025, 9, 8, 11, 14, 22, 381, DateTimeKind.Utc).AddTicks(6503), new DateTime(2026, 3, 8, 11, 14, 22, 381, DateTimeKind.Utc).AddTicks(6504), new DateTime(2026, 6, 8, 11, 14, 22, 381, DateTimeKind.Utc).AddTicks(6505) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "InstallDate" },
                values: new object[] { new DateTime(2026, 5, 8, 11, 14, 22, 381, DateTimeKind.Utc).AddTicks(6510), new DateTime(2025, 5, 8, 11, 14, 22, 381, DateTimeKind.Utc).AddTicks(6512) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate" },
                values: new object[] { new DateTime(2026, 5, 8, 11, 14, 22, 381, DateTimeKind.Utc).AddTicks(6517), new DateTime(2026, 2, 8, 11, 14, 22, 381, DateTimeKind.Utc).AddTicks(6519), new DateTime(2026, 4, 24, 11, 14, 22, 381, DateTimeKind.Utc).AddTicks(6520) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "InstallDate" },
                values: new object[] { new DateTime(2026, 5, 8, 11, 14, 22, 381, DateTimeKind.Utc).AddTicks(6538), new DateTime(2025, 12, 8, 11, 14, 22, 381, DateTimeKind.Utc).AddTicks(6540) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 8, 11, 14, 22, 381, DateTimeKind.Utc).AddTicks(6545), new DateTime(2025, 7, 8, 11, 14, 22, 381, DateTimeKind.Utc).AddTicks(6547), new DateTime(2026, 4, 8, 11, 14, 22, 381, DateTimeKind.Utc).AddTicks(6548), new DateTime(2026, 8, 8, 11, 14, 22, 381, DateTimeKind.Utc).AddTicks(6549) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "InstallDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 8, 11, 14, 22, 381, DateTimeKind.Utc).AddTicks(6554), new DateTime(2026, 3, 8, 11, 14, 22, 381, DateTimeKind.Utc).AddTicks(6557), new DateTime(2026, 9, 8, 11, 14, 22, 381, DateTimeKind.Utc).AddTicks(6558) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 8, 11, 14, 22, 381, DateTimeKind.Utc).AddTicks(6564));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Role",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

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

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 8, 10, 13, 30, 958, DateTimeKind.Utc).AddTicks(329));
        }
    }
}
