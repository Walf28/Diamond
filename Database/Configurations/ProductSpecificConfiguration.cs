using Diamond.Models;
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
            // С продукцией
            builder.HasOne(ps => ps.ProductGroup).WithMany(pg => pg.ProductsSpecific)
                .HasPrincipalKey(pg => pg.Id)
                .OnDelete(DeleteBehavior.Cascade);
            // C заявками
            builder.HasMany(p => p.Requests).WithOne(r => r.Product)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}