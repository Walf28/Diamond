using Diamond.Database;
using Diamond.Models.Materials;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Timers;

namespace Diamond.Models.Factory
{
    public class Region : FactoryObject
    {
        #region Поля
        #region Простые
        public Technology Type { get; set; } = Technology.NONE; // Тип участка
        public int Workload { get; set; } = 0; // Текущая загруженность
        public int TransitTime { get; set; } = 0; // Время прохода продукции по участку
        public int ReadjustmentTime { get; set; } = 0; // Время, затрачиваемое на переналадку (в минутах)
        public RegionStatus Status { get; set; } = RegionStatus.OFF; // Текущий статус участка
        #endregion

        #region Ссылочные
        [NotMapped]
        public DB context = new();
        public Factory Factory { get; set; } = null!; // Фабрика
        public List<Route> Routes { get; set; } = []; // Маршруты, проходящие по данному участку
        public List<Region> RegionsParents { get; set; } = []; // Список родительских участков
        public List<Region> RegionsChildrens { get; set; } = []; // Список подчиннных участков, куда направляется изготовленная продукция
        public Downtime? Downtime { get; set; } // Простой
        public List<MaterialForRegion> Materials { get; set; } = []; // Производительность под каждое сырье
        public Material? MaterialOptionNow { get; set; } // Под какое сырьё участок сейчас настроен
        public Plan? Plan { get; set; }
        #endregion

        #region Id ссылок
        public int FactoryId { get; set; }
        [NotMapped]
        public List<int>? RoutesId { get; set; } // Маршруты, проходящие по данному участку
        [NotMapped]
        public List<int>? RegionsParentsId { get; set; } // Список родительских участков
        [NotMapped]
        public List<int>? RegionsChildrensId { get; set; } // Список подчиннных участков, куда направляется изготовленная продукция
        [NotMapped]
        public List<int>? MaterialsId { get; set; } // Производительность под каждое сырье
        public int? MaterialOptionNowId { get; set; } // Под какое сырьё участок сейчас настроен
        #endregion

        #region ВременнЫе
        [NotMapped]
        private System.Timers.Timer timer = null!;
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
                .Include(r => r.Plan)
                .FirstOrDefault();
            if (region == null)
                return false;

            region.Plan = Plan;
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
                result.Add(context.Materials.AsNoTracking().Where(m => m.Id == material.MaterialId).First());
            }

