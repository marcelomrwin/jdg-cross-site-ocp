using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeNetCoreApp.Migrations
{
    /// <inheritdoc />
    public partial class Update5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Departments_DepartmentId",
                table: "Employees");

            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                table: "Employees",
                newName: "Department");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_DepartmentId",
                table: "Employees",
                newName: "IX_Employees_Department");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDate",
                table: "Employees",
                type: "datetime2",
                nullable: true,
                defaultValue: new DateTime(2023, 3, 17, 15, 20, 20, 844, DateTimeKind.Utc).AddTicks(1820),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValue: new DateTime(2023, 3, 17, 14, 58, 26, 64, DateTimeKind.Utc).AddTicks(7700));

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Departments_Department",
                table: "Employees",
                column: "Department",
                principalTable: "Departments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Departments_Department",
                table: "Employees");

            migrationBuilder.RenameColumn(
                name: "Department",
                table: "Employees",
                newName: "DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_Department",
                table: "Employees",
                newName: "IX_Employees_DepartmentId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDate",
                table: "Employees",
                type: "datetime2",
                nullable: true,
                defaultValue: new DateTime(2023, 3, 17, 14, 58, 26, 64, DateTimeKind.Utc).AddTicks(7700),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValue: new DateTime(2023, 3, 17, 15, 20, 20, 844, DateTimeKind.Utc).AddTicks(1820));

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Departments_DepartmentId",
                table: "Employees",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
