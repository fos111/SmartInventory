using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartInventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceRectangleWithPolygon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Height",
                table: "ZoneSiteShapes");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "ZoneSiteShapes");

            migrationBuilder.DropColumn(
                name: "X",
                table: "ZoneSiteShapes");

            migrationBuilder.DropColumn(
                name: "Y",
                table: "ZoneSiteShapes");

            migrationBuilder.AddColumn<string>(
                name: "Points",
                table: "ZoneSiteShapes",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Points",
                table: "ZoneSiteShapes");

            migrationBuilder.AddColumn<double>(
                name: "Height",
                table: "ZoneSiteShapes",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Width",
                table: "ZoneSiteShapes",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "X",
                table: "ZoneSiteShapes",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Y",
                table: "ZoneSiteShapes",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
