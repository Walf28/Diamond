﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Diamond.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Diamond.Migrations
{
    [DbContext(typeof(DB))]
    partial class DBModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Diamond.Models.Downtime", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer");

                    b.Property<int?>("DowntimeDuration")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("DowntimeFinish")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("DowntimeReason")
                        .HasColumnType("text");

                    b.Property<DateTime?>("DowntimeStart")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("Downtimes");
                });

            modelBuilder.Entity("Diamond.Models.Factory.Factory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.PrimitiveCollection<List<int>>("ProductSumId")
                        .IsRequired()
                        .HasColumnType("integer[]");

                    b.PrimitiveCollection<List<int>>("ProductSumSize")
                        .IsRequired()
                        .HasColumnType("integer[]");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Factories");
                });

            modelBuilder.Entity("Diamond.Models.Factory.Plan", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer");

                    b.Property<DateTime>("ComingSoon")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsFabricating")
                        .HasColumnType("boolean");

                    b.Property<int>("ProductId")
                        .HasColumnType("integer");

                    b.Property<int?>("RegionId")
                        .HasColumnType("integer");

                    b.Property<int?>("RouteId")
                        .HasColumnType("integer");

                    b.Property<int>("Size")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ProductId");

                    b.HasIndex("RegionId")
                        .IsUnique();

                    b.HasIndex("RouteId");

                    b.ToTable("Plans");
                });

            modelBuilder.Entity("Diamond.Models.Factory.Region", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int?>("DowntimeId")
                        .HasColumnType("integer");

                    b.Property<int>("FactoryId")
                        .HasColumnType("integer");

                    b.Property<int?>("MaterialOptionNowId")
                        .HasColumnType("integer");

                    b.Property<int>("MaxVolume")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<int>("TransitTime")
                        .HasColumnType("integer");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.Property<int>("UploadedNow")
                        .HasColumnType("integer");

                    b.Property<int>("Workload")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("FactoryId");

                    b.HasIndex("MaterialOptionNowId");

                    b.ToTable("Regions");
                });

            modelBuilder.Entity("Diamond.Models.Factory.Route", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("FactoryId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.PrimitiveCollection<List<int>>("RegionsRoute")
                        .IsRequired()
                        .HasColumnType("integer[]");

                    b.HasKey("Id");

                    b.HasIndex("FactoryId");

                    b.ToTable("Routes");
                });

            modelBuilder.Entity("Diamond.Models.Material", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Materials");
                });

            modelBuilder.Entity("Diamond.Models.Materials.MaterialForRegion", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("MaterialId")
                        .HasColumnType("integer");

                    b.Property<int>("Power")
                        .HasColumnType("integer");

                    b.Property<int>("RegionId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("MaterialId");

                    b.HasIndex("RegionId");

                    b.ToTable("RegionsMaterials");
                });

            modelBuilder.Entity("Diamond.Models.ProductGroup", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("MaterialId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.PrimitiveCollection<int[]>("TechnologyProcessing")
                        .IsRequired()
                        .HasColumnType("integer[]");

                    b.HasKey("Id");

                    b.HasIndex("MaterialId");

                    b.ToTable("ProductsGroup");
                });

            modelBuilder.Entity("Diamond.Models.ProductSpecific", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("Price")
                        .HasColumnType("integer");

                    b.Property<int>("ProductGroupId")
                        .HasColumnType("integer");

                    b.Property<int>("Size")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ProductGroupId");

                    b.ToTable("ProductsSpecific");
                });

            modelBuilder.Entity("Diamond.Models.Request", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("Count")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("DateOfAcceptance")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("DateOfComplete")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("DateOfDesiredComplete")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("DateOfReceipt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int?>("FactoryId")
                        .HasColumnType("integer");

                    b.Property<int>("ProductId")
                        .HasColumnType("integer");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("FactoryId");

                    b.HasIndex("ProductId");

                    b.ToTable("Requests");
                });

            modelBuilder.Entity("RegionRegion", b =>
                {
                    b.Property<int>("RegionsChildrensId")
                        .HasColumnType("integer");

                    b.Property<int>("RegionsParentsId")
                        .HasColumnType("integer");

                    b.HasKey("RegionsChildrensId", "RegionsParentsId");

                    b.HasIndex("RegionsParentsId");

                    b.ToTable("RegionRegion");
                });

            modelBuilder.Entity("RegionRoute", b =>
                {
                    b.Property<int>("RegionsId")
                        .HasColumnType("integer");

                    b.Property<int>("RoutesId")
                        .HasColumnType("integer");

                    b.HasKey("RegionsId", "RoutesId");

                    b.HasIndex("RoutesId");

                    b.ToTable("RegionRoute");
                });

            modelBuilder.Entity("Diamond.Models.Downtime", b =>
                {
                    b.HasOne("Diamond.Models.Factory.Region", "Region")
                        .WithOne("Downtime")
                        .HasForeignKey("Diamond.Models.Downtime", "Id")
                        .OnDelete(DeleteBehavior.SetNull)
                        .IsRequired();

                    b.Navigation("Region");
                });

            modelBuilder.Entity("Diamond.Models.Factory.Plan", b =>
                {
                    b.HasOne("Diamond.Models.Factory.Factory", "Factory")
                        .WithMany("Plan")
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Diamond.Models.ProductSpecific", "Product")
                        .WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Diamond.Models.Factory.Region", "Region")
                        .WithOne("Plan")
                        .HasForeignKey("Diamond.Models.Factory.Plan", "RegionId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("Diamond.Models.Factory.Route", "Route")
                        .WithMany("Plan")
                        .HasForeignKey("RouteId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Factory");

                    b.Navigation("Product");

                    b.Navigation("Region");

                    b.Navigation("Route");
                });

            modelBuilder.Entity("Diamond.Models.Factory.Region", b =>
                {
                    b.HasOne("Diamond.Models.Factory.Factory", "Factory")
                        .WithMany("Regions")
                        .HasForeignKey("FactoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Diamond.Models.Material", "MaterialOptionNow")
                        .WithMany("RegionsOptions")
                        .HasForeignKey("MaterialOptionNowId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Factory");

                    b.Navigation("MaterialOptionNow");
                });

            modelBuilder.Entity("Diamond.Models.Factory.Route", b =>
                {
                    b.HasOne("Diamond.Models.Factory.Factory", "Factory")
                        .WithMany("Routes")
                        .HasForeignKey("FactoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Factory");
                });

            modelBuilder.Entity("Diamond.Models.Materials.MaterialForRegion", b =>
                {
                    b.HasOne("Diamond.Models.Material", "Material")
                        .WithMany("Materials")
                        .HasForeignKey("MaterialId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Diamond.Models.Factory.Region", "Region")
                        .WithMany("Materials")
                        .HasForeignKey("RegionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Material");

                    b.Navigation("Region");
                });

            modelBuilder.Entity("Diamond.Models.ProductGroup", b =>
                {
                    b.HasOne("Diamond.Models.Material", "Material")
                        .WithMany("Products")
                        .HasForeignKey("MaterialId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Material");
                });

            modelBuilder.Entity("Diamond.Models.ProductSpecific", b =>
                {
                    b.HasOne("Diamond.Models.ProductGroup", "ProductGroup")
                        .WithMany("ProductsSpecific")
                        .HasForeignKey("ProductGroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ProductGroup");
                });

            modelBuilder.Entity("Diamond.Models.Request", b =>
                {
                    b.HasOne("Diamond.Models.Factory.Factory", "Factory")
                        .WithMany("Requests")
                        .HasForeignKey("FactoryId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("Diamond.Models.ProductSpecific", "Product")
                        .WithMany("Requests")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Factory");

                    b.Navigation("Product");
                });

            modelBuilder.Entity("RegionRegion", b =>
                {
                    b.HasOne("Diamond.Models.Factory.Region", null)
                        .WithMany()
                        .HasForeignKey("RegionsChildrensId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Diamond.Models.Factory.Region", null)
                        .WithMany()
                        .HasForeignKey("RegionsParentsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("RegionRoute", b =>
                {
                    b.HasOne("Diamond.Models.Factory.Region", null)
                        .WithMany()
                        .HasForeignKey("RegionsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Diamond.Models.Factory.Route", null)
                        .WithMany()
                        .HasForeignKey("RoutesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Diamond.Models.Factory.Factory", b =>
                {
                    b.Navigation("Plan");

                    b.Navigation("Regions");

                    b.Navigation("Requests");

                    b.Navigation("Routes");
                });

            modelBuilder.Entity("Diamond.Models.Factory.Region", b =>
                {
                    b.Navigation("Downtime");

                    b.Navigation("Materials");

                    b.Navigation("Plan");
                });

            modelBuilder.Entity("Diamond.Models.Factory.Route", b =>
                {
                    b.Navigation("Plan");
                });

            modelBuilder.Entity("Diamond.Models.Material", b =>
                {
                    b.Navigation("Materials");

                    b.Navigation("Products");

                    b.Navigation("RegionsOptions");
                });

            modelBuilder.Entity("Diamond.Models.ProductGroup", b =>
                {
                    b.Navigation("ProductsSpecific");
                });

            modelBuilder.Entity("Diamond.Models.ProductSpecific", b =>
                {
                    b.Navigation("Requests");
                });
#pragma warning restore 612, 618
        }
    }
}
