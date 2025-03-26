using Diamond.Models;
using Diamond.Models.Factory;
using Diamond.Models.Materials;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Diamond.Database.Configurations
{
    public class MaterialWarehouseConfiguration : IEntityTypeConfiguration<MaterialWarehouse>
    {
        public void Configure(EntityTypeBuilder<MaterialWarehouse> builder)
        {
            builder.HasKey(m => m.Id);

            #region Связи
            // Склад
            builder.HasOne(mw => mw.Warehouse).WithMany(w => w.Materials)
                .HasForeignKey(mw => mw.WarehouseId)
                .OnDelete(DeleteBehavior.Cascade);
            // Сырьё
            builder.HasOne(mw => mw.Material).WithMany(m => m.MaterialWarehouses)
                .HasForeignKey(mw => mw.MaterialId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}