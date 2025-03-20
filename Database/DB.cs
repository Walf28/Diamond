using Diamond.Database.Configurations;
using Diamond.Models;
using Diamond.Models.Factory;
using Diamond.Models.Materials;
using Microsoft.EntityFrameworkCore;

namespace Diamond.Database
{
    public class DB : DbContext
    {
        public readonly static string ConnectionString = "Database=Diamond;Host=localhost;Username=postgres;Password=111;";
        public DbSet<Factory> Factories { get; set; } = null!;
        public DbSet<Models.Factory.Route> Routes { get; set; } = null!;
        public DbSet<Region> Regions { get; set; } = null!;
        public DbSet<Plan> Plans { get; set; } = null!;
        public DbSet<Downtime> Downtimes { get; set; } = null!;
        public DbSet<ProductGroup> ProductsGroup { get; set; } = null!;
        public DbSet<ProductSpecific> ProductsSpecific { get; set; } = null!;
        public DbSet<Material> Materials { get; set; } = null!;
        public DbSet<MaterialForRegion> RegionsMaterials { get; set; } = null!;
        public DbSet<Request> Requests { get; set; } = null!;

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
            modelBuilder.ApplyConfiguration(new FactoryConfiguration());
            modelBuilder.ApplyConfiguration(new RouteConfiguration());
            modelBuilder.ApplyConfiguration(new RegionConfiguration());
            modelBuilder.ApplyConfiguration(new DowntimeConfiguration());
            modelBuilder.ApplyConfiguration(new MaterialConfiguration());
            modelBuilder.ApplyConfiguration(new MaterialForRegionConfiguration());
            modelBuilder.ApplyConfiguration(new RequestConfiguration());
            modelBuilder.ApplyConfiguration(new ProductGroupConfiguration());
            modelBuilder.ApplyConfiguration(new ProductSpecificConfiguration());
            modelBuilder.ApplyConfiguration(new PlanConfiguration());

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