            return result;
        }

        /// <summary>
        /// Объём (в граммах) сырья, которое может принять участок
        /// </summary>
        public int GetVolume(int materialId)
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
        /// Время (в минутах), которое понадобится участку для выполнения всего подготовленного плана.
        /// Уже выполненные планы тоже берутся в расчёт.
        /// </summary>
        public double GetTime()
        {
            double Time = 0;
            foreach (var route in Routes)
                foreach (var plan in route.Plan)
                    Time += GetTime(plan);
            return Time;
        }

        /// <summary>
        /// Время (в минутах), за сколько участок переработает данное сырьё вы указанном размере
        /// </summary>
        public double GetTime(int materialId, int Size)
        {
            double time = double.PositiveInfinity;
            foreach (var m in Materials)
                if (m.MaterialId == materialId)
                {
                    int volume = (int)(m.Power / 60.0 * TransitTime);
                    int complete = 0;
                    while (complete < Size)
                        complete += volume;
                    double PowerInMinute = m.Power / 60.0;
                    time = complete / PowerInMinute;
                    return time;
                }
            return time;
        }

        /// <summary>
        /// Время (в минутах), за сколько участок переработает данное сырьё в данном плане
        /// </summary>
        public double GetTime(Plan plan)
        {
            double time = double.PositiveInfinity;
            foreach (var m in Materials)
                if (m.MaterialId == plan.MaterialId || (plan.Material != null && plan.Material.Id == m.MaterialId))
                {
                    int volume = (int)(m.Power / 60.0 * TransitTime);
                    int complete = 0;
                    while (complete < plan.Size)
                        complete += volume;
                    double PowerInMinute = m.Power / 60.0;
                    time = complete / PowerInMinute;
                    return time;
                }
            return time;
        }

        /// <summary>
        /// Найти самый ранний план, который участок должен будет выполнить.
        /// Ищутся только уже задействованные планы.
        /// </summary>
        public Plan? FindEarlyPlan()
        {
            return FindEarlyPlan(this);
        }

        /// <summary>
        /// Найти самый ранний план, который участок должен будет выполнить.
        /// Ищутся только уже задействованные планы.
        /// </summary>
        public static Plan? FindEarlyPlan(Region region)
        {
            // Если нет родительского участка (или у этого есть план), возвращаем значение
            if (region.RegionsParents.Count == 0 || region.Plan != null)
            {
                if (region.Plan == null)
                    return null;
                else
                    return region.Plan;
            }

            // Поиск в родительских участках
            List<Plan> plans = [];
            foreach (var r in region.RegionsParents)
            {
                Plan? p = FindEarlyPlan(r);
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
        public bool SetPlan(ref Plan Plan)
        {
            // Проверка
            int routeId = Plan.RouteId;
            int routeIndex = Routes.FindIndex(r => r.Id == routeId);
            if (Status != RegionStatus.FREE || routeIndex == -1)
                return false;

            // Установка плана
            Plan.Region = this;
            this.Plan = Plan;
            if (MaterialOptionNowId == null || MaterialOptionNowId != Plan.MaterialId)
                Plan.Route.RegionUpdateStatus(Id, RegionStatus.READJUSTMENT);
            else
                Plan.Route.RegionUpdateStatus(Id, RegionStatus.AWAIT_DOWNLOAD);

            // Завршение выполнения метода
            Plan.IsFabricating = true;
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
                case RegionStatus.IN_WORKING:
                    {
                        if (Plan == null)
                        {
                            Plan? earlyPlan = FindEarlyPlan();
                            Workload = 0;
                            Status = RegionStatus.FREE;
                            if (earlyPlan != null)
                                Routes[Routes.FindIndex(r => r.Id == earlyPlan.RouteId)].RegionUpdateStatus(Id);
                            return;
                        }
                        timer = new(TimeSpan.FromSeconds(GetTime(Plan)));
                        timer.Elapsed += Finish;
                        timer.Start();
                        Plan.Route.RegionUpdateStatus(Id, RegionStatus.IN_WORKING);
                        return;
                    }
                case RegionStatus.READJUSTMENT:
                case RegionStatus.FREE_READJUSTMENT:
                    {
                        timer = new(TimeSpan.FromSeconds(ReadjustmentTime + 1));
                        timer.Elapsed += FinishReadjustment;
                        timer.Start();
                        return;
                    }
                case RegionStatus.AWAIT_DOWNLOAD:
                case RegionStatus.AWAIT_UNLOADING:
                    {
                        if (Plan == null)
                        {
                            Plan? earlyPlan = FindEarlyPlan();
                            Workload = 0;
                            Status = RegionStatus.FREE;
                            if (earlyPlan != null)
                                Routes[Routes.FindIndex(r => r.Id == earlyPlan.RouteId)].RegionUpdateStatus(Id);
                            return;
                        }
                        else
                            Plan.Route.RegionUpdateStatus(Id);
                        return;
                    }
                default: return;
            }
        }

        /// <summary>
        /// Запустить участок
        /// </summary>
        public bool Start()
        {
            if (Plan == null || Status != RegionStatus.AWAITING_LAUNCH)
                return false;
            timer = new(TimeSpan.FromSeconds(GetTime(Plan)));
            timer.Elapsed += Finish;
            timer.Start();
            Plan.Route.RegionUpdateStatus(Id, RegionStatus.IN_WORKING);
            return true;
        }

        /// <summary>
        /// Завершить работу участка
        /// </summary>
        public void Finish(object? o, ElapsedEventArgs eea)
        {
            timer.Stop();
            timer.Dispose();
            if (Plan == null)
                throw new Exception("План не найден");
            Plan.Route.RegionUpdateStatus(Id, RegionStatus.AWAIT_UNLOADING);
        }

        /// <summary>
        /// Начало переналадки.
        /// </summary>
        public void StartReadjustment(int? materialId = null)
        {
            if (materialId != null)
            {
                MaterialOptionNowId = materialId;
                Status = RegionStatus.FREE_READJUSTMENT;
            }
            else if (Plan != null)
            {
                MaterialOptionNowId = Plan.MaterialId;
                Status = RegionStatus.READJUSTMENT;
            }
            else
                throw new Exception("Неизвестно на что переналаживать");

            // Запуск таймера (считаем, что он действует мгновенно).
            timer = new(TimeSpan.FromMilliseconds(ReadjustmentTime + 1));
            timer.Elapsed += FinishReadjustment;
            timer.Start();
        }

        /// <summary>
        /// Завершение переналадки
        /// </summary>
        public void FinishReadjustment(object? o, ElapsedEventArgs eea)
        {
            timer.Stop();
            timer.Dispose();
            if (Plan == null)
            {
                Plan? plan = FindEarlyPlan();
                if (plan == null)
                    return;
                else
                    plan.Route.RegionUpdateStatus(Id, RegionStatus.FREE);
            }
            else
                Plan!.Route.RegionUpdateStatus(Id, RegionStatus.AWAIT_DOWNLOAD);
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
            int volume = GetVolume(MaterialOptionNowId!.Value);
            int WorkloadAndSize = Workload + Size;

            // Если объём больше этой суммы, значит место ещё хватает для заполнения.
            // В ином случае, надо загрузить сколько возможно.
            if (volume > WorkloadAndSize)
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
            if (Workload >= Plan!.Size)
            {
                Plan.Size = Workload;
                Plan.Route.RegionUpdateStatus(Id, RegionStatus.AWAITING_LAUNCH);
            }
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
                Plan = null;
                Plan? earlyPlan = FindEarlyPlan();
                if (earlyPlan != null)
                    Routes[Routes.FindIndex(r => r.Id == earlyPlan.RouteId)].RegionUpdateStatus(Id, RegionStatus.FREE_READJUSTMENT);
                else if (RegionsParents.Count == 0)
                    Factory.StartPlan();
            }
        }
        #endregion
        #endregion
    }
}