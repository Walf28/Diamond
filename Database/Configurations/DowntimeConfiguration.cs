using Diamond.Models;
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
                .HasForeignKey<Downtime>(d => d.Id)
                .OnDelete(DeleteBehavior.SetNull);
            #endregion
        }
    }
}