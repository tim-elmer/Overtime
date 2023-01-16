﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Overtime.Model;

#nullable disable

namespace Overtime.Migrations
{
    [DbContext(typeof(Context))]
    [Migration("20230115215508_Unsplit")]
    partial class Unsplit
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.2");

            modelBuilder.Entity("Overtime.Model.UserInformation", b =>
                {
                    b.Property<ulong>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("NoRemind")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TimeZoneId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("UserId");

                    b.ToTable("UserInformation");
                });
#pragma warning restore 612, 618
        }
    }
}