using Diamond.Models.Materials;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Diamond.Database.Configurations
{
    public class MaterialForRegionConfiguration : IEntityTypeConfiguration<MaterialForRegion>
    {
        public void Configure(EntityTypeBuilder<MaterialForRegion> builder)
        {
            builder.HasKey(m => m.Id);

            #region Связи
            // С родительским материалом
            builder.HasOne(m => m.Material).WithMany(m => m.Materials)
                .HasPrincipalKey(m => m.Id)
                .HasForeignKey(m => m.MaterialId)
                .OnDelete(DeleteBehavior.Cascade);
            // С участком
            builder.HasOne(m => m.Region).WithMany(r => r.Materials)
                .HasPrincipalKey(r => r.Id)
                .HasForeignKey(m => m.RegionId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}