using Diamond.Models.Factory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Diamond.Database.Configurations
{
    public class PlanConfiguration : IEntityTypeConfiguration<Plan>
    {
        public void Configure(EntityTypeBuilder<Plan> builder)
        {
            builder.HasKey(p => p.Id);

            #region Связи
            // С заводом
            builder.HasOne(p => p.Factory).WithMany(f => f.Plan)
                .HasPrincipalKey(f => f.Id)
                .HasForeignKey(p => p.Id)
                .OnDelete(DeleteBehavior.Cascade);
            // С маршрутом
            builder.HasOne(p => p.Route).WithMany(r => r.Plan)
                .HasPrincipalKey(r => r.Id)
                .HasForeignKey(p => p.RouteId)
                .OnDelete(DeleteBehavior.SetNull);
            // С участком
            builder.HasOne(p => p.Region).WithOne(r => r.Plan)
                .HasPrincipalKey<Region>(r => r.Id)
                .HasForeignKey<Plan>(p => p.RegionId)
                .OnDelete(DeleteBehavior.SetNull);
            #endregion
        }
    }
}