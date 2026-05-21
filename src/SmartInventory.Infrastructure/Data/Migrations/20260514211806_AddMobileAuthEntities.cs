using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartInventory.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMobileAuthEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PasswordResetTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Otp = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
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

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_Email",
                table: "PasswordResetTokens",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_ExpiresAt",
                table: "PasswordResetTokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ExpiresAt",
                table: "RefreshTokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PasswordResetTokens");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5503), new DateTime(2025, 11, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5517), new DateTime(2026, 4, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5528), new DateTime(2026, 7, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5529) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "InstallDate" },
                values: new object[] { new DateTime(2026, 5, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5537), new DateTime(2026, 1, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5539) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5547), new DateTime(2025, 9, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5549), new DateTime(2026, 3, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5550), new DateTime(2026, 6, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5551) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "InstallDate" },
                values: new object[] { new DateTime(2026, 5, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5557), new DateTime(2025, 5, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5560) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate" },
                values: new object[] { new DateTime(2026, 5, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5567), new DateTime(2026, 2, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5569), new DateTime(2026, 4, 29, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5570) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "InstallDate" },
                values: new object[] { new DateTime(2026, 5, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5591), new DateTime(2025, 12, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5593) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5600), new DateTime(2025, 7, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5602), new DateTime(2026, 4, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5604), new DateTime(2026, 8, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5605) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "InstallDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5611), new DateTime(2026, 3, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5614), new DateTime(2026, 9, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5615) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 13, 22, 0, 55, 960, DateTimeKind.Utc).AddTicks(5622));
        }
    }
}
