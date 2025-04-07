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
        private static readonly DB context = new();
        private static readonly System.Timers.Timer timer = new(TimeSpan.FromSeconds(1));
        private static List<Factory.Factory> factories = [];
        private static readonly Dictionary<int, Factory.Factory> Factories = [];

        #region Обращение к БД
        public static void FactoryLoad()
        {
            try
            {
                factories = [..context.Factories
                    .Include(f=>f.Warehouse).ThenInclude(w=>w.Products)
                    .Include(f=>f.Warehouse).ThenInclude(w=>w.Materials)
                    .Include(f=>f.Routes).ThenInclude(r=>r.Regions)
                    .Include(f=>f.Routes).ThenInclude(r=>r.Plan)
                    .Include(f=>f.Regions).ThenInclude(r=>r.Plan)
                    .Include(f=>f.Regions).ThenInclude(r=>r.Routes)
                    .Include(f=>f.Regions).ThenInclude(r=>r.RegionsParents)
                    .Include(f=>f.Regions).ThenInclude(r=>r.RegionsChildrens)
                    .Include(f=>f.Regions).ThenInclude(r=>r.MaterialOptionNow)
                    .Include(f=>f.Regions).ThenInclude(r=>r.Downtime)
                    .Include(f=>f.Regions).ThenInclude(r=>r.Materials)
                    .Include(f=>f.Plan)
                    .Include(f=>f.Plan).ThenInclude(p=>p.Route)
                    .Include(f=>f.Plan).ThenInclude(p=>p.Region)
                    .Include(f=>f.Plan).ThenInclude(p=>p.Product)
                    .Include(f=>f.Plan).ThenInclude(p=>p.Material)
                    .Include(f=>f.Orders.Where(r=>r.Status == RequestStatus.FABRICATING))];
            }
            catch (TimeoutException te)
            {
                Console.WriteLine(te.Message);
                return;
            }

            factories.ForEach(f =>
            {
                f.LaunchAllRegions();
                f.StartPlan();
            });

            Factories.Clear();
            for (int i = 0; i < factories.Count; ++i)
                Factories.Add(factories[i].Id, factories[i]);

            timer.Elapsed += Save;
            timer.Start();
        }
        
        public static void Save(object? o, ElapsedEventArgs eea)
        {
            try
            {
                context.SaveChanges();
            }
            catch { }
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
            f.Plan.First(p => p.Id == planId).Status = PlanStatus.QUEUE;
            context.SaveChanges();
            f.StartPlan();
        }
        public static void PlanApproveAll(int factoryId)
        {
            _ = Factories[factoryId].Plan
                .Where(p => p.Status == PlanStatus.AWAIT_CONFIRMATION)
                .All(p => { p.Status = PlanStatus.QUEUE; return true; });
            context.SaveChanges();
            Factories[factoryId].StartPlan();
        }
        public static void PlanCreate(Plan plan)
        {
            Factory.Factory f = factories.First(f => f.Id == plan.FactoryId);
            plan.Route = f.Routes.First(r => r.Id == plan.RouteId);
            plan.Factory = f;
            f.Plan.Add(plan);
            f.Routes.First(r => r.Id == plan.RouteId).Plan.Add(plan);
            
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
                .Any(p => { p.Status = PlanStatus.PAUSE; return true; });
            context.SaveChanges();
        }
        public static void PlanEditRoute(Plan plan)
        {
            // Нахождение необходимых переменных
            Plan p = Factories[plan.FactoryId].Plan.First(p => p.Id == plan.Id);
            Route route = Factories[plan.FactoryId].Routes.First(r => r.Id == plan.RouteId);

            // Смена значений
            if (p.RouteId == plan.RouteId)
                p.Status = PlanStatus.PAUSE;
            else
            {
                p.RouteId = plan.RouteId;
                p.Route = route;
                p.Status = PlanStatus.PRODUCTION;
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