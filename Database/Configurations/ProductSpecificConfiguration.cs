using Diamond.Models.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Diamond.Database.Configurations
{
    public class ProductSpecificConfiguration : IEntityTypeConfiguration<ProductSpecific>
    {
        public void Configure(EntityTypeBuilder<ProductSpecific> builder)
        {
            builder.HasKey(p => p.Id);

            #region Связи
            // С группой продукции
            builder.HasOne(ps => ps.ProductGroup).WithMany(pg => pg.ProductsSpecific)
                .HasPrincipalKey(pg => pg.Id)
                .HasForeignKey(ps => ps.ProductGroupId)
                .OnDelete(DeleteBehavior.Cascade);
            // C заявками
            builder.HasMany(p => p.OrderParts).WithOne(r => r.Product)
                .HasPrincipalKey(ps => ps.Id)
                .HasForeignKey(op => op.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            // С планом
            builder.HasMany(ps => ps.Plans).WithOne(p => p.Product)
                .HasPrincipalKey(ps => ps.Id)
                .HasForeignKey(p => p.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            // Склад
            builder.HasMany(ps => ps.ProductWarehouses).WithOne(psw => psw.Product)
                .HasForeignKey(psw => psw.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}