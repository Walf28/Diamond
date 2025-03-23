using Diamond.Models;
using Diamond.Models.Factory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Diamond.Database.Configurations
{
    public class DowntimeConfiguration : IEntityTypeConfiguration<Downtime>
    {
        public void Configure(EntityTypeBuilder<Downtime> builder)
        {
            builder.HasKey(d => d.Id);

            #region Связи
            // С участком
            builder.HasOne(d => d.Region).WithOne(r => r.Downtime)
                .HasPrincipalKey<Region>(r => r.Id)
                .HasForeignKey<Downtime>(d => d.RegionId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}