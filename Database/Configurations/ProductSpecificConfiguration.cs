using Diamond.Models.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Diamond.Database.Configurations
{
    public class ProductSpecificConfiguration : IEntityTypeConfiguration<Package>
    {
        public void Configure(EntityTypeBuilder<Package> builder)
        {
            builder.HasKey(p => p.Id);

            #region Связи
            // С группой продукции
            builder.HasOne(ps => ps.ProductGroup).WithMany(pg => pg.ProductsSpecific)
                .HasPrincipalKey(pg => pg.Id)
                .HasForeignKey(ps => ps.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            // C заявками
            builder.HasMany(p => p.OrderParts).WithOne(r => r.Product)
                .HasPrincipalKey(ps => ps.Id)
                .HasForeignKey(op => op.PackageId)
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