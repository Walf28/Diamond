using Microsoft.EntityFrameworkCore;
using MyCompany.Models;
using MyCompany.Database.Configurations;
using System.ComponentModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MyCompany
{
    public class DB(DbContextOptions options) : DbContext(options)
    {
        private static string ConnectionString = "Database=MyCompany;Host=localhost;Username=postgres;Password=111;";

        public static string GetConnectionString => ConnectionString;

        /*string cmdDeleteMigrations = "Remove-Migration";
        string cmdCreateMigrations = "Add-Migration InitialCreate";
        string cmdUpdateMigrations = "Update-Database";
        string cmdDeleteMigrations = "dotnet ef migrations remove";
        string cmdCreateMigrations = "dotnet ef migrations add InitialCreate";
        string cmdUpdateMigrations = "dotnet ef database update";*/

        public required DbSet<Factory> Factories { get; set; }
        public required DbSet<Route> Routes { get; set; }
        public required DbSet<Region> Regions { get; set; }
        public required DbSet<Downtime> Downtimes { get; set; }
        public required DbSet<Product> Products { get; set; }
        public required DbSet<Material> Materials { get; set; }
        public required DbSet<MaterialForRegion> RegionsMaterials { get; set; }
        public required DbSet<Request> Requests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Принятие конфигураций
            modelBuilder.ApplyConfiguration(new FactoryConfiguration());
            modelBuilder.ApplyConfiguration(new RouteConfiguration());
            modelBuilder.ApplyConfiguration(new RegionConfiguration());
            modelBuilder.ApplyConfiguration(new DowntimeConfiguration());
            modelBuilder.ApplyConfiguration(new MaterialConfiguration());
            modelBuilder.ApplyConfiguration(new MaterialForRegionConfiguration());
            modelBuilder.ApplyConfiguration(new RequestConfiguration());
            modelBuilder.ApplyConfiguration(new ProductConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}