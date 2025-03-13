using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyCompany.Database.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(p => p.Id);

            #region Связи
            // C заявками
            builder.HasMany(p => p.Requests).WithOne(r => r.Product)
                .OnDelete(DeleteBehavior.Cascade);
            // С сырьём
            builder.HasOne(p => p.Material).WithMany(m => m.Products)
                .HasPrincipalKey(m => m.Id)
                .HasForeignKey(p => p.MaterialId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}