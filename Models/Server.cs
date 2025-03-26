using Diamond.Database;
using Microsoft.EntityFrameworkCore;
using System.Timers;

namespace Diamond.Models
{
    public static class Server
    {
        private static readonly DB context = new();
        private static readonly System.Timers.Timer timer = new(TimeSpan.FromSeconds(3));
        private static readonly Dictionary<int, Factory.Factory> Factories = [];
        private static List<Factory.Factory> list = [];
        

        public static void FactoryLoad()
        {
            list = [..context.Factories
                .Include(f=>f.Warehouse).ThenInclude(w=>w.Products)
                .Include(f=>f.Warehouse).ThenInclude(w=>w.Materials)
                .Include(f=>f.Routes).ThenInclude(r=>r.Regions)
                .Include(f=>f.Routes).ThenInclude(r=>r.Plan)
                .Include(f=>f.Regions).ThenInclude(r=>r.Plan)
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
                .Include(f=>f.Requests)];

            Factories.Clear();
            /*for (int i = 0; i < list.Count; ++i)
                Factories.Add(list[i].Id, list[i]);*/

            list.ForEach(f => 
            {
                f.LaunchAllRegions();
                f.StartPlan();
            });

            timer.Elapsed += Save;
            timer.Start();
        }

        public static void Save(object? o, ElapsedEventArgs eea)
        {
            context.SaveChanges();
        }

        public static void NullMethod()
        {
            int x;
            x = 0;
            ++x;
        }

        public static void AddRequest(Request request)
        {
            for (int i = 0; i < list.Count; ++i)
                if (list[i].Id == request.FactoryId)
                {
                    list[i].AddRequest(request.Id);
                    break;
                }
        }
    }
}