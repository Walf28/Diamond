using Diamond.Models;
using Diamond.Models.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Diamond.Database.Configurations
{
    public class ProductSpecificWarehouseConfiguration : IEntityTypeConfiguration<ProductSpecificWarehouse>
    {
        public void Configure(EntityTypeBuilder<ProductSpecificWarehouse> builder)
        {
            builder.HasKey(p => p.Id);

            #region Связи
            // Склад
            builder.HasOne(psw=>psw.Warehouse).WithMany(w=>w.Products)
                .HasForeignKey(psw=>psw.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            // Продукция
            builder.HasOne(psw => psw.Product).WithMany(ps => ps.ProductWarehouses)
                .HasForeignKey(psw => psw.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}