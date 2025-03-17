using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diamond.Models
{
    public class Route : FactoryObject
    {
        #region Поля
        #region Обычные
        #endregion

        #region Ссылочные
        [NotMapped]
        private readonly DB context = new();

        // Завод
        [ForeignKey("FactoryId")]
        public Factory Factory { get; set; } = new();
        // Участки данного маршрута
        public List<Region> Regions { get; set; } = [];
        // Заказы на данном маршруте
        public List<Request>? Requests { get; set; }
        #endregion

        #region Id ссылок
        public int FactoryId { get; set; }
        #endregion
        #endregion

        #region Свойства
        public string GetContent
        {
            get
            {
                string content = "";
                try
                {
                    if (Regions.Count == 0)
                        Regions = context.Routes.AsNoTracking().Where(r => r.Id == Id).Include(r => r.Regions).First().Regions;
                    foreach (var region in Regions)
                    {
                        content += $"{region.Name} -> ";
                    }
                    content = content.Remove(content.Length - 4);
                }
                catch { }
                return content;
            }
        }
        #endregion

        #region Методы
        // Список, на каком сырье может работать данный маршрут
        public List<Material> GetAcceptableMaterials()
        {
            // Проверка
            if (Regions.Count == 0)
            {
                try
                {
                    Regions = context.Routes
                        .AsNoTracking()
                        .Where(r => r.Id == Id)
                        .Include(r => r.Regions)
                        .First()
                        .Regions;
                    if (Regions.Count == 0)
                        return [];
                }
                catch { return []; }
            }

            // Начальный список берём из первого участка
            List<Material> materials = Regions[0].GetMaterials();
            for (int i = 1; i < Regions.Count && materials.Count > 0; ++i) // Просмотрим каждый участок, начиная со второго, т.к. сырьё первого уже известно
            {
                List<Material> m1 = []; // Сюда будут записаны все повторы
                foreach (var am in materials) // Просмотр уже имеющегося списка сырья
                    foreach (var material in Regions[i].GetMaterials()) // Просмотр доступного сырья
                        if (am.Id == material.Id)
                            m1.Add(material);
                materials = m1; // Обновляем первичный список
            }

            return materials;
        }

        // Список, какую продукцию на нём можно производить
        public List<ProductGroup> GetAvailableProducts()
        {
            List<Material> materials = GetAcceptableMaterials();
            if (materials.Count == 0)
                return [];

            List<ProductGroup> products = [];
            foreach (var material in materials)
            {
                products.AddRange([..context.ProductsGroup
                    .AsNoTracking()
                    .Where(p=>p.MaterialId == material.Id)]);
            }

            return products;
        }

        // Список, какое сырьё можно производить и с какой скоростью
        public List<MaterialForRegion> GetMaxPowerOnMaterial()
        {
            List<Material> materials = GetAcceptableMaterials();
            List<Region> ThisRegions;
            if (Regions.Count == 0)
            {
                try
                {
                    ThisRegions = context.Routes
                        .AsNoTracking()
                        .Where(r => r.Id == Id)
                        .Include(r => r.Regions).ThenInclude(r => r.Materials)
                        .First()
                        .Regions;
                }
                catch { return []; }
            }
            else
                ThisRegions = [.. Regions];

            List<MaterialForRegion> result = [];
            foreach (var material in materials)
            {
                result.Add(new MaterialForRegion() { MaterialId = material.Id, Power = int.MaxValue });
                foreach (var region in ThisRegions)
                    foreach (var regionMaterial in region.Materials)
                        if (regionMaterial.MaterialId == material.Id)
                        {
                            if (result[^1].Power > regionMaterial.Power)
                                result[^1] = regionMaterial;
                            break;
                        }
            }

            return result;
        }
        #endregion
    }
}