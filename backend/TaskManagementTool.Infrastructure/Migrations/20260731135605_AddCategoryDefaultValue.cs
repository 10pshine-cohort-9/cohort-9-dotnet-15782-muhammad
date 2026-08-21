using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagementTool.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryDefaultValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Tasks",
                type: "int",
                nullable: false,
                defaultValue: 4,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Tasks",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 4);
        }
    }
}
