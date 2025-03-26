using Diamond.Models.Factory;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diamond.Models.Products
{
    public class ProductSpecificWarehouse
    {
        #region Поля
        #region Обычные
        public int Id { get; set; }
        public required int ProductId { get; set; }
        public required int WarehouseId { get; set; }
        public int Count { get; set; } = 0;
        #endregion

        #region Ссылочные
        [ForeignKey(nameof(ProductId))]
        public ProductSpecific Product { get; set; } = null!;
        [ForeignKey(nameof(WarehouseId))]
        public Warehouse Warehouse { get; set; } = null!;
        #endregion
        #endregion
    }
}