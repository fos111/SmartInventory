using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartInventory.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSyncQueue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SyncQueueEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: true),
                    OperationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: false),
                    TargetRoomCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    NewStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ClientOperationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PerformedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsProcessed = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncQueueEntries", x => x.Id);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_SyncQueueEntries_ClientOperationId",
                table: "SyncQueueEntries",
                column: "ClientOperationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SyncQueueEntries_DeviceId_IsProcessed",
                table: "SyncQueueEntries",
                columns: new[] { "DeviceId", "IsProcessed" });

            migrationBuilder.CreateIndex(
                name: "IX_SyncQueueEntries_ReceivedAt",
                table: "SyncQueueEntries",
                column: "ReceivedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SyncQueueEntries");

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 14, 21, 18, 6, 192, DateTimeKind.Utc).AddTicks(1782), new DateTime(2025, 11, 14, 21, 18, 6, 192, DateTimeKind.Utc).AddTicks(1801), new DateTime(2026, 4, 14, 21, 18, 6, 192, DateTimeKind.Utc).AddTicks(1816), new DateTime(2026, 7, 14, 21, 18, 6, 192, DateTimeKind.Utc).AddTicks(1817) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "InstallDate" },
                values: new object[] { new DateTime(2026, 5, 14, 21, 18, 6, 192, DateTimeKind.Utc).AddTicks(1829), new DateTime(2026, 1, 14, 21, 18, 6, 192, DateTimeKind.Utc).AddTicks(1833) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 14, 21, 18, 6, 192, DateTimeKind.Utc).AddTicks(1838), new DateTime(2025, 9, 14, 21, 18, 6, 192, DateTimeKind.Utc).AddTicks(1842), new DateTime(2026, 3, 14, 21, 18, 6, 192, DateTimeKind.Utc).AddTicks(1842), new DateTime(2026, 6, 14, 21, 18, 6, 192, DateTimeKind.Utc).AddTicks(1843) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "InstallDate" },
                values: new object[] { new DateTime(2026, 5, 14, 21, 18, 6, 192, DateTimeKind.Utc).AddTicks(1848), new DateTime(2025, 5, 14, 21, 18, 6, 192, DateTimeKind.Utc).AddTicks(1851) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate" },
                values: new object[] { new DateTime(2026, 5, 14, 21, 18, 6, 192, DateTimeKind.Utc).AddTicks(1856), new DateTime(2026, 2, 14, 21, 18, 6, 192, DateTimeKind.Utc).AddTicks(1857), new DateTime(2026, 4, 30, 21, 18, 6, 192, DateTimeKind.Utc).AddTicks(1858) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "InstallDate" },
                values: new object[] { new DateTime(2026, 5, 14, 21, 18, 6, 192, DateTimeKind.Utc).AddTicks(1880), new DateTime(2025, 12, 14, 21, 18, 6, 192, DateTimeKind.Utc).AddTicks(1882) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 14, 21, 18, 6, 192, DateTimeKind.Utc).AddTicks(1887), new DateTime(2025, 7, 14, 21, 18, 6, 192, DateTimeKind.Utc).AddTicks(1889), new DateTime(2026, 4, 14, 21, 18, 6, 192, DateTimeKind.Utc).AddTicks(1890), new DateTime(2026, 8, 14, 21, 18, 6, 192, DateTimeKind.Utc).AddTicks(1890) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "InstallDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 14, 21, 18, 6, 192, DateTimeKind.Utc).AddTicks(1895), new DateTime(2026, 3, 14, 21, 18, 6, 192, DateTimeKind.Utc).AddTicks(1897), new DateTime(2026, 9, 14, 21, 18, 6, 192, DateTimeKind.Utc).AddTicks(1898) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 14, 21, 18, 6, 192, DateTimeKind.Utc).AddTicks(1903));
        }
    }
}
