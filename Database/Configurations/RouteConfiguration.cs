using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyCompany.Database.Configurations
{
    public class RouteConfiguration : IEntityTypeConfiguration<Route>
    {
        public void Configure(EntityTypeBuilder<Route> builder)
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
            // С заказами
            builder.HasMany(r => r.Requests).WithOne(r => r.Route)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}