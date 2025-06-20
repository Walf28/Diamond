using Diamond.Models.Factory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Diamond.Database.Configurations
{
    public class PlanConfiguration : IEntityTypeConfiguration<Part>
    {
        public void Configure(EntityTypeBuilder<Part> builder)
        {
            builder.HasKey(p => p.Id);

            #region Связи
            // С заводом
            builder.HasOne(p => p.Factory).WithMany(f => f.Plan)
                .HasPrincipalKey(f => f.Id)
                .HasForeignKey(p => p.FactoryId)
                .OnDelete(DeleteBehavior.Cascade);
            // С маршрутом
            builder.HasOne(p => p.Route).WithMany(r => r.Part)
                .HasPrincipalKey(r => r.Id)
                .HasForeignKey(p => p.RouteId)
                .OnDelete(DeleteBehavior.Cascade);
            // С участком
            builder.HasOne(p => p.Region).WithOne(r => r.Part)
                .HasPrincipalKey<Region>(r => r.Id)
                .HasForeignKey<Part>(p => p.RegionId)
                .OnDelete(DeleteBehavior.Cascade);
            // С продукцией
            builder.HasOne(p => p.Product).WithMany(ps => ps.Plans)
                .HasPrincipalKey(pg => pg.Id)
                .HasForeignKey(p => p.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            // Сырьё
            builder.HasOne(p => p.Material).WithMany(m => m.Plans)
                .HasPrincipalKey(m => m.Id)
                .HasForeignKey(p => p.MaterialId)
                .OnDelete(DeleteBehavior.Cascade);
            // С заказом
            builder.HasOne(p => p.Order).WithMany(o => o.Plan)
                .HasPrincipalKey(m => m.Id)
                .HasForeignKey(p => p.MaterialId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}