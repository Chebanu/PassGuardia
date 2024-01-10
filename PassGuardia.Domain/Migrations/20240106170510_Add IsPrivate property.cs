using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PassGuardia.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddIsPrivateproperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Is_Private",
                table: "passwords",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Is_Private",
                table: "passwords");
        }
    }
}