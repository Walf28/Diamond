using Diamond.Database;
using Diamond.Models.Materials;
using Diamond.Models.Products;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diamond.Models.Factory
{
    public enum PlanStatus
    {
        [Display(Name = "Ожидает подтверждения")]
        AWAIT_CONFIRMATION,
        [Display(Name = "В очереди")]
        QUEUE,
        [Display(Name = "В производстве")]
        PRODUCTION,
        [Display(Name = "Не может продолжить производство")]
        STOP,
        [Display(Name = "Ожидает возобновления производства")]
        PAUSE,
        [Display(Name = "Выполнен")]
        DONE
    }

    public class Part
    {
        #region Поля
        #region Обычные
        public int Id { get; set; }
        public int Size { get; set; } // Сколько производим
        public DateTime ComingSoon { get; set; } // Ближайшее время, за которое желательно произвести продукцию
        public PlanStatus Status { get; set; } = PlanStatus.AWAIT_CONFIRMATION; // Статус плана
        #endregion

        #region Ссылки
        [NotMapped]
        private readonly DB context = new();
        [ForeignKey("OrderId")]
        public Factory Factory { get; set; } = null!; // Какому заводу принадлежит
        [ForeignKey("RouteId")]
        public Route Route { get; set; } = null!; // На каком маршруте выполняется процесс
        [ForeignKey("RegionId")]
        public Region? Region { get; set; } // На каком участке на данный момент находится
        [ForeignKey("ProductsId")]
        public ProductSpecific Product { get; set; } = null!; // Что необходимо произвести
        [ForeignKey("MaterialId")]
        public Material Material { get; set; } = null!; // На чём необходимо произвести
        #endregion

        #region Id ссылок
        public int FactoryId { get; set; } // Id завода
        public int RouteId { get; set; } // Id маршрута
        public int? RegionId { get; set; } // На каком участке на данный момент находится
        public int ProductId { get; set; } // Что производим
        public int MaterialId { get; set; } // Id сырья
        #endregion
        #endregion

        #region Свойства
        public Material? GetMaterial
        {
            get
            {
                ProductSpecific? ps = context.ProductsSpecific
                    .AsNoTracking()
                    .Where(ps => ps.Id == ProductId)
                    .Include(ps => ps.ProductGroup).ThenInclude(pg => pg.Material)
                    .FirstOrDefault();
                if (ps == null)
                    return null;
                return ps.ProductGroup.Material;
            }
        }
        public int GetCountProduct => Size / context.ProductsSpecific.AsNoTracking().First(ps=>ps.Id == ProductId).Size;
        #endregion
    }
}