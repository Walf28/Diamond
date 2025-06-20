using Diamond.Models.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Diamond.Database.Configurations
{
    public class OrderPartConfiguration : IEntityTypeConfiguration<OrderPart>
    {
        public void Configure(EntityTypeBuilder<OrderPart> builder)
        {
            builder.HasKey(r => r.Id);

            #region Связи
            // С заказом
            builder.HasOne(op => op.Order).WithMany(o => o.OrderParts)
                .HasForeignKey(op => op.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // С продукцией
            builder.HasOne(op => op.Product).WithMany(p => p.OrderParts)
                .HasForeignKey(op => op.PackageId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}