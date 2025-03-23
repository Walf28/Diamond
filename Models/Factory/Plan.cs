﻿using Diamond.Database;
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
        public bool IsFabricating { get; set; } = false; // Начался ли процесс производства
        #endregion

        #region Ссылки
        [NotMapped]
        private readonly DB context = new();
        [ForeignKey("FactoryId")]
        public Factory Factory { get; set; } = null!; // Какому заводу принадлежит
        [ForeignKey("RouteId")]
        public Route Route { get; set; } = null!; // На каком маршруте выполняется процесс
        [ForeignKey("RegionId")]
        public Region? Region { get; set; } // На каком участке на данный момент находится
        [ForeignKey("ProductId")]
        public ProductSpecific Product { get; set; } = null!; // Что необходимо произвести
        #endregion

        #region Id ссылок
        public int FactoryId { get; set; } // Id завода
        public int RouteId { get; set; } // Id маршрута
        public int? RegionId { get; set; } // На каком участке на данный момент находится
        public int ProductId { get; set; } // Что производим
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
        #endregion
    }
}