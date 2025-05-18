using Diamond.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Diamond.Database.Configurations
{
    public class TechnologyConfiguration : IEntityTypeConfiguration<ProductionStage>
    {
        public void Configure(EntityTypeBuilder<ProductionStage> builder)
        {
            builder.HasKey(m => m.Id);

            #region Связи
            // Тип участка
            builder.HasMany(t => t.Regions).WithOne(r => r.Type)
                .HasForeignKey(r => r.TypeId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}