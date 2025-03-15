using Diamond.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Diamond.Database.Configurations
{
    public class ProductGroupConfiguration : IEntityTypeConfiguration<ProductGroup>
    {
        public void Configure(EntityTypeBuilder<ProductGroup> builder)
        {
            builder.HasKey(p => p.Id);

            #region Связи
            // С конкретной продукцией
            builder.HasMany(pg => pg.ProductsSpecific).WithOne(ps => ps.ProductGroup)
                .HasPrincipalKey(pg => pg.Id)
                .HasForeignKey(pg => pg.ProductGroupId)
                .OnDelete(DeleteBehavior.Cascade);
            // C сырьём
            builder.HasOne(pg => pg.Material).WithMany(m => m.Products)
                .HasPrincipalKey(pg => pg.Id)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}