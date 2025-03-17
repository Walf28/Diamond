using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diamond.Models
{
    public class ProductSpecific
    {
        #region Поля
        #region Обычные
        public int Id { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Разрешены только положительные значения")]
        public int Size { get; set; } // Размер в граммах
        [Range(0, int.MaxValue, ErrorMessage = "Разрешены только положительные значения")]
        public int Price { get; set; } // Цена
        #endregion

        #region Ссылочные
        [NotMapped]
        private readonly DB context = new();
        [ForeignKey("ProductGroupId")]
        public ProductGroup ProductGroup { get; set; } = new();
        public List<Request> Requests { get; set; } = []; // Заявки
        #endregion

        #region Id ссылок
        public int ProductGroupId { get; set; } // На каком сырье создаётся
        #endregion
        #endregion

        #region Свойства
        public ProductGroup? GetProductGroup => context.ProductsGroup.AsNoTracking().Where(pg => pg.Id == ProductGroupId).FirstOrDefault();
        #endregion

        #region Методы
        
        #endregion
    }
}