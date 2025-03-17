using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Diamond.Models;

namespace Diamond.Database.Configurations
{
    public class RouteConfiguration : IEntityTypeConfiguration<Models.Route>
    {
        public void Configure(EntityTypeBuilder<Models.Route> builder)
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
            #endregion
        }
    }
}