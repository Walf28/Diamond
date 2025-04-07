using Diamond.Database;
using Diamond.Models.Products;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diamond.Models.Orders
{
    public class OrderPart
    {
        #region Поля
        #region Обычные
        public int Id { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Разрешены только положительные значения")]
        public int Count { get; set; } = 1; // Сколько заказали
        public int CountComplete { get; set; } = 0; // Сколько выполнено от заказа
        #endregion

        #region Ссылочные
        [NotMapped]
        public readonly DB context = new();
        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; } = null!;
        [ForeignKey(nameof(ProductId))]
        public ProductSpecific Product { get; set; } = null!;
        #endregion

        #region Id ссылок
        public int? OrderId { get; set; }
        public int ProductId { get; set; }
        #endregion
        #endregion
    }
}