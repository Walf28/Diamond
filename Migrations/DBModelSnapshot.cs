﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MyCompany;
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

            modelBuilder.Entity("MyCompany.Factory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Factories");
                });

            modelBuilder.Entity("MyCompany.Material", b =>
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

            modelBuilder.Entity("MyCompany.MaterialForRegion", b =>
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

            modelBuilder.Entity("MyCompany.Models.Downtime", b =>
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

            modelBuilder.Entity("MyCompany.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("MaterialId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<int>("Price")
                        .HasColumnType("integer");

                    b.Property<int>("Size")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("MaterialId");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("MyCompany.Region", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int?>("DowntimeId")
                        .HasColumnType("integer");

                    b.Property<int>("FactoryId")
                        .HasColumnType("integer");

                    b.Property<int>("MaxVolume")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<int>("TransitTime")
                        .HasColumnType("integer");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.Property<int>("Workload")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("FactoryId");

                    b.ToTable("Regions");
                });

            modelBuilder.Entity("MyCompany.Request", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("Count")
                        .HasColumnType("integer");

                    b.Property<int?>("FactoryId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("ProductId")
                        .HasColumnType("integer");

                    b.Property<int?>("RouteId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("FactoryId");

                    b.HasIndex("ProductId");

                    b.HasIndex("RouteId");

                    b.ToTable("Requests");
                });

            modelBuilder.Entity("MyCompany.Route", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("FactoryId")
                        .HasColumnType("integer");

                    b.Property<int>("MaxPower")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("Id");

                    b.HasIndex("FactoryId");

                    b.ToTable("Routes");
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

            modelBuilder.Entity("MyCompany.MaterialForRegion", b =>
                {
                    b.HasOne("MyCompany.Material", "Material")
                        .WithMany("Materials")
                        .HasForeignKey("MaterialId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MyCompany.Region", "Region")
                        .WithMany("Materials")
                        .HasForeignKey("RegionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Material");

                    b.Navigation("Region");
                });

            modelBuilder.Entity("MyCompany.Models.Downtime", b =>
                {
                    b.HasOne("MyCompany.Region", "Region")
                        .WithOne("Downtime")
                        .HasForeignKey("MyCompany.Models.Downtime", "Id")
                        .OnDelete(DeleteBehavior.SetNull)
                        .IsRequired();

                    b.Navigation("Region");
                });

            modelBuilder.Entity("MyCompany.Product", b =>
                {
                    b.HasOne("MyCompany.Material", "Material")
                        .WithMany("Products")
                        .HasForeignKey("MaterialId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Material");
                });

            modelBuilder.Entity("MyCompany.Region", b =>
                {
                    b.HasOne("MyCompany.Factory", "Factory")
                        .WithMany("Regions")
                        .HasForeignKey("FactoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Factory");
                });

            modelBuilder.Entity("MyCompany.Request", b =>
                {
                    b.HasOne("MyCompany.Factory", "Factory")
                        .WithMany("Requests")
                        .HasForeignKey("FactoryId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("MyCompany.Product", "Product")
                        .WithMany("Requests")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MyCompany.Route", "Route")
                        .WithMany("Requests")
                        .HasForeignKey("RouteId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Factory");

                    b.Navigation("Product");

                    b.Navigation("Route");
                });

            modelBuilder.Entity("MyCompany.Route", b =>
                {
                    b.HasOne("MyCompany.Factory", "Factory")
                        .WithMany("Routes")
                        .HasForeignKey("FactoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Factory");
                });

            modelBuilder.Entity("RegionRegion", b =>
                {
                    b.HasOne("MyCompany.Region", null)
                        .WithMany()
                        .HasForeignKey("RegionsChildrensId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MyCompany.Region", null)
                        .WithMany()
                        .HasForeignKey("RegionsParentsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("RegionRoute", b =>
                {
                    b.HasOne("MyCompany.Region", null)
                        .WithMany()
                        .HasForeignKey("RegionsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MyCompany.Route", null)
                        .WithMany()
                        .HasForeignKey("RoutesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MyCompany.Factory", b =>
                {
                    b.Navigation("Regions");

                    b.Navigation("Requests");

                    b.Navigation("Routes");
                });

            modelBuilder.Entity("MyCompany.Material", b =>
                {
                    b.Navigation("Materials");

                    b.Navigation("Products");
                });

            modelBuilder.Entity("MyCompany.Product", b =>
                {
                    b.Navigation("Requests");
                });

            modelBuilder.Entity("MyCompany.Region", b =>
                {
                    b.Navigation("Downtime");

                    b.Navigation("Materials");
                });

            modelBuilder.Entity("MyCompany.Route", b =>
                {
                    b.Navigation("Requests");
                });
#pragma warning restore 612, 618
        }
    }
}
