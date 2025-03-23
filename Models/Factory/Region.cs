using Diamond.Database;
using Diamond.Models.Materials;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diamond.Models.Factory
{
    public class Region : FactoryObject
    {
        #region Поля
        #region Простые
        public Technology Type { get; set; } = Technology.NONE; // Тип участка
        public int Workload { get; set; } = 0; // Текущая загруженность
        public int MaxVolume { get; set; } = 0; // Максимальная вместительность участка
        public int TransitTime { get; set; } = 0; // Время прохода продукции по участку
        public RegionStatus Status { get; set; } = 0; // Текущий статус участка
        public int UploadedNow { get; set; } = 0; // Сейчас загружено
        #endregion

        #region Ссылочные
        [NotMapped]
        public DB context = new();
        public Factory Factory { get; set; } = null!;// Фабрика
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

        #region Временные
        //private Timer timer = null!;
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
        /// Уже выполненные планы не берутся в расчёт.
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
                if (m.MaterialId == plan.GetMaterial!.Id)
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
        #endregion

        #region Работа с данным участком
        // Обновить статус
        private void SetStatus(RegionStatus status, bool SaveInDatabase = true)
        {
            Status = status;
            if (SaveInDatabase)
                SaveStatus();
        }

        // Выбрать, что производить
        public void SetPlan(Plan Plan, out bool SaveSuccess)
        {
            SaveSuccess = false;
            if (Status != RegionStatus.READY_TO_WORK)
                return;
            Region? region = context.Regions
                .Where(r => r.Id == Id)
                .Include(r => r.Plan)
                .FirstOrDefault();

            this.Plan = Plan;
            SaveSuccess = SavePlan();

            if (MaterialOptionNowId == null || MaterialOptionNowId != Plan.GetMaterial!.Id)
                SetStatus(RegionStatus.READJUSTMENT);
            else
                SetStatus(RegionStatus.AWAIT_DOWNLOAD);

            //Start();
        }
        #endregion
        #endregion
    }
}