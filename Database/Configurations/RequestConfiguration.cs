using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyCompany.Database.Configurations
{
    public class RequestConfiguration : IEntityTypeConfiguration<Request>
    {
        public void Configure(EntityTypeBuilder<Request> builder)
        {
            builder.HasKey(r => r.Id);

            #region Связи
            // С заводом
            builder.HasOne(r => r.Factory).WithMany(f => f.Requests)
                .OnDelete(DeleteBehavior.SetNull);

            // С маршрутом
            builder.HasOne(r => r.Route).WithMany(r => r.Requests)
                .OnDelete(DeleteBehavior.SetNull);

            // С товаром
            builder.HasOne(r => r.Product).WithMany(p=>p.Requests)
                .OnDelete(DeleteBehavior.SetNull);
            #endregion
        }
    }
}