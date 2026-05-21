using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartInventory.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceBleIdFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(6621), new DateTime(2025, 11, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(6628), new DateTime(2026, 4, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(6640), new DateTime(2026, 7, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(6641) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "InstallDate" },
                values: new object[] { new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(6647), new DateTime(2026, 1, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(6652) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(6657), new DateTime(2025, 9, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(6659), new DateTime(2026, 3, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(6660), new DateTime(2026, 6, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(6661) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "InstallDate" },
                values: new object[] { new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(6666), new DateTime(2025, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(6667) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate" },
                values: new object[] { new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(6672), new DateTime(2026, 2, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(6674), new DateTime(2026, 5, 5, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(6674) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "InstallDate" },
                values: new object[] { new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(6693), new DateTime(2025, 12, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(6695) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(6700), new DateTime(2025, 7, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(6702), new DateTime(2026, 4, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(6702), new DateTime(2026, 8, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(6703) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "InstallDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(6708), new DateTime(2026, 3, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(6710), new DateTime(2026, 9, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(6711) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(6716));

            migrationBuilder.UpdateData(
                table: "Buildings",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(4417));

            migrationBuilder.UpdateData(
                table: "Buildings",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(4425));

            migrationBuilder.UpdateData(
                table: "Buildings",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(4430));

            migrationBuilder.UpdateData(
                table: "Buildings",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(4435));

            migrationBuilder.UpdateData(
                table: "Floors",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(4466));

            migrationBuilder.UpdateData(
                table: "Floors",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(4472));

            migrationBuilder.UpdateData(
                table: "Floors",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(4477));

            migrationBuilder.UpdateData(
                table: "Floors",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(4482));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(4494));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(4502));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(4508));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(4513));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(4518));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(4523));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(4528));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(4533));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(4538));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(4543));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(4548));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(4553));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(4559));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(4564));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(4570));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(4575));

            migrationBuilder.UpdateData(
                table: "Sites",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(4227));

            migrationBuilder.UpdateData(
                table: "Zones",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222221"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(4363));

            migrationBuilder.UpdateData(
                table: "Zones",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(4370));

            migrationBuilder.UpdateData(
                table: "Zones",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222223"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(4375));

            migrationBuilder.UpdateData(
                table: "Zones",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222224"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 11, 39, 3, 887, DateTimeKind.Utc).AddTicks(4380));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 19, 1, 11, 4, 792, DateTimeKind.Utc).AddTicks(503), new DateTime(2025, 11, 19, 1, 11, 4, 792, DateTimeKind.Utc).AddTicks(513), new DateTime(2026, 4, 19, 1, 11, 4, 792, DateTimeKind.Utc).AddTicks(524), new DateTime(2026, 7, 19, 1, 11, 4, 792, DateTimeKind.Utc).AddTicks(526) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "InstallDate" },
                values: new object[] { new DateTime(2026, 5, 19, 1, 11, 4, 792, DateTimeKind.Utc).AddTicks(534), new DateTime(2026, 1, 19, 1, 11, 4, 792, DateTimeKind.Utc).AddTicks(538) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 19, 1, 11, 4, 792, DateTimeKind.Utc).AddTicks(545), new DateTime(2025, 9, 19, 1, 11, 4, 792, DateTimeKind.Utc).AddTicks(551), new DateTime(2026, 3, 19, 1, 11, 4, 792, DateTimeKind.Utc).AddTicks(552), new DateTime(2026, 6, 19, 1, 11, 4, 792, DateTimeKind.Utc).AddTicks(554) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "InstallDate" },
                values: new object[] { new DateTime(2026, 5, 19, 1, 11, 4, 792, DateTimeKind.Utc).AddTicks(561), new DateTime(2025, 5, 19, 1, 11, 4, 792, DateTimeKind.Utc).AddTicks(565) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate" },
                values: new object[] { new DateTime(2026, 5, 19, 1, 11, 4, 792, DateTimeKind.Utc).AddTicks(573), new DateTime(2026, 2, 19, 1, 11, 4, 792, DateTimeKind.Utc).AddTicks(576), new DateTime(2026, 5, 5, 1, 11, 4, 792, DateTimeKind.Utc).AddTicks(577) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "InstallDate" },
                values: new object[] { new DateTime(2026, 5, 19, 1, 11, 4, 792, DateTimeKind.Utc).AddTicks(599), new DateTime(2025, 12, 19, 1, 11, 4, 792, DateTimeKind.Utc).AddTicks(603) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "InstallDate", "LastServiceDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 19, 1, 11, 4, 792, DateTimeKind.Utc).AddTicks(610), new DateTime(2025, 7, 19, 1, 11, 4, 792, DateTimeKind.Utc).AddTicks(614), new DateTime(2026, 4, 19, 1, 11, 4, 792, DateTimeKind.Utc).AddTicks(615), new DateTime(2026, 8, 19, 1, 11, 4, 792, DateTimeKind.Utc).AddTicks(616) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "InstallDate", "MaintenanceDueDate" },
                values: new object[] { new DateTime(2026, 5, 19, 1, 11, 4, 792, DateTimeKind.Utc).AddTicks(623), new DateTime(2026, 3, 19, 1, 11, 4, 792, DateTimeKind.Utc).AddTicks(631), new DateTime(2026, 9, 19, 1, 11, 4, 792, DateTimeKind.Utc).AddTicks(632) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 1, 11, 4, 792, DateTimeKind.Utc).AddTicks(640));

            migrationBuilder.UpdateData(
                table: "Buildings",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 1, 11, 4, 791, DateTimeKind.Utc).AddTicks(6639));

            migrationBuilder.UpdateData(
                table: "Buildings",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 1, 11, 4, 791, DateTimeKind.Utc).AddTicks(6654));

            migrationBuilder.UpdateData(
                table: "Buildings",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 1, 11, 4, 791, DateTimeKind.Utc).AddTicks(6662));

            migrationBuilder.UpdateData(
                table: "Buildings",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 1, 11, 4, 791, DateTimeKind.Utc).AddTicks(6670));

            migrationBuilder.UpdateData(
                table: "Floors",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 1, 11, 4, 791, DateTimeKind.Utc).AddTicks(6726));

            migrationBuilder.UpdateData(
                table: "Floors",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 1, 11, 4, 791, DateTimeKind.Utc).AddTicks(6737));

            migrationBuilder.UpdateData(
                table: "Floors",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 1, 11, 4, 791, DateTimeKind.Utc).AddTicks(6745));

            migrationBuilder.UpdateData(
                table: "Floors",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 1, 11, 4, 791, DateTimeKind.Utc).AddTicks(6752));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 1, 11, 4, 791, DateTimeKind.Utc).AddTicks(6772));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 1, 11, 4, 791, DateTimeKind.Utc).AddTicks(6782));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 1, 11, 4, 791, DateTimeKind.Utc).AddTicks(6790));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 1, 11, 4, 791, DateTimeKind.Utc).AddTicks(6797));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 1, 11, 4, 791, DateTimeKind.Utc).AddTicks(6804));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 1, 11, 4, 791, DateTimeKind.Utc).AddTicks(6811));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 1, 11, 4, 791, DateTimeKind.Utc).AddTicks(6819));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 1, 11, 4, 791, DateTimeKind.Utc).AddTicks(6826));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 1, 11, 4, 791, DateTimeKind.Utc).AddTicks(6835));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 1, 11, 4, 791, DateTimeKind.Utc).AddTicks(6843));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 1, 11, 4, 791, DateTimeKind.Utc).AddTicks(6851));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 1, 11, 4, 791, DateTimeKind.Utc).AddTicks(6859));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 1, 11, 4, 791, DateTimeKind.Utc).AddTicks(6868));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 1, 11, 4, 791, DateTimeKind.Utc).AddTicks(6877));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 1, 11, 4, 791, DateTimeKind.Utc).AddTicks(6885));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 1, 11, 4, 791, DateTimeKind.Utc).AddTicks(6893));

            migrationBuilder.UpdateData(
                table: "Sites",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 1, 11, 4, 791, DateTimeKind.Utc).AddTicks(6115));

            migrationBuilder.UpdateData(
                table: "Zones",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222221"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 1, 11, 4, 791, DateTimeKind.Utc).AddTicks(6545));

            migrationBuilder.UpdateData(
                table: "Zones",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 1, 11, 4, 791, DateTimeKind.Utc).AddTicks(6558));

            migrationBuilder.UpdateData(
                table: "Zones",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222223"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 1, 11, 4, 791, DateTimeKind.Utc).AddTicks(6565));

            migrationBuilder.UpdateData(
                table: "Zones",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222224"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 1, 11, 4, 791, DateTimeKind.Utc).AddTicks(6573));
        }
    }
}
