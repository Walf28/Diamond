using Diamond.Database;
using Microsoft.EntityFrameworkCore;

namespace Diamond.Models.Factory
{
    public static class Server
    {
        private static readonly DB context = new();
        private static readonly Dictionary<int, Factory> Factories = [];
        private static List<Factory> list = [];

        public static void FactoryLoad()
        {
            list = [..context.Factories
                .Include(f=>f.Routes).ThenInclude(r=>r.Regions)
                .Include(f=>f.Regions).ThenInclude(r=>r.Plan)
                .Include(f=>f.Regions).ThenInclude(r=>r.RegionsParents)
                .Include(f=>f.Regions).ThenInclude(r=>r.RegionsChildrens)
                .Include(f=>f.Regions).ThenInclude(r=>r.MaterialOptionNow)
                .Include(f=>f.Regions).ThenInclude(r=>r.Downtime)
                .Include(f=>f.Regions).ThenInclude(r=>r.Materials)
                .Include(f=>f.Plan).ThenInclude(p=>p.Route)
                .Include(f=>f.Plan).ThenInclude(p=>p.Region)
                .Include(f=>f.Plan).ThenInclude(p=>p.Product)
                .Include(f=>f.Requests)];

            Factories.Clear();
            /*for (int i = 0; i < list.Count; ++i)
                Factories.Add(list[i].Id, list[i]);*/
        }

        public static void Save()
        {
            context.SaveChanges();
        }

        public static void AddRequest(Request request)
        {
            try
            {
                for (int i = 0; i < list.Count; ++i)
                    if (list[i].Id == request.FactoryId)
                    {
                        list[i].AddRequest(request.Id);
                        context.SaveChanges();
                        break;
                    }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}