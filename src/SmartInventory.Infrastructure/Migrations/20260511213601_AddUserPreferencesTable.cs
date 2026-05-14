using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartInventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPreferencesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    EntityType = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<string>(type: "text", nullable: false),
                    EntityName = table.Column<string>(type: "text", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: true),
                    ChangedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_UserId_Key",
                table: "UserPreferences",
                columns: new[] { "UserId", "Key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLogs");

            migrationBuilder.DropTable(
                name: "UserPreferences");

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
    }
}
