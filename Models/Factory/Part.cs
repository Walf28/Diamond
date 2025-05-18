using Diamond.Database;
using Diamond.Models.Materials;
using Diamond.Models.Products;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diamond.Models.Factory
{
    public enum PartStatus
    {
        [Display(Name = "Ожидает подтверждения")] // 0
        AWAIT_CONFIRMATION,
        [Display(Name = "В очереди")] // 1
        QUEUE,
        [Display(Name = "В производстве")] // 2
        PRODUCTION,
        [Display(Name = "Не может продолжить производство")] // 3
        STOP,
        [Display(Name = "Ожидает возобновления производства")] // 4
        PAUSE,
        [Display(Name = "Выполнен")] // 5
        DONE
    }

    public class Part
    {
        #region Поля
        #region Обычные
        public int Id { get; set; }
        public int Size { get; set; } // Сколько производим
        public DateTime ComingSoon { get; set; } // Ближайшее время, за которое желательно произвести продукцию
        public PartStatus Status { get; set; } = PartStatus.AWAIT_CONFIRMATION; // Статус плана
        #endregion

        #region Ссылки
        [NotMapped]
        private readonly DB context = new();
        [ForeignKey(nameof(FactoryId))]
        public Factory Factory { get; set; } = null!; // Какому заводу принадлежит
        [ForeignKey(nameof(RouteId))]
        public Route Route { get; set; } = null!; // На каком маршруте выполняется процесс
        [ForeignKey(nameof(RegionId))]
        public Region? Region { get; set; } // На каком участке на данный момент находится
        [ForeignKey(nameof(ProductId))]
        public Package Product { get; set; } = null!; // Что необходимо произвести
        [ForeignKey(nameof(MaterialId))]
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
                Package? ps = context.Package
                    .AsNoTracking()
                    .Where(ps => ps.Id == ProductId)
                    .Include(ps => ps.ProductGroup).ThenInclude(pg => pg.Material)
                    .FirstOrDefault();
                if (ps == null)
                    return null;
                return ps.ProductGroup.Material;
            }
        }
        public int GetCountProduct 
        {
            get
            {
                try
                {
                    return Size / context.Package.AsNoTracking().First(ps => ps.Id == ProductId).Size;
                }
                catch 
                {
                    return 0;
                }
            }
        }
        #endregion
    }
}