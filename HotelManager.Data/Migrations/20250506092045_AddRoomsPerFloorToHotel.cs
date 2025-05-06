using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomsPerFloorToHotel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RoomsPerFloor",
                table: "Hotels",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoomsPerFloor",
                table: "Hotels");
        }
    }
}
