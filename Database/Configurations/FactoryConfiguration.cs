using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace MyCompany.Database.Configurations
{
    public class FactoryConfiguration : IEntityTypeConfiguration<Factory>
    {
        public void Configure(EntityTypeBuilder<Factory> builder)
        {
            builder.HasKey(f => f.Id);
            builder.HasIndex(f => f.Name).IsUnique();

            #region Связи
            // С маршрутами
            builder.HasMany(f => f.Routes).WithOne(r => r.Factory)
                .OnDelete(DeleteBehavior.Cascade);
            // С участками
            builder.HasMany(f => f.Regions).WithOne(r => r.Factory)
                .HasPrincipalKey(f => f.Id)
                .HasForeignKey(r => r.FactoryId)
                .OnDelete(DeleteBehavior.Cascade);
            // С заказами
            builder.HasMany(f => f.Requests).WithOne(r => r.Factory)
                .OnDelete(DeleteBehavior.SetNull);
            #endregion
        }
    }
}