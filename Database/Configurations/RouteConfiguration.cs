using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Diamond.Database.Configurations
{
    public class RouteConfiguration : IEntityTypeConfiguration<Models.Factory.Route>
    {
        public void Configure(EntityTypeBuilder<Models.Factory.Route> builder)
        {
            builder.HasKey(r => r.Id);

            #region Связи
            // С заводом
            builder.HasOne(r => r.Factory).WithMany(f => f.Routes)
                .HasPrincipalKey(f => f.Id)
                .HasForeignKey(r => r.FactoryId)
                .OnDelete(DeleteBehavior.Cascade);
            // С участками
            builder.HasMany(r => r.Regions).WithMany(r => r.Routes);
            // С планом
            builder.HasMany(f => f.Plan).WithOne(p => p.Route)
                .OnDelete(DeleteBehavior.SetNull);
            #endregion
        }
    }
}