﻿using Diamond.Database;
using Diamond.Models.Materials;
using Diamond.Models.Products;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Timers;

namespace Diamond.Models.Factory
{
    public enum RegionStatus
    {
        [Display(Name = "Выключен")] // 0
        OFF,
        [Display(Name = "Свободен")] // 1
        FREE,
        [Display(Name = "Идёт переналадка свободного участка")] // 2
        FREE_READJUSTMENT,
        [Display(Name = "Идёт переналадка под план")] // 3
        READJUSTMENT,
        [Display(Name = "Ожидает загрузки")] // 4
        AWAIT_DOWNLOAD,
        [Display(Name = "Ожидает запуска")] // 5
        AWAITING_LAUNCH,
        [Display(Name = "Работает")] // 6
        IN_WORKING,
        [Display(Name = "Ожидает разгрузки")] // 7
        AWAIT_UNLOADING,
        [Display(Name = "Не работает")] // 8
        DOWNTIME,
        [Display(Name = "Синхронизация с маршрутами")] // 9
        DOWNTIME_FINISH
    }

    public class Region
    {
        #region Поля
        #region Простые
        public int Id { get; set; } // Номер объекта в БД
        [Required(ErrorMessage = "Название обязательно")]
        [StringLength(50, ErrorMessage = "Название должно быть не длиннее 50 символов")]
        public string Name { get; set; } = ""; // Название объекта
        public int Workload { get; set; } = 0; // Текущая загруженность
        public int TransitTime { get; set; } = 0; // Время прохода продукции по участку
        public int ReadjustmentTime { get; set; } = 0; // Время, затрачиваемое на переналадку (в минутах)
        public RegionStatus Status { get; set; } = RegionStatus.OFF; // Текущий статус участка
        #endregion

        #region Ссылочные
        [NotMapped]
        public DB context = new();
        [ForeignKey(nameof(TypeId))]
        public ProductionStage Type { get; set; } = new ProductionStage() { Name = "-" }; // Тип участка
        public Factory Factory { get; set; } = null!; // Фабрика
        public List<Route> Routes { get; set; } = []; // Маршруты, проходящие по данному участку
        public List<Region> RegionsParents { get; set; } = []; // Список родительских участков
        public List<Region> RegionsChildrens { get; set; } = []; // Список подчиннных участков, куда направляется изготовленная продукция
        public Downtime? Downtime { get; set; } // Простой
        public List<MaterialForRegion> Materials { get; set; } = []; // Производительность под каждое сырье
        public Material? MaterialOptionNow { get; set; } // Под какое сырьё участок сейчас настроен
        public Part? Part { get; set; }
        #endregion

        #region Id ссылок
        public int FactoryId { get; set; }
        public int TypeId = 0;
        public int? MaterialOptionNowId { get; set; } // Под какое сырьё участок сейчас настроен
        [NotMapped]
        public List<int>? RoutesId { get; set; } // Маршруты, проходящие по данному участку
        [NotMapped]
        public List<int>? RegionsParentsId { get; set; } // Список родительских участков
        [NotMapped]
        public List<int>? RegionsChildrensId { get; set; } // Список подчиннных участков, куда направляется изготовленная продукция
        [NotMapped]
        public List<int>? MaterialsId { get; set; } // Производительность под каждое сырье
        #endregion

        #region ВременнЫе
        [NotMapped]
        private System.Timers.Timer timer = null!;
        [NotMapped]
        private DateTime? startTimer = null;
        #endregion
        #endregion

