using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyCompany.Database.Configurations
{
    public class MaterialConfiguration : IEntityTypeConfiguration<Material>
    {
        public void Configure(EntityTypeBuilder<Material> builder)
        {
            builder.HasKey(m => m.Id);

            #region Связи
            // С материалом для конкретного участка
            builder.HasMany(m => m.Materials).WithOne(m => m.Material)
                .OnDelete(DeleteBehavior.Cascade);
            // Продукты
            builder.HasMany(m => m.Products).WithOne(p => p.Material)
                .OnDelete(DeleteBehavior.SetNull);
            #endregion
        }
    }
}