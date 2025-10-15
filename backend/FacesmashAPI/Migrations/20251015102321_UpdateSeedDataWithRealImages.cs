using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacesmashAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeedDataWithRealImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PhotoUrl",
                value: "https://randomuser.me/api/portraits/women/65.jpg");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PhotoUrl",
                value: "https://randomuser.me/api/portraits/men/52.jpg");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "PhotoUrl",
                value: "https://randomuser.me/api/portraits/women/68.jpg");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PhotoUrl",
                value: "alice.jpg");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PhotoUrl",
                value: "bob.jpg");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "PhotoUrl",
                value: "charlie.jpg");
        }
    }
}
