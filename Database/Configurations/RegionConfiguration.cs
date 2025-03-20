using Diamond.Models;
using Diamond.Models.Factory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Diamond.Database.Configurations
{
    public class RegionConfiguration : IEntityTypeConfiguration<Region>
    {
        public void Configure(EntityTypeBuilder<Region> builder)
        {
            builder.HasKey(r => r.Id);

            #region Связи
            // С заводом
            builder.HasOne(r => r.Factory).WithMany(f => f.Regions)
                .HasPrincipalKey(f => f.Id)
                .HasForeignKey(r => r.FactoryId)
                .OnDelete(DeleteBehavior.Cascade);
            // С маршрутами
            builder.HasMany(r => r.Routes).WithMany(r => r.Regions);
            // С родительскими участками
            builder.HasMany(r => r.RegionsParents).WithMany(r => r.RegionsChildrens);
            // С подчинёнными участками
            builder.HasMany(r => r.RegionsChildrens).WithMany(r => r.RegionsParents);
            // Простой
            builder.HasOne(r => r.Downtime).WithOne(d => d.Region)
                .HasForeignKey<Region>(r => r.Id)
                .OnDelete(DeleteBehavior.Cascade);
            // Сырьё
            builder.HasMany(r => r.Materials).WithOne(m => m.Region)
                .HasForeignKey(m => m.RegionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(r => r.MaterialOptionNow).WithMany(m => m.RegionsOptions)
                .HasPrincipalKey(m => m.Id)
                .HasForeignKey(r => r.MaterialOptionNowId)
                .OnDelete(DeleteBehavior.Cascade);
            // Ссылка на себя самого
            builder.HasMany(r => r.RegionsChildrens).WithMany(r => r.RegionsParents);
            builder.HasMany(r => r.RegionsParents).WithMany(r => r.RegionsChildrens);
            // С планом
            builder.HasOne(r => r.Plan).WithOne(p => p.Region)
                .HasPrincipalKey<Region>(r => r.Id)
                .HasForeignKey<Plan>(p => p.Id)
                .OnDelete(DeleteBehavior.SetNull);
            #endregion
        }
    }
}