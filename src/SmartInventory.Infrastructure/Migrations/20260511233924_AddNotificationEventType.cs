using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartInventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationEventType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EventType",
                table: "Notifications",
                type: "integer",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 11, 23, 39, 23, 890, DateTimeKind.Utc).AddTicks(9946), new DateTime(2025, 11, 11, 23, 39, 23, 890, DateTimeKind.Utc).AddTicks(9972), new DateTime(2026, 4, 11, 23, 39, 23, 890, DateTimeKind.Utc).AddTicks(9992), new DateTime(2026, 7, 11, 23, 39, 23, 890, DateTimeKind.Utc).AddTicks(9994) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "InstallDate" },
                values: new object[] { new DateTime(2026, 5, 11, 23, 39, 23, 891, DateTimeKind.Utc).AddTicks(6), new DateTime(2026, 1, 11, 23, 39, 23, 891, DateTimeKind.Utc).AddTicks(10) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 11, 23, 39, 23, 891, DateTimeKind.Utc).AddTicks(17), new DateTime(2025, 9, 11, 23, 39, 23, 891, DateTimeKind.Utc).AddTicks(22), new DateTime(2026, 3, 11, 23, 39, 23, 891, DateTimeKind.Utc).AddTicks(23), new DateTime(2026, 6, 11, 23, 39, 23, 891, DateTimeKind.Utc).AddTicks(24) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "InstallDate" },
                values: new object[] { new DateTime(2026, 5, 11, 23, 39, 23, 891, DateTimeKind.Utc).AddTicks(31), new DateTime(2025, 5, 11, 23, 39, 23, 891, DateTimeKind.Utc).AddTicks(34) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate" },
                values: new object[] { new DateTime(2026, 5, 11, 23, 39, 23, 891, DateTimeKind.Utc).AddTicks(41), new DateTime(2026, 2, 11, 23, 39, 23, 891, DateTimeKind.Utc).AddTicks(47), new DateTime(2026, 4, 27, 23, 39, 23, 891, DateTimeKind.Utc).AddTicks(47) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "InstallDate" },
                values: new object[] { new DateTime(2026, 5, 11, 23, 39, 23, 891, DateTimeKind.Utc).AddTicks(93), new DateTime(2025, 12, 11, 23, 39, 23, 891, DateTimeKind.Utc).AddTicks(97) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 11, 23, 39, 23, 891, DateTimeKind.Utc).AddTicks(105), new DateTime(2025, 7, 11, 23, 39, 23, 891, DateTimeKind.Utc).AddTicks(108), new DateTime(2026, 4, 11, 23, 39, 23, 891, DateTimeKind.Utc).AddTicks(109), new DateTime(2026, 8, 11, 23, 39, 23, 891, DateTimeKind.Utc).AddTicks(110) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "InstallDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 11, 23, 39, 23, 891, DateTimeKind.Utc).AddTicks(116), new DateTime(2026, 3, 11, 23, 39, 23, 891, DateTimeKind.Utc).AddTicks(122), new DateTime(2026, 9, 11, 23, 39, 23, 891, DateTimeKind.Utc).AddTicks(123) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 11, 23, 39, 23, 891, DateTimeKind.Utc).AddTicks(130));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventType",
                table: "Notifications");

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 11, 21, 36, 1, 138, DateTimeKind.Utc).AddTicks(3418), new DateTime(2025, 11, 11, 21, 36, 1, 138, DateTimeKind.Utc).AddTicks(3437), new DateTime(2026, 4, 11, 21, 36, 1, 138, DateTimeKind.Utc).AddTicks(3448), new DateTime(2026, 7, 11, 21, 36, 1, 138, DateTimeKind.Utc).AddTicks(3450) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "InstallDate" },
                values: new object[] { new DateTime(2026, 5, 11, 21, 36, 1, 138, DateTimeKind.Utc).AddTicks(3460), new DateTime(2026, 1, 11, 21, 36, 1, 138, DateTimeKind.Utc).AddTicks(3465) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 11, 21, 36, 1, 138, DateTimeKind.Utc).AddTicks(3473), new DateTime(2025, 9, 11, 21, 36, 1, 138, DateTimeKind.Utc).AddTicks(3476), new DateTime(2026, 3, 11, 21, 36, 1, 138, DateTimeKind.Utc).AddTicks(3483), new DateTime(2026, 6, 11, 21, 36, 1, 138, DateTimeKind.Utc).AddTicks(3484) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "InstallDate" },
                values: new object[] { new DateTime(2026, 5, 11, 21, 36, 1, 138, DateTimeKind.Utc).AddTicks(3490), new DateTime(2025, 5, 11, 21, 36, 1, 138, DateTimeKind.Utc).AddTicks(3493) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate" },
                values: new object[] { new DateTime(2026, 5, 11, 21, 36, 1, 138, DateTimeKind.Utc).AddTicks(3500), new DateTime(2026, 2, 11, 21, 36, 1, 138, DateTimeKind.Utc).AddTicks(3503), new DateTime(2026, 4, 27, 21, 36, 1, 138, DateTimeKind.Utc).AddTicks(3504) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "InstallDate" },
                values: new object[] { new DateTime(2026, 5, 11, 21, 36, 1, 138, DateTimeKind.Utc).AddTicks(3532), new DateTime(2025, 12, 11, 21, 36, 1, 138, DateTimeKind.Utc).AddTicks(3534) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 11, 21, 36, 1, 138, DateTimeKind.Utc).AddTicks(3540), new DateTime(2025, 7, 11, 21, 36, 1, 138, DateTimeKind.Utc).AddTicks(3543), new DateTime(2026, 4, 11, 21, 36, 1, 138, DateTimeKind.Utc).AddTicks(3544), new DateTime(2026, 8, 11, 21, 36, 1, 138, DateTimeKind.Utc).AddTicks(3545) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "InstallDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 11, 21, 36, 1, 138, DateTimeKind.Utc).AddTicks(3551), new DateTime(2026, 3, 11, 21, 36, 1, 138, DateTimeKind.Utc).AddTicks(3554), new DateTime(2026, 9, 11, 21, 36, 1, 138, DateTimeKind.Utc).AddTicks(3554) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 11, 21, 36, 1, 138, DateTimeKind.Utc).AddTicks(3567));
        }
    }
}
