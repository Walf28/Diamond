﻿using Diamond.Database.Configurations;
using Diamond.Models;
using Diamond.Models.Factory;
using Diamond.Models.Materials;
using Diamond.Models.Orders;
using Diamond.Models.Products;
using Microsoft.EntityFrameworkCore;

namespace Diamond.Database
{
    public class DB : DbContext
    {
        public readonly static string ConnectionString = "Database=Diamond;Host=localhost;Username=postgres;Password=111;";
        public DbSet<Factory> Factories { get; set; } = null!;
        public DbSet<Models.Factory.Route> Routes { get; set; } = null!;
        public DbSet<Region> Regions { get; set; } = null!;
        public DbSet<Part> Plans { get; set; } = null!;
        public DbSet<Downtime> Downtimes { get; set; } = null!;
        public DbSet<Product> ProductsGroup { get; set; } = null!;
        public DbSet<Package> Package { get; set; } = null!;
        public DbSet<ProductWarehouse> ProductsWarehouse { get; set; } = null!;
        public DbSet<Material> Materials { get; set; } = null!;
        public DbSet<MaterialForRegion> RegionsMaterials { get; set; } = null!;
        public DbSet<MaterialWarehouse> MaterialsWarehouse { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderPart> OrderParts { get; set; } = null!;
        public DbSet<Warehouse> Warehouses { get; set; } = null!;
        public DbSet<ProductionStage> ProductionStage { get; set; } = null!;

        /*string cmdDeleteMigrations = "Remove-Migration";
        string cmdCreateMigrations = "Add-Migration InitialCreate";
        string cmdUpdateMigrations = "Update-Database";
        string cmdDeleteMigrations = "dotnet ef migrations remove";
        string cmdCreateMigrations = "dotnet ef migrations add InitialCreate";
        string cmdUpdateMigrations = "dotnet ef database update";*/

        public DB(DbContextOptions options) : base(options) { }
        public DB() { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Принятие конфигураций
            modelBuilder.ApplyConfiguration(new DowntimeConfiguration());
            modelBuilder.ApplyConfiguration(new FactoryConfiguration());
            modelBuilder.ApplyConfiguration(new MaterialConfiguration());
            modelBuilder.ApplyConfiguration(new MaterialForRegionConfiguration());
            modelBuilder.ApplyConfiguration(new MaterialWarehouseConfiguration());
            modelBuilder.ApplyConfiguration(new PlanConfiguration());
            modelBuilder.ApplyConfiguration(new ProductGroupConfiguration());
            modelBuilder.ApplyConfiguration(new ProductSpecificConfiguration());
            modelBuilder.ApplyConfiguration(new ProductSpecificWarehouseConfiguration());
            modelBuilder.ApplyConfiguration(new RegionConfiguration());
            modelBuilder.ApplyConfiguration(new OrderConfiguration());
            modelBuilder.ApplyConfiguration(new OrderPartConfiguration());
            modelBuilder.ApplyConfiguration(new RouteConfiguration());
            modelBuilder.ApplyConfiguration(new WarehouseConfiguration());
            modelBuilder.ApplyConfiguration(new WarehouseConfiguration());
            modelBuilder.ApplyConfiguration(new TechnologyConfiguration());

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(ConnectionString);
            }
        }
    }
}