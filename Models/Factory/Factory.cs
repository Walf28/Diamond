using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyCompany
{
    public class Factory : FactoryObject
    {
        #region Поля
        #region Ссылочные
        [NotMapped]
        private readonly DB context = new();
        public List<Route> Routes { get; set; } = [];
        public List<Region> Regions { get; set; } = [];
        public List<Request> Requests { get; set; } = [];
        #endregion

        #region Id ссылок
        [NotMapped]
        public List<int> RoutesId { get; set; } = [];
        [NotMapped]
        public List<int> RegionsId { get; set; } = [];
        [NotMapped]
        public List<int> RequestsId { get; set; } = [];
        #endregion
        #endregion

        public Factory()
        {
            context = new DB();
        }

        #region Методы
        public List<Route> FindAllRoutes()
        {
            if (Regions.Count == 0)
                Regions = [.. context.Regions.Where(r => r.FactoryId == Id).Include(r => r.RegionsParents).Include(r => r.RegionsChildrens)];

            List<Route> routes = [];
            foreach (var r in Regions)
            {
                if (r.RegionsParents.Count != 0 || (r.RegionsParents.Count == 0 && r.RegionsChildrens.Count == 0))
                    continue;
                routes.AddRange(FindRouteFor(r));
            }

            return routes;
        }
        private List<Route> FindRouteFor(Region region, List<Route>? routes = null, List<Region>? list = null)
        {
            list ??= [region];
            routes ??= [];
            if (region.RegionsChildrens.Count == 0)
                region = context.Regions.Where(r => r.Id == region.Id).First();

            // Конец
            if (region.RegionsChildrens.Count == 0)
            {
                routes.Add(new() { Regions = list });
                return routes;
            }

            // Просмотр всех последующих участков
            foreach (var children in region.RegionsChildrens)
            {
                List<Region> regions = new(list)
                {
                    children
                };
                routes.AddRange(FindRouteFor(children, [.. routes], [.. regions]));
            }

            return routes;
        }
        #endregion
    }
}