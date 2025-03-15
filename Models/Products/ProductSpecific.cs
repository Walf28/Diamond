using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diamond.Models
{
    public class ProductSpecific
    {
        #region Поля
        #region Обычные
        public int Id { get; set; }
        public int Size { get; set; } // Размер в граммах
        public int Price { get; set; } // Цена
        #endregion

        #region Ссылочные
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