        #region Свойства
        public bool IsRegionsParents
        {
            get
            {
                try
                {
                    if (RegionsParents == null || RegionsParents.Count == 0)
                    {
                        List<Region> regions = context.Regions
                            .AsNoTracking()
                            .Where(r => r.Id == Id)
                            .Include(r => r.RegionsParents)
                            .First()
                            .RegionsParents;
                        if (regions.Count == 0)
                            return false;
                    }
                    return true;
                }
                catch { return false; }
            }
        }
        public bool IsRegionsChildrens
        {
            get
            {
                try
                {
                    if (RegionsChildrens == null || RegionsChildrens.Count == 0)
                    {
                        List<Region> regions = context.Regions
                            .AsNoTracking()
                            .Where(r => r.Id == Id)
                            .Include(r => r.RegionsChildrens)
                            .First()
                            .RegionsChildrens;
                        if (regions.Count == 0)
                            return false;
                    }
                    return true;
                }
                catch { return false; }
            }
        }
        #endregion

        #region Методы
        #region Взаимодействие с БД
        public bool SavePlan()
        {
            Region? region = context.Regions
                .Where(r => r.Id == Id)
                .Include(r => r.Part)
                .FirstOrDefault();
            if (region == null)
                return false;

            region.Part = Part;
            context.SaveChanges();
            return true;
        }
        public bool SaveStatus()
        {
            Region? region = context.Regions
                .Where(r => r.Id == Id)
                .FirstOrDefault();
            if (region == null)
                return false;

            region.Status = Status;
            context.SaveChanges();
            return true;
        }
        #endregion

