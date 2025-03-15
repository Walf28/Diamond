using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diamond.Models
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

        #region Специально для взаимодействия представления и контроллера
        [NotMapped]
        public List<bool>? check { get; set; }
        #endregion
        #endregion

        #region Методы
        // Найти все маршруты
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

        // Найти неиспользуемые маршруты
        public List<Route> FindUnusingRoutes()
        {
            if (Regions.Count == 0)
                Regions = [.. context.Regions.AsNoTracking().Where(r => r.FactoryId == Id).Include(r => r.RegionsParents).Include(r => r.RegionsChildrens)];
            if (Routes.Count == 0)
                Routes = [..context.Routes.AsNoTracking().Where(r => r.FactoryId == Id)];

            List<Route> routes = [];
            foreach (var r in Regions)
            {
                if (r.RegionsParents.Count > 0 || r.RegionsChildrens.Count == 0)
                    continue;
                List<Route> ThisRoutes = FindRouteFor(r);

                // Нахождение используемых маршрутов
                for (int i = 0; i < ThisRoutes.Count; ++i)
                    foreach (var UsedRoute in Routes)
                        if (ThisRoutes[i].GetContent == UsedRoute.GetContent)
                        {
                            ThisRoutes.RemoveAt(i);
                            --i;
                            break;
                        }

                // Добавление маршрутов
                routes.AddRange(ThisRoutes);
            }

            return routes;
        }

        // Найти маршруты, начинающиеся с данного участка
        private List<Route> FindRouteFor(Region region, List<Route>? routes = null, List<Region>? list = null)
        {
            list ??= [];
            list.Add(region);
            routes ??= [];
            if (region.RegionsChildrens.Count == 0)
                region = context.Regions.AsNoTracking().Where(r => r.Id == region.Id).Include(r=>r.RegionsChildrens).First();

            // Конец
            if (region.RegionsChildrens.Count == 0)
            {
                routes.Add(new() { Regions = list });
                return routes;
            }

            // Просмотр всех последующих участков
            foreach (var children in region.RegionsChildrens)
            {
                routes = FindRouteFor(children, [.. routes], [.. list]);
            }

            return routes;
        }
        #endregion
    }
}