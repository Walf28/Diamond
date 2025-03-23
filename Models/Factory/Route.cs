using Diamond.Database;
using Diamond.Models.Materials;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diamond.Models.Factory
{
    public class Route : FactoryObject
    {
        #region Поля
        #region Обычные
        public List<int> RegionsRoute { get; set; } = []; // послежовательность id, составляющая последовательность участков
        #endregion

        #region Ссылочные
        [NotMapped]
        private readonly DB context = new();

        // Завод
        [ForeignKey("FactoryId")]
        public Factory Factory { get; set; } = new();
        // Участки данного маршрута
        public List<Region> Regions { get; set; } = [];
        // Плановая работа
        public List<Plan> Plan { get; set; } = [];
        #endregion

        #region Id ссылок
        public int FactoryId { get; set; }
        #endregion
        #endregion

        #region Свойства
        // Последовательность участков в пользовательском виде
        public string GetContent
        {
            get
            {
                string content = "";
                try
                {
                    List<Region> regions;
                    if (Regions.Count > 0)
                        regions = [.. Regions];
                    else
                        regions = context.Routes.AsNoTracking().Where(r => r.Id == Id).Include(r => r.Regions).First().Regions;

                    if (regions.Count > 0)
                    {
                        foreach (var regionId in RegionsRoute)
                        {
                            content += $"{regions.Where(r => r.Id == regionId).First().Name} -> ";
                        }
                        content = content.Remove(content.Length - 4);
                    }
                }
                catch 
                {
                    content = "ERROR";
                }
                return content;
            }
        }
        // Готов ли маршрут к началу производства следующей продукции
        public bool ReadyToContinue
        {
            get
            {
                if (Regions.Count == 0 || Regions[0].Status != RegionStatus.READY_TO_WORK)
                    return false;
                else
                    return true;
            }
        }
        #endregion

        #region Методы
        #region Информационные
        /// <summary>
        /// Список, на каком сырье может работать данный маршрут
        /// </summary>
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

            // Начальный список берём из "гарантированного" участка
            List<Material> materials = Regions[0].GetMaterials();
            for (int i = 1; i < Regions.Count && materials.Count > 0; ++i) // Прошерстим все остальные участки
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

        /// <summary>
        /// Список, какую продукцию на нём можно производить
        /// </summary>
        public List<ProductGroup> GetAvailableProducts()
        {
            // Сырьё, которое может использовать маршрут
            List<Material> materials = GetAcceptableMaterials();
            if (materials.Count == 0)
                return [];

            // Продукция, которую можно производить, если не считать тех. обработку
            List<ProductGroup> products = [];
            foreach (var material in materials)
            {
                products.AddRange([..context.ProductsGroup
                    .AsNoTracking()
                    .Where(p=>p.MaterialId == material.Id)]);
            }

            // Теперь учитываем процесс
            for (int i = 0; i < products.Count; ++i)
            {
                List<Technology> tech = [.. products[i].TechnologyProcessing];
                for (int j = 0; j < tech.Count; ++j)
                {
                    Region r = Regions.Where(r=>r.Id == RegionsRoute[j]).First();
                    if (r == null || r.Type != tech[j] || (j == tech.Count - 1 && r.IsRegionsChildrens))
                    {
                        products.RemoveAt(i);
                        --i;
                        break;
                    }
                }
            }

            return products;
        }

        /// <summary>
        /// Список, какое сырьё можно производить и с какой скоростью
        /// </summary>
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

        /// <summary>
        /// Максимальный объём (в граммах) сырья, которое маршрут может обработать в одной партии
        /// </summary>
        public int GetMaxVolume(int materialId)
        {
            int volume = int.MaxValue;
            foreach (var r in Regions)
            {
                int regionVolume = r.GetVolume(materialId);
                if (regionVolume < volume)
                {
                    volume = regionVolume;
                    break;
                }
            }
            return (int)volume;
        }
        
        /// <summary>
        /// Время (в мин.), за которое маршрут обработает данную партию
        /// </summary>
        public double GetTime(int materialId, int Size)
        {
            double time = 0;
            foreach (var r in Regions)
            {
                double regionTime = r.GetTime(materialId, Size);
                if (regionTime == double.PositiveInfinity)
                    return regionTime;
                time += regionTime;
            }
            return time;
        }

        /// <summary>
        /// Можно ли произвести данную продукцию на этом маршруте
        /// </summary>
        public bool CanProduceProduct(int productId)
        {
            var list = GetAvailableProducts();
            foreach (var item in list)
                if (item.Id == productId)
                    return true;
            return false;
        }

        /// <summary>
        /// Сколько минут требуется маршруту, чтобы завершить весь план, который у него имеется.
        /// Пересечение с другими маршрутами и выполняющиеся сейчас планы не учитываются.
        /// </summary>
        public double MinutesToCompletePlan()
        {
            double Time = 0;
            foreach (var p in Plan)
                Time += GetTime(p.GetMaterial!.Id, p.Size);
            return Time;
        }

        /// <summary>
        /// Сколько минут требуется маршруту, чтобы завершить все планы, которые следует выполнить в назначенный срок.
        /// Пересечение с другими маршрутами и выполняющиеся сейчас планы не учитываются.
        /// </summary>
        public double MinutesToCompletePlan(DateTime timeBefore)
        {
            double Time = 0;
            foreach (var p in Plan)
                if (p.ComingSoon < timeBefore)
                    Time += GetTime(p.GetMaterial!.Id, p.Size);
            return Time;
        }

        /// <summary>
        /// Сколько минут требуется маршруту, чтобы завершить все планы до указанного участка (включительно).
        /// Пересечение с другими маршрутами и выполняющиеся сейчас планы не учитываются.
        /// Если участок в маршруте отсутствует, вернётся false, а Time будет равен общему времени выполнения всех планов, в ином случае - наоборот.
        /// </summary>
        public bool MinutesToCompletePlan(int regionId, out double Time)
        {
            Time = 0;
            foreach (var regId in RegionsRoute) // Прошерстим участки
            {
                List<Plan> plans = [.. Plan.Where(p => p.RegionId == regId)];
                foreach (var p in plans)
                    Time += Regions[Regions.FindIndex(r => r.Id == regId)].GetTime(p);

                if (regId == regionId)
                    return true;
            }
            return false;
        }
        #endregion

        #region Взаимодействие с БД
        // Сохранить план
        public bool SavePlan()
        {
            Route? route = context.Routes
                .Where(r=>r.Id == Id)
                .Include(r=>r.Plan)
                .FirstOrDefault();
            if (route == null)
                return false;
            
            route.Plan = Plan;
            context.SaveChanges();
            return true;
        }
        #endregion

        #region Работа с данным участком
        // Начать новый процесс
        public void Start(Plan plan, out bool SaveSuccess)
        {
            if (Regions.Count == 0)
            {
                Regions = [.. context.Routes
                    .Where(r => r.Id == Id)
                    .First()
                    .Regions];
                if (Regions.Count == 0)
                    throw new Exception("Участки не найдены");
                else if (Regions[0].Status != RegionStatus.READY_TO_WORK)
                    throw new Exception("Участок не готов принять следующую продукцию");
            }

            Regions[0].SetPlan(plan, out SaveSuccess);
            
            if(Plan.Where(p=>p.Id == plan.Id).FirstOrDefault() == null)
                Plan.Add(context.Plans.Where(p=>p.Id == Id).First());

            if (SaveSuccess)
                SaveSuccess = SavePlan();
        }
        #endregion
        #endregion
    }
}