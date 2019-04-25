﻿// <auto-generated />
using System;
using IReckonUpload;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace IReckonUpload.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20190425004850_AddRawDeliveryRange")]
    partial class AddRawDeliveryRange
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("IReckonUpload.Models.Business.Color", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Code");

                    b.Property<string>("Label");

                    b.HasKey("Id");

                    b.ToTable("Color");
                });

            modelBuilder.Entity("IReckonUpload.Models.Business.DeliveryRange", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("RangeEnd");

                    b.Property<int>("RangeStart");

                    b.Property<string>("Raw");

                    b.Property<string>("Unit");

                    b.HasKey("Id");

                    b.ToTable("DeliveryRange");
                });

            modelBuilder.Entity("IReckonUpload.Models.Business.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ArticleCode");

                    b.Property<int?>("ColorId");

                    b.Property<int?>("DeliveredInId");

                    b.Property<string>("Description");

                    b.Property<double>("DiscountedPrice");

                    b.Property<string>("Key");

                    b.Property<double>("Price");

                    b.Property<string>("Q1");

                    b.Property<string>("Size");

                    b.HasKey("Id");

                    b.HasIndex("ColorId");

                    b.HasIndex("DeliveredInId");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("IReckonUpload.Models.Consumers.Consumer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Password");

                    b.Property<string>("Username");

                    b.HasKey("Id");

                    b.ToTable("Consumers");
                });

            modelBuilder.Entity("IReckonUpload.Models.Business.Product", b =>
                {
                    b.HasOne("IReckonUpload.Models.Business.Color", "Color")
                        .WithMany("Products")
                        .HasForeignKey("ColorId");

                    b.HasOne("IReckonUpload.Models.Business.DeliveryRange", "DeliveredIn")
                        .WithMany("Products")
                        .HasForeignKey("DeliveredInId");
                });
#pragma warning restore 612, 618
        }
    }
}