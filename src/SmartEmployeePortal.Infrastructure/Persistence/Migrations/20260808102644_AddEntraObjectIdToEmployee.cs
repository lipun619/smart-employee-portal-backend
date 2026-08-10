using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartEmployeePortal.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEntraObjectIdToEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EntraObjectId",
                table: "Employees",
                type: "nvarchar(36)",
                maxLength: 36,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntraObjectId",
                table: "Employees");
        }
    }
}
