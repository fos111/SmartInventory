using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartInventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetLocationHistoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastDetectedUpdate",
                table: "Assets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AssetLocationHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousRoomCode = table.Column<string>(type: "text", nullable: true),
                    NewRoomCode = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetLocationHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoomGeometries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShapeType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    X = table.Column<double>(type: "double precision", nullable: false),
                    Y = table.Column<double>(type: "double precision", nullable: false),
                    Width = table.Column<double>(type: "double precision", nullable: false),
                    Height = table.Column<double>(type: "double precision", nullable: false),
                    Color = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    Stroke = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomGeometries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoomGeometries_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "InstallDate", "LastDetectedUpdate", "LastServiceDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5503), new DateTime(2025, 11, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5517), null, new DateTime(2026, 4, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5528), new DateTime(2026, 7, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5529) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "InstallDate", "LastDetectedUpdate" },
                values: new object[] { new DateTime(2026, 5, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5537), new DateTime(2026, 1, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5539), null });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "InstallDate", "LastDetectedUpdate", "LastServiceDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5547), new DateTime(2025, 9, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5549), null, new DateTime(2026, 3, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5550), new DateTime(2026, 6, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5551) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "InstallDate", "LastDetectedUpdate" },
                values: new object[] { new DateTime(2026, 5, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5557), new DateTime(2025, 5, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5560), null });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "InstallDate", "LastDetectedUpdate", "LastServiceDate" },
                values: new object[] { new DateTime(2026, 5, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5567), new DateTime(2026, 2, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5569), null, new DateTime(2026, 4, 29, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5570) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "InstallDate", "LastDetectedUpdate" },
                values: new object[] { new DateTime(2026, 5, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5591), new DateTime(2025, 12, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5593), null });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "InstallDate", "LastDetectedUpdate", "LastServiceDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5600), new DateTime(2025, 7, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5602), null, new DateTime(2026, 4, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5604), new DateTime(2026, 8, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5605) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "InstallDate", "LastDetectedUpdate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5611), new DateTime(2026, 3, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5614), null, new DateTime(2026, 9, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5615) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "LastDetectedUpdate" },
                values: new object[] { new DateTime(2026, 5, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5622), null });

            migrationBuilder.CreateIndex(
                name: "IX_RoomGeometries_RoomId",
                table: "RoomGeometries",
                column: "RoomId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetLocationHistories");

            migrationBuilder.DropTable(
                name: "RoomGeometries");

            migrationBuilder.DropColumn(
                name: "LastDetectedUpdate",
                table: "Assets");

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
    }
}