        #region Информационные
        /// <summary>
        /// Взять список сырья, на котором может работать данный участок
        /// </summary>
        public List<Material> GetMaterials()
        {
            if (Materials.Count == 0)
                Materials = [.. context.RegionsMaterials.AsNoTracking().Where(rm => rm.RegionId == Id)];

            List<Material> result = [];
            foreach (var material in Materials)
            {
                try
                {
                    result.Add(context.Materials.AsNoTracking().Where(m => m.Id == material.MaterialId).First());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return result;
        }

        /// <summary>
        /// Объём (в граммах) сырья, которое может принять участок
        /// </summary>
        public int GetVolumeSizeMaterial(int materialId)
        {
            double volume = 0;
            foreach (var m in Materials)
                if (m.MaterialId == materialId)
                {
                    volume = (m.Power / 60.0) * TransitTime;
                    break;
                }
            return (int)volume;
        }

        /// <summary>
        /// Количество продукции (в пачках), которое может произвести участок
        /// </summary>
        public int GetVolumeCountProduct(int productId)
        {
            Package product = context.Package
                .AsNoTracking()
                .Where(p => p.Id == productId)
                .Include(p => p.ProductGroup)
                .First();
            double volume = 0;
            var m = Materials.FirstOrDefault(m => m.Id == product.ProductGroup.MaterialId);
            if (m!= null)
                volume = ((m.Power / 60.0) * TransitTime) / product.Size;
            return (int)volume;
        }

        /// <summary>
        /// Время (в минутах), за которое участок закончит выполнение данного плана.
        /// Если данный план уже установлен участку и производится на данный момент,
        /// то вернётся время, оставшееся до конца производства; если ожидает разгрузки - 0.
        /// </summary>
        public double GetTime(Part part)
        {
            // Проверка
            if (Part != null && Part.Id == part.Id)
            {
                if (Status == RegionStatus.IN_WORKING && timer != null && timer.Enabled)
                    return startTimer!.Value.AddMilliseconds(timer.Interval).ToUniversalTime().Subtract(DateTime.UtcNow).TotalMinutes;
                else if (Status == RegionStatus.AWAIT_UNLOADING)
                    return 0;
            }

            // Подсчёт
            double time = double.PositiveInfinity;
            foreach (var m in Materials)
                if (m.MaterialId == part.MaterialId || (part.Material != null && part.Material.Id == m.MaterialId))
                {
                    int volume = (int)(m.Power / 60.0 * TransitTime);
                    int complete = 0;
                    while (complete < part.Size)
                        complete += volume;
                    double PowerInMinute = m.Power / 60.0;
                    time = complete / PowerInMinute;
                    return time;
                }
            return time;
        }

        /// <summary>
        /// Время (в минутах), которое понадобится участку для выполнения всего подготовленного плана.
        /// Уже выполненные планы не берутся в расчёт.
        /// Простаивание берётся в расчёт.
        /// </summary>
        public double GetTime(bool ConsiderDowntime = true)
        {
            double Time = 0;
            // Считаем суммарное время, которое понадобится для выполнения без учёта простоев
            foreach (var route in Routes)
                foreach (var part in route.Part.Where(p => p.Route.PlanWasCompletedInRegion(p.Id, Id)!.Value <= 0))
                    Time += GetTime(part);

            // Теперь учитываем простои
            if (ConsiderDowntime && Downtime != null)
            {
                // Находим пересечение простоя с выполнением плана
                DateTime dtFinish = DateTime.Now.AddMinutes(Time);
                if (dtFinish > Downtime.GetDowntimeStartLocal)
                {
                    // Если мы здесь, то пересечение имеется
                    if (Downtime.DowntimeFinish == null)
                        return double.PositiveInfinity;
                    else if (Downtime.IsDowntimeNow)
                        Time += Downtime.DowntimeFinish!.Value.Subtract(DateTime.UtcNow).TotalMinutes;
                    else
                        Time += Downtime.DowntimeFinish!.Value.Subtract(Downtime.DowntimeStart).TotalMinutes;
                }
            }
            return Time;
        }

        /// <summary>
        /// Время (в минутах), которое понадобится участку для выполнения всего подготовленного плана.
        /// Уже выполненные и менее приоритетные партии, которые ещё не начали своё движение) не берутся в расчёт.
        /// Простаивание берётся в расчёт.
        /// </summary>
        public double GetTime(DateTime? priority = null, bool ConsiderDowntime = true)
        {
            double Time = 0;
            // Считаем суммарное время, которое понадобится для выполнения без учёта простоев
            foreach (var route in Routes)
            {
                var lp = priority == null ? route.Part : route.Part.Where(p => p.ComingSoon.ToUniversalTime() <= priority.Value.ToUniversalTime());
                foreach (var part in lp)
                {
                    int res = route.PlanWasCompletedInRegion(part.Id, Id)!.Value;
                    if (res > 0 || (res == 0 && Status == RegionStatus.AWAIT_UNLOADING))
                        continue;
                    else
                        Time += GetTime(part);
                }
            }

            // Теперь учитываем простои
            if (ConsiderDowntime && Downtime != null)
            {
                // Находим пересечение простоя с выполнением плана
                DateTime dtFinish = DateTime.Now.AddMinutes(Time);
                if (dtFinish > Downtime.GetDowntimeStartLocal)
                {
                    // Если мы здесь, то пересечение имеется
                    if (Downtime.DowntimeFinish == null)
                        return double.PositiveInfinity;
                    else if (Downtime.IsDowntimeNow)
                        Time += Downtime.DowntimeFinish!.Value.Subtract(DateTime.UtcNow).TotalMinutes;
                    else
                        Time += Downtime.DowntimeFinish!.Value.Subtract(Downtime.DowntimeStart).TotalMinutes;
                }
            }
            return Time;
        }

        /// <summary>
        /// Найти самый ранний план, который участок должен будет выполнить.
        /// Ищутся только уже задействованные планы.
        /// </summary>
        public Part? FindEarlyPlan()
        {
            return FindEarlyPlan(this);
        }

        /// <summary>
        /// Найти самый ранний план, который участок должен будет выполнить.
        /// Ищутся только уже задействованные планы.
        /// </summary>
        public static Part? FindEarlyPlan(Region region)
        {
            // Если нет родительского участка (или у этого есть план), возвращаем значение
            if (region.RegionsParents.Count == 0 || region.Part != null)
            {
                if (region.Part == null)
                    return null;
                else
                    return region.Part;
            }

            // Поиск в родительских участках
            List<Part> plans = [];
            foreach (var r in region.RegionsParents)
            {
                Part? p = FindEarlyPlan(r);
                if (p != null)
                    plans.Add(p);
            }

            // Конец
            if (plans.Count == 0)
                return null;
            return plans[plans.FindIndex(p => p.ComingSoon == plans.Min(p => p.ComingSoon))];
        }
        #endregion

        #region Работа с данным участком
        /// <summary>
        /// Установить план
        /// </summary>
        /// <returns>Если план успешно установлен, то возвращается true, иначе - false</returns>
        public bool SetPart(ref Part Part)
        {
            if (Part == null)
                return false;
            // Проверка
            int routeId = Part.RouteId;
            int routeIndex = Routes.FindIndex(r => r.Id == routeId);
            if (Status != RegionStatus.FREE || routeIndex == -1 || Workload > 0 ||
                (Downtime != null && Downtime.IsDowntime(DateTime.UtcNow.AddMinutes(GetTime(Part)))))
                return false;

            // Установка плана
            this.Part = Part;
            Part!.Region = this;
            if (MaterialOptionNowId == null || MaterialOptionNowId != Part.MaterialId)
                Part.Route.RegionUpdateStatus(Id, RegionStatus.READJUSTMENT);
            else
                Part.Route.RegionUpdateStatus(Id, RegionStatus.AWAIT_DOWNLOAD);

            // Завршение выполнения метода
            Part.Status = PartStatus.PRODUCTION;
            return true;
        }

        /// <summary>
        /// Включить участок.
        /// </summary>
        public void Launch()
        {
            switch (Status)
            {
                case RegionStatus.OFF: Status = RegionStatus.FREE; return;
                case RegionStatus.AWAIT_DOWNLOAD:
                case RegionStatus.AWAIT_UNLOADING:
                    if (Workload <= 0)
                    {
                        Workload = 0;
                        Status = RegionStatus.FREE;
                    }
                    Part?.Route.RegionUpdateStatus(Id);
                    return;
                case RegionStatus.IN_WORKING:
                    {
                        if (Part == null)
                        {
                            Part? earlyPlan = FindEarlyPlan();
                            Workload = 0;
                            Status = RegionStatus.FREE;
                            if (earlyPlan != null)
                                Routes[Routes.FindIndex(r => r.Id == earlyPlan.RouteId)].RegionUpdateStatus(Id);
                            return;
                        }
                        //timer = new(TimeSpan.FromSeconds(GetTime(Part)));
                        timer = new(TimeSpan.FromMinutes(GetTime(Part)));
                        timer.Elapsed += Finish;
                        timer.Start();
                        startTimer = DateTime.UtcNow;
                        Part.Route.RegionUpdateStatus(Id, RegionStatus.IN_WORKING);
                        return;
                    }
                case RegionStatus.READJUSTMENT:
                case RegionStatus.FREE_READJUSTMENT:
                    StartReadjustment();
                    return;
                case RegionStatus.DOWNTIME:
                    StartDowntime();
                    return;
                default: return;
            }
        }

        /// <summary>
        /// Запустить участок
        /// </summary>
        public bool Start()
        {
            if (Part == null || Status != RegionStatus.AWAITING_LAUNCH)
                return false;
            //timer = new(TimeSpan.FromSeconds(GetTime(Part)));
            timer = new(TimeSpan.FromMinutes(GetTime(Part)));
            timer.Elapsed += Finish;
            timer.Start();
            startTimer = DateTime.UtcNow;
            Part.Route.RegionUpdateStatus(Id, RegionStatus.IN_WORKING);
            return true;
        }

        /// <summary>
        /// Завершить работу участка
        /// </summary>
        public void Finish(object? o, ElapsedEventArgs eea)
        {
            timer.Stop();
            try
            {
                if (Part == null)
                    throw new Exception("План не найден");
                Part.Route.RegionUpdateStatus(Id, RegionStatus.AWAIT_UNLOADING);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ОШИБКА: " + ex.Message);
            }
        }

        /// <summary>
        /// Начало переналадки.
        /// </summary>
        public void StartReadjustment(int? materialId = null)
        {
            if (timer != null && timer.Enabled)
                return;
            if (Workload > 0)
                throw new Exception("Нельзя начать переналадку, если что-то уже загружено");
            if (materialId != null)
            {
                MaterialOptionNowId = materialId;
                Status = RegionStatus.FREE_READJUSTMENT;
            }
            else if (Part != null)
            {
                MaterialOptionNowId = Part.MaterialId;
                Status = RegionStatus.READJUSTMENT;
            }
            else
            {
                Part ePlan = FindEarlyPlan(this) ?? throw new Exception("Неизвестно на что переналаживать");
                MaterialOptionNowId = ePlan.MaterialId;
                Status = RegionStatus.FREE_READJUSTMENT;
            }

            // Запуск таймера (считаем, что он действует мгновенно).
            timer = new(TimeSpan.FromSeconds(ReadjustmentTime + 1));
            timer.Elapsed += FinishReadjustment;
            timer.Start();
            startTimer = DateTime.UtcNow;
        }

        /// <summary>
        /// Завершение переналадки
        /// </summary>
        public void FinishReadjustment(object? o, ElapsedEventArgs eea)
        {
            if (Workload > 0)
                throw new Exception();
            timer.Stop();
            if (Part == null)
            {
                Part? plan = FindEarlyPlan();
                if (plan == null)
                    throw new Exception("");
                else
                    plan.Route.RegionUpdateStatus(Id, RegionStatus.FREE);
            }
            else
                Part!.Route.RegionUpdateStatus(Id, RegionStatus.AWAIT_DOWNLOAD);
        }

        /// <summary>
        /// Загрузить участок. Когда он загружается по плану (или до лимита), статус меняется.
        /// </summary>
        public void AddWorkload(int Size, out int remains)
        {
            // Сначала узнаём допустимый объём под данное сырьё 
            // и суммарное значение текущей нагруженности с тем, сколько пытаются загрузить
            if (Status != RegionStatus.AWAIT_DOWNLOAD)
            {
                remains = Size;
                return;
            }    
            int volume = GetVolumeSizeMaterial(MaterialOptionNowId!.Value);
            int WorkloadAndSize = Workload + Size;

            // Если объём больше этой суммы, значит место ещё хватает для заполнения.
            // В ином случае, надо загрузить сколько возможно.
            if (volume >= WorkloadAndSize)
            {
                Workload = WorkloadAndSize;
                remains = 0;
            }
            else
            {
                Workload = volume;
                remains = WorkloadAndSize - volume;
            }

            // Если по плану загружены, то, значит, участок готов к запуску
            if (Workload >= Part!.Size)
                Part.Route.RegionUpdateStatus(Id, RegionStatus.AWAITING_LAUNCH);
            else
                throw new Exception();
        }

        /// <summary>
        /// Разгрузить участок. Когда он разгружается полностью, статус меняется, а план отцепляется.
        /// </summary>
        public void SubWorkload(int Size)
        {
            if (Status != RegionStatus.AWAIT_UNLOADING)
                return;

            Workload -= Size;
            // Если по плану разгружены, то, значит, участок свободен
            if (Workload <= 0)
            {
                Status = RegionStatus.FREE;
                Part = null;
                Workload = 0;
                if (RegionsParents.Count == 0)
                    Factory.StartPlan();
                else
                {
                    Part? earlyPart = FindEarlyPlan();
                    if (earlyPart == null)
                        return;
                    int earlyRouteIndex = Routes.FindIndex(r => r.Id == earlyPart.RouteId);
                    if (earlyRouteIndex != -1)
                        Routes[earlyRouteIndex].RegionUpdateStatus(Id);
                }
            }
        }

        /// <summary>
        /// Остановить идущий процесс
        /// </summary>
        public void StopProcess()
        {
            if (timer != null && timer.Enabled)
                timer.Stop();
        }

        /// <summary>
        /// Обнаружение поломки
        /// </summary>
        public void DowntimeDetected()
        {
            StopProcess();
            Workload = 0;
            Downtime = new Downtime()
            {
                Region = this,
                RegionId = Id,
                DowntimeStart = DateTime.UtcNow,
                DowntimeDuration = int.MaxValue,
                DowntimeReason = ""
            };
            Status = RegionStatus.DOWNTIME;
            foreach (var route in Routes)
                route.RegionUpdateStatus(Id);
        }

        /// <summary>
        /// Установить простой
        /// </summary>
        public bool SetDowntime(Downtime downtime)
        {
            try
            {
                if (Downtime == null)
                {
                    // Если простой ещё не установлен
                    Downtime = downtime;
                    if (Downtime.IsDowntimeNow)
                    {
                        Status = RegionStatus.DOWNTIME;
                        foreach (var route in Routes)
                            route.RegionUpdateStatus(Id);
                    }
                }
                else
                {
                    // Установка некоторых новых значений
                    Downtime.SetDowntimeDuration = downtime.DowntimeDuration;
                    Downtime.SetDowntimeFinish = downtime.DowntimeFinish;
                    Downtime.DowntimeReason = downtime.DowntimeReason;
                    if (Downtime.IsDowntimeNow)
                        Status = RegionStatus.DOWNTIME;
                }

                // Начало отсчёта
                StartDowntime();

                // Конец
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// Начать отсчёт поломки
        /// </summary>
        public void StartDowntime()
        {
            // Небольшая проверочка
            if (Status != RegionStatus.DOWNTIME || Downtime == null || Downtime.DowntimeFinish == null || !Downtime.IsDowntimeNow)
                return;

            // Запуск отсчёта
            timer = new(TimeSpan.FromMinutes(Downtime.DowntimeFinish!.Value.Subtract(Downtime.DowntimeStart).TotalMinutes));
            timer.Elapsed += StopDowntime;
            timer.Start();
            startTimer = DateTime.UtcNow;
        }

        /// <summary>
        /// Остановка отсчёта конца поломки
        /// </summary>
        private void StopDowntime(object? sender, ElapsedEventArgs e)
        {
            // Проверка небольшая
            if (Status != RegionStatus.DOWNTIME)
                return;

            // Содержание метода
            timer?.Stop(); // Сброс таймера
            Downtime = null; // Сброс простоя
            Status = RegionStatus.DOWNTIME_FINISH; // Изменение статуса
            foreach (var route in Routes) // Оповещение всех маршрутов о том, что по ним можно будет выполнить план
                route.RegionUpdateStatus(Id);
            // Уведомление, что участок готов к работе
            Status = RegionStatus.FREE;
            FindEarlyPlan()?.Route.RegionUpdateStatus(Id);
        }

        /// <summary>
        /// Объявить простаивающий участок работающим досрочно
        /// </summary>
        public void StopDowntime()
        {
            // Проверка небольшая
            if (Status != RegionStatus.DOWNTIME)
                return;

            // Содержание метода
            timer?.Stop(); // Сброс таймера
            Downtime = null; // Сброс простоя
            Status = RegionStatus.DOWNTIME_FINISH; // Изменение статуса
            foreach (var route in Routes) // Оповещение всех маршрутов о том, что по ним можно будет выполнить план
                route.RegionUpdateStatus(Id);
            // Уведомление, что участок готов к работе
            Status = RegionStatus.FREE;
            FindEarlyPlan()?.Route.RegionUpdateStatus(Id);
        }
        #endregion

        #region Статические и переопределяющие
        public override string ToString()
        {
            return Name;
        }
        #endregion
        #endregion
    }
}