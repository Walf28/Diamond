using Diamond.Database;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diamond.Models.Factory
{
    public class Plan
    {
        #region Поля
        #region Обычные
        public int Id { get; set; }
        public int Size { get; set; } // Сколько производим
        public DateTime ComingSoon { get; set; } // Ближайшее время, за которое желательно произвести продукцию
        public bool IsFabricating { get; set; } // Начался ли процесс производства
        #endregion

        #region Ссылки
        [NotMapped]
        private readonly DB context = new();
        public Factory Factory { get; set; } = null!; // Какому заводу принадлежит
        [ForeignKey("RouteId")]
        public Route? Route { get; set; } = null!; // На каком маршруте выполняется процесс
        [ForeignKey("RegionId")]
        public Region? Region { get; set; } = null!; // На каком участке на данный момент находится
        [ForeignKey("ProductId")]
        public required ProductSpecific Product { get; set; } = null!; // На каком участке на данный момент находится
        #endregion

        #region Id ссылок
        public int? RouteId { get; set; } // Id маршрута
        public int? RegionId { get; set; } // На каком участке на данный момент находится
        public required int ProductId { get; set; } // Что производим
        #endregion
        #endregion

        #region Свойства
        public Material? GetUsingMaterial
        {
            get
            {
                ProductSpecific? ps = context.ProductsSpecific
                    .AsNoTracking()
                    .Where(ps => ps.Id == ProductId)
                    .Include(ps => ps.ProductGroup).ThenInclude(pg => pg.Material)
                    .FirstOrDefault();
                if (ps != null)
                    return ps.ProductGroup.Material;
                return null;
            }
        }
        #endregion
    }
}