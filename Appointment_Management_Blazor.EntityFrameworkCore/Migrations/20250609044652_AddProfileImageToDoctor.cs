using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Appointment_Management_Blazor.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileImageToDoctor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfileImagePath",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileImagePath",
                table: "Doctors");
        }
    }
}
