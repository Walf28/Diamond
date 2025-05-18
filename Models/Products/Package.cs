using Diamond.Database;
using Diamond.Models.Factory;
using Diamond.Models.Materials;
using Diamond.Models.Orders;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diamond.Models.Products
{
    public class Package
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
        public List<OrderPart> OrderParts { get; set; } = []; // Заявки
        [ForeignKey(nameof(ProductId))]
        public Product ProductGroup { get; set; } = new();
        public List<Part> Plans { get; set; } = [];
        public List<ProductWarehouse> ProductWarehouses { get; set; } = [];
        #endregion

        #region Id ссылок
        public int ProductId { get; set; } // На каком сырье создаётся
        #endregion
        #endregion

        #region Свойства
        public Product? GetProductGroup => context.ProductsGroup.AsNoTracking().Where(pg => pg.Id == ProductId).FirstOrDefault();
        public Material? GetMaterial
        {
            get
            {
                var pg = context.ProductsGroup
                    .AsNoTracking()
                    .Where(pg => pg.Id == ProductId)
                    .Include(pg => pg.Material)
                    .FirstOrDefault();
                if (pg == null)
                    return null;
                return pg.Material;
            }
        }
        public string? GetNameAndCount
        {
            get
            {
                try
                {
                    return context.ProductsGroup.AsNoTracking().Where(pg => pg.Id == ProductId).First().Name + $" {Size} гр.";
                }
                catch
                {
                    return "";
                }
            }
        }
        #endregion

        #region Методы
        public override string ToString()
        {
            try
            {
                return context.ProductsGroup.AsNoTracking().Where(pg => pg.Id == ProductId).First().Name + $" {Size} гр.";
            }
            catch
            {
                return "";
            }
        }
        #endregion
    }
}