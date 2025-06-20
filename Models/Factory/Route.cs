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
        public string? Name { get; set; } = ""; // Название объекта
        public List<int> RegionsRoute { get; set; } = []; // послежовательность id, составляющая последовательность участков
        #endregion

        #region Ссылочные
        [NotMapped]
        private readonly DB context = new();

        // Завод
        [ForeignKey(nameof(FactoryId))]
        public Factory Factory { get; set; } = new();
        // Участки данного маршрута
        public List<Region> Regions { get; set; } = [];
        // Плановая работа
        public List<Part> Part { get; set; } = [];
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
        // Проверяет в наличии участки, указанные в последовательности
        public bool IsWorking
        {
            get
            {
                for (int i = 0; i < RegionsRoute.Count; ++i)
                {
                    Region? region = Regions.FirstOrDefault(r => r.Id == RegionsRoute[i]);
                    // Проверка участка на существование
                    if (region == null)
                        return false;

                    // Проверка последнего участка
                    if (i == RegionsRoute.Count - 1)
                    {
                        if (region.RegionsChildrens.Count > 0)
                            return false;
                        else
                            return true;
                    }

                    // Проверка участка на существование связей с последующим участком
                    if (region.RegionsChildrens.FirstOrDefault(r => r.Id == RegionsRoute[i + 1]) == null)
                        return false;
                }
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
        public List<Product> GetAvailableProducts()
        {
            // Сырьё, которое может использовать маршрут
            List<Material> materials = GetAcceptableMaterials();
            if (materials.Count == 0)
                return [];

            // Продукция, которую можно производить, если не считать тех. обработку
            List<Product> allProducts = [];
            foreach (var material in materials)
            {
                allProducts.AddRange([..context.ProductsGroup
                    .AsNoTracking()
                    .Where(p=>p.MaterialId == material.Id)]);
            }

            // Рассмотрим каждую продукцию
            List<int> thisTechnologyProcess = [];
            foreach (var rr in RegionsRoute)
                thisTechnologyProcess.Add(Regions.First(r => r.Id == rr).TypeId);
            List<Product> potecialProducts = allProducts.Where(p=>p.TechnologyProcessing.SequenceEqual(thisTechnologyProcess)).ToList();

            return potecialProducts;
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
        public int GetMaxVolumeCountProduct(int packageId)
        {
            // Находим саму продукцию из БД
            Package product = context.Package
                .AsNoTracking()
                .Where(ps => ps.Id == packageId)
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
        /// Можно ли произвести данную продукцию на этом маршруте
        /// </summary>
        public bool CanProduceProduct(int productId)
        {
            List<Product> list = GetAvailableProducts();
            foreach (var item in list)
                if (item.Id == productId)
                    return true;
            return false;
        }
        /*public bool CanProduceProduct(int packageId)
        {
            var list = GetAvailableProducts();
            int productId = context.Package.AsNoTracking().First(p => p.Id == productId).ProductId;
            foreach (var item in list)
                if (item.Id == productId)
                    return true;
            return false;
        }*/

        /// <summary>
        /// Сколько минут требуется маршруту, чтобы обработать все партии до указанного участка (включительно).
        /// Пересечение с другими маршрутами и выполняющиеся сейчас партии не учитываются.
        /// Если участок в маршруте отсутствует, вернётся false, а Time будет равен общему времени выполнения всех планов, в ином случае - наоборот.
        /// </summary>
        public bool GetMinutesToCompletePart(int regionId, out double Time)
        {
            Time = 0;
            foreach (var regId in RegionsRoute) // Прошерстим участки
            {
                List<Part> plans = [.. Part.Where(p => p.RegionId == regId)];
                foreach (var p in plans)
                    Time += Regions[Regions.FindIndex(r => r.Id == regId)].GetTime(p);

                if (regId == regionId)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Время (в минутах), требуемое для завершения данной партии.
        /// Другие партии не берутся в расчёт.
        /// </summary>
        public double GetTimeToCompletePart(Part part)
        {
            double Time = 0;
            foreach (var region in Regions)
            {
                double time = region.GetTime(part);
                if (double.IsInfinity(time))
                    return double.PositiveInfinity;
                Time += time;
            }
            return Time;
        }

        /// <summary>
        /// Время (в минутах), требуемое для завершения обработки всех партий с учётом пересечений.
        /// </summary>
        public double GetTimeToCompleteFullPlan(bool ConsiderDowntime = true)
        {
            if (!IsWorking)
                throw new Exception("Маршрут не нашёл все необходимые участки");
            if (!ConsiderDowntime)
                return Regions.Select(r => r.GetTime(false)).Sum();

            double Time = 0;
            foreach (var regionId in RegionsRoute)
            {
                // Обычный подсчёт
                Region region = Regions.First(r=>r.Id == regionId);
                Time += region.GetTime(false);

                // Подсчёт простоев
                if (region.Downtime != null)
                {
                    if (region.Downtime!.DowntimeFinish == null)
                        return double.PositiveInfinity;

                    // Ищем время окончания производства при текущих обстоятельствах
                    // и проверяем, не попадаем ли мы за это время под начало простоя текущего участка.
                    // Если не попадаем, то не учитываем время простоя этого участка.
                    DateTime dtFactFinish = DateTime.UtcNow.AddMinutes(Time);
                    if (dtFactFinish < region.Downtime!.DowntimeStart)
                        continue;

                    // Если этот простой неизвестно когда закончится, то возвращаем бесконечность;
                    // Если этот простой уже идёт, считаем через сколько он закончится;
                    // В остальных случаях берём продолжительность простоя.
                    if (region.Downtime!.IsDowntimeNow)
                        Time += region.Downtime!.DowntimeFinish!.Value.Subtract(dtFactFinish).TotalMinutes;
                    else
                        Time += region.Downtime!.DowntimeDuration;
                }
            }

            // Вывод результата
            return Time;
        }

        /// <summary>
        /// Время (в минутах), требуемое для завершения обработки всех партий с учётом пересечений и приоритета.
        /// </summary>
        public double GetTimeToCompleteFullPlan(DateTime priority, bool ConsiderDowntime = true)
        {
            return Regions.Select(r => r.GetTime(priority, ConsiderDowntime)).Sum();
        }
        /*public double GetTimeToCompleteFullPlan(DateTime priority, bool ConsiderDowntime = true)
        {
            if (!IsWorking)
                throw new Exception("Маршрут не нашёл все необходимые участки");
            else if (!ConsiderDowntime)
                return Regions.Select(r => r.GetTime(priority, false)).Sum();

            double Time = 0;
            foreach (var regionId in RegionsRoute)
            {
                // Обычный подсчёт
                Region region = Regions.First(r => r.Id == regionId);
                Time += region.GetTime(priority, false);

                // Подсчёт простоев
                if (region.Downtime != null)
                {
                    // Ищем время окончания производства при текущих обстоятельствах
                    // и проверяем, не попадаем ли мы за это время под начало простоя текущего участка.
                    // Если не попадаем, то не учитываем время простоя этого участка.
                    DateTime dtFactFinish = DateTime.UtcNow.AddMinutes(Time);
                    if (dtFactFinish < region.Downtime.DowntimeStart)
                        continue;

                    // Если этот простой неизвестно когда закончится, то возвращаем бесконечность;
                    // Если этот простой уже идёт, считаем через сколько он закончится;
                    // В остальных случаях берём продолжительность простоя.
                    if (region.Downtime.DowntimeFinish == null)
                        return double.PositiveInfinity;
                    else if (region.Downtime.IsDowntimeNow)
                        Time += region.Downtime.DowntimeFinish!.Value.Subtract(dtFactFinish).TotalMinutes;
                    else
                        Time += region.Downtime.DowntimeDuration;
                }
            }

            // Вывод результата
            return Time;
        }*/


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
        /// Была ли пройдена партия по данному участку.
        /// </summary>
        /// <returns>
        /// -1, если не была пройдёна;
        /// 0, если находится на данном участке;
        /// 1, если прошла данный участок;
        /// null, если партия или участок не были найдены
        /// </returns>
        public int? PlanWasCompletedInRegion(int planId, int regionId)
        {
            // Сначала проверка, что план и участок имеются у данного маршрута
            Part? plan = Part.FirstOrDefault(p => p.Id == planId);
            if (plan == null || Regions.FirstOrDefault(r => r.Id == regionId) == null)
                return null;
            else if (plan.Status == PartStatus.DONE)
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
                .Include(r => r.Part)
                .FirstOrDefault();
            if (route == null)
                return false;

            route.Part = Part;
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
        public bool Start(Part plan, bool hardStart = false)
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
            int planIndex = Part.FindIndex(p => p.Id == plan.Id);
            if (planIndex == -1)
            {
                if (hardStart)
                {
                    plan.Route = this;
                    plan.FactoryId = FactoryId;
                    Part.Add(plan);
                    planIndex = Part.Count - 1;
                }
                else
                    return false;
            }

            // Запуск
            Regions[0].SetPart(ref plan); // КИРИЛЛ, УБЕДИСЬ ЧТО В СПИСКЕ ЗНАЧЕНИЕ IsFabrication ТОЖЕ ИЗМЕНИЛОСЬ
            return plan.Status == PartStatus.PRODUCTION;
        }

        /// <summary>
        /// Уведомление маршрута о том, что статус одного из участков был обновлён
        /// </summary>
        /// <param name="regionId">id участка, который был обновлён</param>
        public void RegionUpdateStatus(int regionId, RegionStatus? regionStatus = null)
        {
            // Проверка
            int regionIndex = Regions.FindIndex(r => r.Id == regionId);
            if (regionIndex == -1 || Regions[regionIndex].Part?.Status == PartStatus.STOP)
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
                    Part? earlyPlan = Regions[regionIndex].FindEarlyPlan();
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
                        Part plan = Part.First(p => p.Id == earlyPlan.Id);
                        Regions[regionIndex].SetPart(ref plan);
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
                        Regions[regionIndex].AddWorkload(Regions[regionIndex].Part!.Size, out remain);
                        return;
                    }

                    // В ином случае надо выгрузить продукцию из предыдущего участка
                    previousRegionIndex = Regions.FindIndex(r => r.Id == RegionsRoute[RegionsRoute.FindIndex(match => match == Regions[regionIndex].Id) - 1]);
                    if (Regions[previousRegionIndex].Status == RegionStatus.AWAIT_UNLOADING
                        && Regions[regionIndex].Workload == 0)
                    {
                        Part part = Regions[previousRegionIndex].Part!;
                        if (Regions[regionIndex].SetPart(ref part))
                        {
                            if (Regions[regionIndex].Status != RegionStatus.AWAIT_DOWNLOAD)
                                return;
                            Regions[regionIndex].AddWorkload(Regions[previousRegionIndex].Workload, out remain);
                            if (remain == 0)
                                Regions[previousRegionIndex].SubWorkload(Regions[previousRegionIndex].Workload);
                            else
                                throw new Exception("Необходимо расщепление плана");
                        }
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
                        Factory.CompletePlan(Regions[regionIndex].Part!.Id);
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

                        Part p = Regions[regionIndex].Part!;
                        if (!Regions[nextRegionIndex].SetPart(ref p))
                        {
                            if (Regions[nextRegionIndex].Status != RegionStatus.AWAIT_DOWNLOAD)
                                return;
                            Regions[nextRegionIndex].AddWorkload(Regions[regionIndex].Workload, out remain);
                            if (remain == 0)
                                Regions[regionIndex].SubWorkload(Regions[regionIndex].Workload);
                            else
                                throw new Exception("Необходимо дробление плана");
                        }
                    }
                    else if (Regions[nextRegionIndex].Status == RegionStatus.AWAIT_DOWNLOAD
                        && Regions[regionIndex].Workload > Regions[nextRegionIndex].GetVolumeSizeMaterial(Regions[regionIndex].MaterialOptionNowId!.Value))
                        throw new Exception($"Следующий участок не может принять слишком огромную партию ({Regions[regionIndex].Id} - {Regions[nextRegionIndex].Id})");
                    else if (Regions[nextRegionIndex].Status == RegionStatus.FREE)
                    {
                        /*Part part = Part.First(p => p.Id == Regions[regionIndex].Part!.Id);
                        bool success = Regions[nextRegionIndex].SetPart(ref part);
                        if (success && Regions[regionIndex].Workload == 0)
                            RegionUpdateStatus(Regions[regionIndex].Id);*/
                        Part part = Regions[regionIndex].Part!;
                        if (Regions[nextRegionIndex].SetPart(ref part))
                        {
                            if (Regions[nextRegionIndex].Status != RegionStatus.AWAIT_DOWNLOAD)
                                return;
                            Regions[nextRegionIndex].AddWorkload(Regions[regionIndex].Workload, out remain);
                            if (remain == 0)
                                Regions[regionIndex].SubWorkload(Regions[regionIndex].Workload);
                        }
                    }

                    // Завершение работы метода
                    return;
                case RegionStatus.DOWNTIME:
                    // Обнуляем план, проходивший по данному участку
                    if (Regions[regionIndex].Part != null)
                    {
                        int planIndex = Part.FindIndex(p => p.Id == Regions[regionIndex].Part!.Id);
                        this.Part[planIndex].Region = null;
                        this.Part[planIndex].Status = PartStatus.QUEUE;
                        Regions[regionIndex].Part = null;
                    }

                    // Запрашиваем ответ, что делать с остальными планами
                    foreach (var p in Part)
                    {
                        // Сначала узнаём, на каком участке он находится (если может закончить выполнение - так оно и будет)
                        int regionIndexWithPlan = Regions.FindIndex(r => r.Id == p.RegionId);
                        if (regionIndexWithPlan > regionIndex)
                            continue;

                        // Всем остальным (почти) надо обновить статус и решить, что с ними делать
                        if (p.Status != PartStatus.AWAIT_CONFIRMATION)
                            p.Status = PartStatus.STOP;
                    }

                    return;
                case RegionStatus.DOWNTIME_FINISH:
                    // Обновляем статус всех планов
                    foreach (var p in Part)
                        if (p.Status == PartStatus.STOP || p.Status == PartStatus.PAUSE)
                        {
                            if (p.RegionId == null)
                                p.Status = PartStatus.QUEUE;
                            else
                                p.Status = PartStatus.PRODUCTION;
                        }
                    return;
                default: return;
            }
        }
        #endregion

        #region Статические и переопределяющие
        public override string ToString()
        {
            if (Name != null && Name != "")
                return Name;
            return GetContent;
        }
        #endregion
        #endregion
    }
}