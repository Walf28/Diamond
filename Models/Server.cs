using Diamond.Database;
using Diamond.Models.Factory;
using Diamond.Models.Orders;
using Microsoft.EntityFrameworkCore;
using System.Timers;
using Route = Diamond.Models.Factory.Route;

namespace Diamond.Models
{
    public static class Server
    {
        private static DB context = new();
        private static readonly System.Timers.Timer timer = new(TimeSpan.FromSeconds(2));
        private static List<Factory.Factory> factories = [];
        public static readonly Dictionary<int, Factory.Factory> Factories = [];

        #region Обращение к БД
        public static void Load()
        {
            try
            {
                context = new();
                factories.Clear();
                factories = [..context.Factories
                    .Include(f=>f.Warehouse).ThenInclude(w=>w.Products)
                    .Include(f=>f.Warehouse).ThenInclude(w=>w.Materials)
                    .Include(f=>f.Routes).ThenInclude(r=>r.Regions)
                    .Include(f=>f.Routes).ThenInclude(r=>r.Part)
                    .Include(f=>f.Regions).ThenInclude(r=>r.Part)
                    .Include(f=>f.Regions).ThenInclude(r=>r.Routes)
                    .Include(f=>f.Regions).ThenInclude(r=>r.RegionsParents)
                    .Include(f=>f.Regions).ThenInclude(r=>r.RegionsChildrens)
                    .Include(f=>f.Regions).ThenInclude(r=>r.MaterialOptionNow)
                    .Include(f=>f.Regions).ThenInclude(r=>r.Downtime)
                    .Include(f=>f.Regions).ThenInclude(r=>r.Materials)
                    .Include(f=>f.Regions).ThenInclude(r=>r.Type)
                    .Include(f=>f.Plan).ThenInclude(p=>p.Route)
                    .Include(f=>f.Plan).ThenInclude(p=>p.Region)
                    .Include(f=>f.Plan).ThenInclude(p=>p.Product)
                    .Include(f=>f.Plan).ThenInclude(p=>p.Material)
                    .Include(f=>f.Plan).ThenInclude(p=>p.Order)
                    .Include(f=>f.Orders.Where(r=>r.Status == OrderStatus.FABRICATING)).ThenInclude(o => o.OrderParts).ThenInclude(op => op.Product).ThenInclude(p => p.ProductGroup)];
            }
            catch (TimeoutException te)
            {
                Console.WriteLine(te.Message);
                return;
            }

            /*factories.ForEach(f =>
            {
                f.LaunchAllRegions();
                f.StartPlan();
            });*/

            Factories.Clear();
            for (int i = 0; i < factories.Count; ++i)
                Factories.Add(factories[i].Id, factories[i]);

            timer.Elapsed += Save;
            timer.Start();
        }
        
        private static void Save(object? o, ElapsedEventArgs eea)
        {
            Save();
        }
        public static void Save()
        {
            try
            {
                context.SaveChanges(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА! {ex.Message}");
            }
        }
        #endregion

        #region Маршруты
        public static void UpdateRoutes(int factoryId)
        {
            Load();
            Factories[factoryId].Routes.Where(r=>!r.IsWorking).ToList().ForEach(
                r => context.Routes.Entry(r).State = EntityState.Deleted);
            Factories[factoryId].UpdateAllRoutes();
            context.Routes.AddRange(Factories[factoryId].Routes.Where(r => r.Id == 0));
            context.SaveChanges();
        }
        #endregion

        #region Для отладки
        public static void NullMethod()
        {
            _ = 0;
        }

        public static void NullALL()
        {
            foreach (var item in factories)
                foreach (Region region in item.Regions)
                {
                    region.Status = RegionStatus.FREE;
                    region.Workload = 0;
                    region.StopProcess();
                }
            context.Plans.ExecuteDelete();
            context.Orders.ExecuteDelete();
            context.SaveChanges();
        }
        #endregion

        #region Заявки
        public static void AddRequest(Order request)
        {
            Factories[request.FactoryId!.Value].AddRequest(request.Id);
        }
        #endregion

        #region Планирование
        public static void PlanApprove(int planId)
        {
            Factory.Factory f = factories.First(f => f.Plan.FirstOrDefault(p => p.Id == planId) != null);
            f.Plan.First(p => p.Id == planId).Status = PartStatus.QUEUE;
            context.SaveChanges();
            f.StartPlan();
        }
        public static void PlanApproveAll(int factoryId)
        {
            _ = Factories[factoryId].Plan
                .Where(p => p.Status == PartStatus.AWAIT_CONFIRMATION)
                .All(p => { p.Status = PartStatus.QUEUE; return true; });
            context.SaveChanges();
            Factories[factoryId].StartPlan();
        }
        public static void PlanCreate(Part plan)
        {
            Factory.Factory f = factories.First(f => f.Id == plan.FactoryId);
            plan.Route = f.Routes.First(r => r.Id == plan.RouteId);
            plan.Factory = f;
            f.Plan.Add(plan);
            f.Routes.First(r => r.Id == plan.RouteId).Part.Add(plan);
            
            context.Plans.Add(plan);
            context.SaveChanges();
            f.StartPlan();
        }
        public static void PlanDelete(int planId)
        {
            Factories.Values.First(f => f.Plan.FirstOrDefault(p => p.Id == planId) != null)
                .Plan.RemoveAll(p => p.Id == planId);
            context.SaveChanges();
        }
        public static void PlanAwait(int planId)
        {
            _ = Factories.Values
                .Select(f => f.Plan.First(p => p.Id == planId))
                .Any(p => { p.Status = PartStatus.PAUSE; return true; });
            context.SaveChanges();
        }
        public static void PlanEditRoute(Part plan)
        {
            // Нахождение необходимых переменных
            Part p = Factories[plan.FactoryId].Plan.First(p => p.Id == plan.Id);
            Route route = Factories[plan.FactoryId].Routes.First(r => r.Id == plan.RouteId);

            // Смена значений
            if (p.RouteId == plan.RouteId)
                p.Status = PartStatus.PAUSE;
            else
            {
                p.RouteId = plan.RouteId;
                p.Route = route;
                p.Status = PartStatus.PRODUCTION;
            }

            // Сохранение
            context.SaveChanges();
        }
        #endregion

        #region Простои
        public static void DowntimeCreate(int regionId)
        {
            Factories.Values
                .FirstOrDefault(f => f.Regions.FirstOrDefault(r => r.Id == regionId) != null)?
                .Regions.First(r=>r.Id == regionId)
                .DowntimeDetected();
            context.SaveChanges();
        }
        public static void DowntimeSet(Downtime downtime)
        {
            if (downtime.RegionId == 0)
                return;
            Factories.Values.Select(f => f.Regions
                .First(r => r.Id == downtime.RegionId))
                .First().SetDowntime(downtime);
            context.SaveChanges();
        }
        public static void DowntimeStop(int regionId)
        {
            if (regionId == 0)
                return;
            Factories.Values.Select(f => f.Regions
                .First(r => r.Id == regionId))
                .First().StopDowntime();
            context.SaveChanges();
        }
        #endregion
    }
}