using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeNetCoreApp.Migrations
{
    /// <inheritdoc />
    public partial class Update4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Department",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Employees");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UUID",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: "1",
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDate",
                table: "Employees",
                type: "datetime2",
                nullable: true,
                defaultValue: new DateTime(2023, 3, 17, 14, 58, 26, 64, DateTimeKind.Utc).AddTicks(7700),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValue: new DateTime(2023, 3, 16, 1, 16, 56, 757, DateTimeKind.Utc).AddTicks(2940));

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "DepartmentName",
                table: "Departments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmentId",
                table: "Employees",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Departments_DepartmentId",
                table: "Employees",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Departments_DepartmentId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_DepartmentId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Employees");

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                table: "Employees",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UUID",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                table: "Employees",
                type: "int",
                nullable: true,
                defaultValue: 1,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldDefaultValue: "1");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDate",
                table: "Employees",
                type: "datetime2",
                nullable: true,
                defaultValue: new DateTime(2023, 3, 16, 1, 16, 56, 757, DateTimeKind.Utc).AddTicks(2940),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValue: new DateTime(2023, 3, 17, 14, 58, 26, 64, DateTimeKind.Utc).AddTicks(7700));

            migrationBuilder.AddColumn<int>(
                name: "Department",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DepartmentName",
                table: "Departments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
