using Diamond.Models.Factory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Diamond.Database.Configurations
{
    public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
    {
        public void Configure(EntityTypeBuilder<Warehouse> builder)
        {
            builder.HasKey(d => d.Id);

            #region Связи
            // Завод
            builder.HasOne(w => w.Factory).WithOne(f => f.Warehouse)
                .HasPrincipalKey<Factory>(f => f.Id)
                .HasForeignKey<Warehouse>(w => w.FactoryId)
                .OnDelete(DeleteBehavior.Cascade);
            // Сырьё
            builder.HasMany(w=> w.Materials).WithOne(m => m.Warehouse)
                .HasForeignKey(m => m.WarehouseId)
                .OnDelete(DeleteBehavior.Cascade);
            // Продукция
            builder.HasMany(w => w.Products).WithOne(p => p.Warehouse)
                .HasForeignKey(p => p.WarehouseId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}