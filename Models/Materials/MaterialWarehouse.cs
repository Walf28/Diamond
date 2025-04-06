using Diamond.Models.Factory;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diamond.Models.Materials
{
    public class MaterialWarehouse
    {
        #region Поля
        #region Обычные
        public int Id { get; set; }
        public int Count { get; set; } = 0;
        public required int MaterialId { get; set; }
        public required int WarehouseId { get; set; }
        #endregion

        #region Ссылочные
        [ForeignKey(nameof(MaterialId))]
        public Material Material { get; set; } = null!;
        [ForeignKey(nameof(WarehouseId))]
        public Warehouse Warehouse { get; set; } = null!;
        #endregion
        #endregion
    }
}