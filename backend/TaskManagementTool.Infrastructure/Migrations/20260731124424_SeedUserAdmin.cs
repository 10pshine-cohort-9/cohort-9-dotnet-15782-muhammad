using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TaskManagementTool.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedUserAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "PasswordHash", "RoleId", "UpdatedAt" },
                values: new object[,]
                {
                    // Admin Password: Admin@390 
                    // User Password: User@251
                    { 1, new DateTime(2026, 7, 31, 0, 0, 0, 0, DateTimeKind.Utc), "admin101@taskmanagertool.com", "Admin", "$2a$11$CIX09ywHumg69tSqjEeZne4vicwPZiz7/hr00vmEbusIIX0DULMQS", 1, null },
                    { 2, new DateTime(2026, 7, 31, 0, 0, 0, 0, DateTimeKind.Utc), "aliahmed45@gmail.com", "Ali Ahmed", "$2a$11$ZuCYGlZy6MsD4Uv9oy18deNV97muiXhMEa6QGtheMyRPjsS75nI8C", 2, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
