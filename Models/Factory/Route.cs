using Diamond.Database;
using Diamond.Models.Materials;
using Diamond.Models.Products;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diamond.Models.Factory
{
    public class Route
    {
        #region Поля
        #region Обычные
        public int Id { get; set; } // Номер объекта в БД

        [Required(ErrorMessage = "Название обязательно")]
        [StringLength(50, ErrorMessage = "Название должно быть не длиннее 50 символов")]
        public string Name { get; set; } = ""; // Название объекта
        public List<int> RegionsRoute { get; set; } = []; // послежовательность id, составляющая последовательность участков
        #endregion

        #region Ссылочные
        [NotMapped]
        private readonly DB context = new();

        // Завод
        [ForeignKey("OrderId")]
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
                if (Regions.Count == 0 || Regions[0].Status != RegionStatus.FREE)
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
                    if (j < RegionsRoute.Count)
                        break;
                    Region r = Regions.Where(r => r.Id == RegionsRoute[j]).First();
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
        public int GetMaxVolumeSizeMaterial(int materialId)
        {
            int volume = int.MaxValue;
            foreach (var r in Regions)
            {
                int regionVolume = r.GetVolumeSizeMaterial(materialId);
                if (regionVolume < volume)
                    volume = regionVolume;
            }
            return volume;
        }

        /// <summary>
        /// Максимальный объём продукции (в пачках), которое маршрут может производить в одной партии
        /// </summary>
        public int GetMaxVolumeCountProduct(int productId)
        {
            // Находим саму продукцию из БД
            ProductSpecific product = context.ProductsSpecific
                .AsNoTracking()
                .Where(ps => ps.Id == productId)
                .Include(ps => ps.ProductGroup)
                .First();
            
            // Узнаём максимальную мощность по требуемому сырью
            int MaxVolumeOnMaterial = GetMaxVolumeSizeMaterial(product.ProductGroup.MaterialId);
            
            // Выводим результат
            if (MaxVolumeOnMaterial > 0)
                return MaxVolumeOnMaterial / product.Size;
            else
                return 0;
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

        /// <summary>
        /// Время (в минутах), требуемое для завершения данного плана.
        /// Другие планы не берутся в расчёт.
        /// </summary>
        public double MinutesToCompletePlan(Plan plan)
        {
            double Time = 0;
            foreach (var region in Regions)
            {
                double time = region.GetTime(plan);
                if (double.IsInfinity(time))
                    return double.PositiveInfinity;
                Time += time;
            }
            return Time;
        }

        /// <summary>
        /// Есть ли на маршруте простаивающий участок
        /// </summary>
        public bool IsHaveDowntimeRegion()
        {
            foreach (Region r in Regions)
                if (r.Downtime != null)
                    return true;
            return false;
        }

        /// <summary>
        /// Количество простоев у маршрута
        /// </summary>
        public int GetDowntimeCount()
        {
            int Count = 0;
            foreach (Region r in Regions)
                if (r.Downtime != null)
                    ++Count;
            return Count;
        }

        /// <summary>
        /// Был ли пройден план по данному участку.
        /// </summary>
        /// <returns>
        /// -1, если не был пройдён;
        /// 0, если находится на данном участке;
        /// 1, если прошёл данный участок;
        /// null, если план или участок не были найдены
        /// </returns>
        public int? PlanWasCompletedInRegion(int planId, int regionId)
        {
            // Сначала проверка, что план и участок имеются у данного маршрута
            Plan? plan = Plan.FirstOrDefault(p => p.Id == planId);
            if (plan == null || Regions.FirstOrDefault(r => r.Id == regionId) == null)
                return null;
            else if (plan.Status == PlanStatus.DONE)
                return 1;

            // Находим индексы
            int itRegionIndex = RegionsRoute.FindIndex(rr => rr == regionId); // Интересующий участок
            int factRegionIndex = RegionsRoute.FindIndex(rr => rr == plan.RegionId); // Фактический участок

            // Проверяем
            if (factRegionIndex < itRegionIndex)
                return -1;
            else if (factRegionIndex == itRegionIndex)
                return 0;
            else if (factRegionIndex > itRegionIndex)
                return 1;
            else
                return null;
        }

        /// <summary>
        /// Время (в минутах), оставшееся до полного конца всех простоев на маршруте
        /// </summary>
        public double GetTimeToEndDowntime()
        {
            DateTime? dtEnd = null;
            foreach (Region r in Regions)
                if (r.Downtime != null)
                {
                    // Если хотя бы на одном участке не указано время окончания простоя, то не имеет смысла продолжать поиски
                    if (r.Downtime.DowntimeFinish == null)
                        return double.PositiveInfinity;

                    if (dtEnd == null || r.Downtime.DowntimeFinish > dtEnd)
                        dtEnd = r.Downtime.DowntimeFinish;
                }

            // Если dtEnd ничему не равен, то значит у маршута нет простоев. В ином случае надо посчитать, сколько осталось.
            if (dtEnd == null)
                return 0;
            return dtEnd!.Value.ToUniversalTime().Subtract(DateTime.UtcNow).TotalMinutes;
        }
        #endregion

        #region Взаимодействие с БД
        // Сохранить план
        public bool SavePlan()
        {
            Route? route = context.Routes
                .Where(r => r.Id == Id)
                .Include(r => r.Plan)
                .FirstOrDefault();
            if (route == null)
                return false;

            route.Plan = Plan;
            context.SaveChanges();
            return true;
        }
        #endregion

        #region Работа с данным маршрутом
        /// <summary>
        /// Начать новый процесс
        /// </summary>
        /// <param name="plan">План, который необходимо начать выполнять</param>
        /// <param name="hardStart">Запустить даже если этого плана нет в списке у этого маршрута 
        /// (если true, то добавится в список этого маршрута добавится данный план, в ином случае вернётся false)</param>
        /// <returns>Возращает true, если запуск был успешен</returns>
        public bool Start(Plan plan, bool hardStart = false)
        {
            // Проверка на существование участков у данного маршрута, и что первый участок свободен
            if (Regions.Count == 0)
            {
                Regions = [.. context.Routes
                    .Where(r => r.Id == Id)
                    .First()
                    .Regions];
                if (Regions.Count == 0)
                    return false;
            }
            if (Regions[0].Status != RegionStatus.FREE)
                return false;

            // Нахождение данного плана в имеющемся списке
            int planIndex = Plan.FindIndex(p => p.Id == plan.Id);
            if (planIndex == -1)
            {
                if (hardStart)
                {
                    plan.Route = this;
                    plan.FactoryId = FactoryId;
                    Plan.Add(plan);
                    planIndex = Plan.Count - 1;
                }
                else
                    return false;
            }

            // Запуск
            Regions[0].SetPlan(ref plan); // КИРИЛЛ, УБЕДИСЬ ЧТО В СПИСКЕ ЗНАЧЕНИЕ IsFabrication ТОЖЕ ИЗМЕНИЛОСЬ
            return plan.Status == PlanStatus.PRODUCTION;
        }

        /// <summary>
        /// Уведомление маршрута о том, что статус одного из участков был обновлён
        /// </summary>
        /// <param name="regionId">id участка, который был обновлён</param>
        public void RegionUpdateStatus(int regionId, RegionStatus? regionStatus = null)
        {
            // Проверка
            int regionIndex = Regions.FindIndex(r => r.Id == regionId);
            if (regionIndex == -1)
                return;
            if (regionStatus.HasValue)
                Regions[regionIndex].Status = regionStatus!.Value;
            int previousRegionIndex, remain;

            // Процесс
            switch (Regions[regionIndex].Status)
            {
                case RegionStatus.OFF: Regions[regionIndex].Launch(); return;
                case RegionStatus.FREE:
                    // Первому участку надо посмотреть новое задание, других надо подготовить к принятию плана
                    if (Regions[regionIndex].RegionsParents.Count == 0)
                    {
                        Factory.StartPlan();
                        return;
                    }

                    // Сначала проверка на то, что участок не просто так уведомил этот маршрут о своём статусе
                    Plan? earlyPlan = Regions[regionIndex].FindEarlyPlan();
                    if (earlyPlan == null)
                        return;
                    // Теперь проверка, что данному маршруту принадлежит участок, который надо сдвинуть с мёртвой точки
                    int earlyRegionIndex = Regions.FindIndex(r => r.Id == earlyPlan.Region!.Id);
                    if (earlyRegionIndex == -1)
                        throw new Exception($"Участок ({regionId}) обратился не к тому маршруту ({Id})");

                    // Теперь, если участок из плана готов разгружаться в данный, то устанавливаем новый план
                    int regionRouteIndex = RegionsRoute.FindIndex(match => match == Regions[regionIndex].Id);
                    previousRegionIndex = Regions.FindIndex(r => r.Id == RegionsRoute[regionRouteIndex - 1]);
                    if (earlyRegionIndex == previousRegionIndex && Regions[earlyRegionIndex].Status == RegionStatus.AWAIT_UNLOADING)
                    {
                        Plan plan = Plan.First(p => p.Id == earlyPlan.Id);
                        Regions[regionIndex].SetPlan(ref plan);
                    }
                    else if (Regions[regionIndex].MaterialOptionNowId != earlyPlan.MaterialId)
                        Regions[regionIndex].StartReadjustment(earlyPlan.MaterialId);

                    return;
                case RegionStatus.FREE_READJUSTMENT:
                case RegionStatus.READJUSTMENT:
                    Regions[regionIndex].StartReadjustment();
                    return;
                case RegionStatus.AWAIT_DOWNLOAD:
                    // Загрузка для первого участка
                    if (Regions[regionIndex].RegionsParents.Count == 0)
                    {
                        Regions[regionIndex].AddWorkload(Regions[regionIndex].Plan!.Size, out remain);
                        return;
                    }

                    // В ином случае надо выгрузить продукцию из предыдущего участка
                    previousRegionIndex = Regions.FindIndex(r => r.Id == RegionsRoute[RegionsRoute.FindIndex(match => match == Regions[regionIndex].Id) - 1]);
                    if (Regions[previousRegionIndex].Status == RegionStatus.AWAIT_UNLOADING
                        && Regions[regionIndex].Workload == 0)
                    {
                        Regions[regionIndex].AddWorkload(Regions[previousRegionIndex].Workload, out remain);
                        if (remain == 0)
                            Regions[previousRegionIndex].SubWorkload(Regions[regionIndex].Workload);
                        else
                            throw new Exception("Необходимо расщепление плана");
                    }

                    // Завершение работы метода
                    return;
                case RegionStatus.AWAITING_LAUNCH:
                    Regions[regionIndex].Start();
                    return;
                case RegionStatus.IN_WORKING: return;
                case RegionStatus.AWAIT_UNLOADING:
                    // Если это был последний участок, то выполнение плана завершено
                    if (Regions[regionIndex].RegionsChildrens.Count == 0)
                    {
                        Factory.CompletePlan(Regions[regionIndex].Plan!.Id);
                        Regions[regionIndex].SubWorkload(Regions[regionIndex].Workload);
                        return;
                    }

                    // В ином случае надо загрузить продукцию в следующий участок
                    int nextRegionIndex = Regions.FindIndex(r => r.Id == RegionsRoute[RegionsRoute.FindIndex(match => match == Regions[regionIndex].Id) + 1]);
                    if (Regions[nextRegionIndex].Status == RegionStatus.AWAIT_DOWNLOAD
                        && Regions[regionIndex].Workload <= Regions[nextRegionIndex].GetVolumeSizeMaterial(Regions[regionIndex].MaterialOptionNowId!.Value))
                    {
                        if (Regions[nextRegionIndex].Workload > 0)
                            throw new Exception();
                        Regions[nextRegionIndex].AddWorkload(Regions[regionIndex].Workload, out remain);
                        if (remain == 0)
                        {
                            Regions[regionIndex].SubWorkload(Regions[nextRegionIndex].Workload);
                            RegionUpdateStatus(Regions[regionIndex].Id, RegionStatus.FREE);
                        }
                        else
                            throw new Exception("Необходимо расщепление плана");
                    }
                    else if (Regions[regionIndex].Workload > Regions[nextRegionIndex].GetVolumeSizeMaterial(Regions[regionIndex].MaterialOptionNowId!.Value))
                        throw new Exception($"Следующий участок не может принять слишком огромную партию ({Regions[regionIndex].Id} - {Regions[nextRegionIndex].Id})");
                    else if (Regions[nextRegionIndex].Status == RegionStatus.FREE)
                    {
                        Plan plan = Plan.First(p => p.Id == Regions[regionIndex].Plan!.Id);
                        bool success = Regions[nextRegionIndex].SetPlan(ref plan);
                        if (success && Regions[regionIndex].Workload == 0)
                            RegionUpdateStatus(Regions[regionIndex].Id);
                    }

                    // Завершение работы метода
                    return;
                case RegionStatus.DOWNTIME:
                    // Обнуляем план, проходивший по данному участку
                    if (Regions[regionIndex].Plan != null)
                    {
                        int planIndex = Plan.FindIndex(p => p.Id == Regions[regionIndex].Plan!.Id);
                        this.Plan[planIndex].Region = null;
                        this.Plan[planIndex].Status = PlanStatus.QUEUE;
                        Regions[regionIndex].Plan = null;
                    }

                    // Запрашиваем ответ, что делать с остальными планами
                    foreach (var p in Plan)
                    {
                        // Сначала узнаём, на каком участке он находится (если может закончить выполнение - так оно и будет)
                        int regionIndexWithPlan = Regions.FindIndex(r => r.Id == p.RegionId);
                        if (regionIndexWithPlan > regionIndex)
                            continue;

                        // Остальным надо обновить статус и решить, что с ними делать
                        p.Status = PlanStatus.STOP;
                    }

                    return;
                case RegionStatus.DOWNTIME_FINISH:
                    // Обновляем статус всех планов
                    foreach (var p in Plan)
                        if (p.Status == PlanStatus.STOP || p.Status == PlanStatus.PAUSE)
                        {
                            if (p.RegionId == null)
                                p.Status = PlanStatus.QUEUE;
                            else
                                p.Status = PlanStatus.PRODUCTION;
                        }
                    return;
                default: return;
            }
        }
        #endregion

        #region Статические и переопределяющие
        public override string ToString()
        {
            if (Name != "")
                return Name;
            return GetContent;
        }
        #endregion
        #endregion
    }
}