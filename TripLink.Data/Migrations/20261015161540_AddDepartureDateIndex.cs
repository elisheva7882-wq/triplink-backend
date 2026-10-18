using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TripLink.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartureDateIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Rides_DepartureAtUtc",
                table: "Rides",
                column: "DepartureAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rides_DepartureAtUtc",
                table: "Rides");
        }
    }
}
