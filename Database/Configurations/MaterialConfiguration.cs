using Diamond.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Diamond.Database.Configurations
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
                .HasPrincipalKey(m => m.Id)
                .HasForeignKey(p => p.MaterialId)
                .OnDelete(DeleteBehavior.Cascade);
            // С участками
            builder.HasMany(m=>m.RegionsOptions).WithOne(r=>r.MaterialOptionNow)
                .HasPrincipalKey(m=>m.Id)
                .HasForeignKey(r=>r.MaterialOptionNowId)
                .OnDelete(DeleteBehavior.SetNull);
            #endregion
        }
    }
}