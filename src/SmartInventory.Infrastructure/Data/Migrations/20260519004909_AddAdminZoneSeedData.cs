using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartInventory.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminZoneSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Zones",
                columns: new[] { "Id", "Code", "CreatedAt", "Description", "Name", "SiteId", "UpdatedAt" },
                values: new object[] { new Guid("22222222-2222-2222-2222-222222222224"), "ADM", new DateTime(2026, 5, 19, 0, 0, 0, 0, DateTimeKind.Utc), null, "Département Administration", new Guid("11111111-1111-1111-1111-111111111111"), null });

            migrationBuilder.InsertData(
                table: "Buildings",
                columns: new[] { "Id", "Code", "CreatedAt", "Description", "Name", "UpdatedAt", "ZoneId" },
                values: new object[] { new Guid("33333333-3333-3333-3333-000000000004"), "ADMIN", new DateTime(2026, 5, 19, 0, 0, 0, 0, DateTimeKind.Utc), null, "Bâtiment Administratif", null, new Guid("22222222-2222-2222-2222-222222222224") });

            migrationBuilder.InsertData(
                table: "Floors",
                columns: new[] { "Id", "BuildingId", "CreatedAt", "Description", "Level", "UpdatedAt" },
                values: new object[] { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-000000000004"), new Guid("33333333-3333-3333-3333-000000000004"), new DateTime(2026, 5, 19, 0, 0, 0, 0, DateTimeKind.Utc), "Rez-de-chaussée", 0, null });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "Id", "Code", "CreatedAt", "Description", "FloorId", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("40000000-0000-0000-0000-000000000001"), "ADM1", new DateTime(2026, 5, 19, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-000000000004"), "Bureau du Directeur", null },
                    { new Guid("40000000-0000-0000-0000-000000000002"), "ADM2", new DateTime(2026, 5, 19, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-000000000004"), "Salle de Réunion", null },
                    { new Guid("40000000-0000-0000-0000-000000000003"), "ADM3", new DateTime(2026, 5, 19, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-000000000004"), "Bureau Administration", null },
                    { new Guid("40000000-0000-0000-0000-000000000004"), "STOCK", new DateTime(2026, 5, 19, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-000000000004"), "Salle de Stockage", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Floors",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-000000000004"));

            migrationBuilder.DeleteData(
                table: "Buildings",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-000000000004"));

            migrationBuilder.DeleteData(
                table: "Zones",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222224"));
        }
    }
}
