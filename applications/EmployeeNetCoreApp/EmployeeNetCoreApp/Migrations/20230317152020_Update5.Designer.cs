﻿// <auto-generated />
using System;
using EmployeeNetCoreApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace EmployeeNetCoreApp.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20230317152020_Update5")]
    partial class Update5
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("EmployeeNetCoreApp.Model.Department", b =>
                {
                    b.Property<int>("DepartmentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("DepartmentId"));

                    b.Property<string>("DepartmentName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("DepartmentId");

                    b.ToTable("Departments");

                    b.HasData(
                        new
                        {
                            DepartmentId = 1,
                            DepartmentName = "IT"
                        },
                        new
                        {
                            DepartmentId = 2,
                            DepartmentName = "HR"
                        },
                        new
                        {
                            DepartmentId = 3,
                            DepartmentName = "Marketing"
                        },
                        new
                        {
                            DepartmentId = 4,
                            DepartmentName = "Sales"
                        });
                });

            modelBuilder.Entity("EmployeeNetCoreApp.Model.Employee", b =>
                {
                    b.Property<int>("EmployeeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("EmployeeId"));

                    b.Property<DateTime?>("CreateDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValue(new DateTime(2023, 3, 17, 15, 20, 20, 844, DateTimeKind.Utc).AddTicks(1820));

                    b.Property<string>("CreatedBy")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(max)")
                        .HasDefaultValue("1");

                    b.Property<int>("Department")
                        .HasColumnType("int");

                    b.Property<string>("Designation")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UUID")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("UpdatedDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("Version")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(1);

                    b.HasKey("EmployeeId");

                    b.HasIndex("Department");

                    b.ToTable("Employees");
                });

            modelBuilder.Entity("EmployeeNetCoreApp.Model.Employee", b =>
                {
                    b.HasOne("EmployeeNetCoreApp.Model.Department", null)
                        .WithMany("Employees")
                        .HasForeignKey("Department")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("EmployeeNetCoreApp.Model.Department", b =>
                {
                    b.Navigation("Employees");
                });
#pragma warning restore 612, 618
        }
    }
}
