using Diamond.Database;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diamond.Models.Factory
{
    public class Factory : FactoryObject
    {
        #region Поля
        #region Обычные
        // Эти два - это очередь
        public List<Plan> Plan { get; set; } = [];

        // Эти два - это суммарно, сколько и чего надо произвести
        public List<int> ProductSumId { get; set; } = []; // Что имеется в очереди
        public List<int> ProductSumSize { get; set; } = []; // Сколько этого надо произвести
        #endregion

        #region Ссылочные
        [NotMapped]
        private readonly DB context = new();
        public List<Route> Routes { get; set; } = [];
        public List<Region> Regions { get; set; } = [];
        public List<Request> Requests { get; set; } = [];
        #endregion
        #endregion

        #region Методы
        #region Взаимодействие с БД
        /// <summary>Загрузить ссылки, которые есть в данном классе</summary>
        public void LoadLinks()
        {
            Factory? factory = context.Factories
                .AsNoTracking()
                .Where(f => f.Id == Id)
                .Include(f => f.Routes)
                .Include(f => f.Regions)
                .Include(f => f.Requests.Where(r => r.Status == RequestStatus.FABRICATING))
                .FirstOrDefault();
            if (factory == null)
                return;
            Routes = factory.Routes;
            Regions = factory.Regions;
            Requests = factory.Requests;
        }
        #endregion

        #region Информационные
        /// <summary>Найти все маршруты</summary>
        public List<Route> FindAllRoutes()
        {
            if (Regions.Count == 0)
                Regions = [.. context.Regions.Where(r => r.FactoryId == Id).Include(r => r.RegionsParents).Include(r => r.RegionsChildrens)];

            List<Route> routes = [];
            foreach (var r in Regions)
            {
                if (r.RegionsParents.Count != 0 || r.RegionsParents.Count == 0 && r.RegionsChildrens.Count == 0)
                    continue;
                routes.AddRange(FindRouteFor(r));
            }

            return routes;
        }

        /// <summary>Найти неиспользуемые маршруты</summary>
        public List<Route> FindUnusingRoutes()
        {
            if (Regions.Count == 0)
                Regions = [.. context.Regions.AsNoTracking().Where(r => r.FactoryId == Id).Include(r => r.RegionsParents).Include(r => r.RegionsChildrens)];
            if (Routes.Count == 0)
                Routes = [.. context.Routes.AsNoTracking().Where(r => r.FactoryId == Id)];

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

        /// <summary>Найти маршруты, начинающиеся с данного участка</summary>
        private List<Route> FindRouteFor(Region region, List<Route>? routes = null, List<Region>? list = null)
        {
            list ??= [];
            list.Add(region);
            routes ??= [];
            if (region.RegionsChildrens.Count == 0)
                region = context.Regions.AsNoTracking().Where(r => r.Id == region.Id).Include(r => r.RegionsChildrens).First();

            // Конец
            if (region.RegionsChildrens.Count == 0)
                routes.Add(new() { Regions = list, RegionsRoute = list.Select(r => r.Id).ToList() });

            // Просмотр всех последующих участков
            foreach (var children in region.RegionsChildrens)
                routes = FindRouteFor(children, [.. routes], [.. list]);

            return routes;
        }
        #endregion

        #region Заявки
        /// <summary>Добавить заявку</summary>
        public void AddRequest(int requestId)
        {
            Request? request = context.Requests
                .Where(r => r.Id == requestId)
                .Include(r => r.Product).ThenInclude(p => p.ProductGroup)
                .FirstOrDefault() ?? throw new Exception("Заказ не найден");
            request.Status = RequestStatus.FABRICATING;

            // Добавление в план
            for (int i = 0; i < ProductSumId.Count; ++i)
            {
                if (ProductSumId[i] == requestId)
                {
                    ProductSumSize[i] += request.Count * request.Product.Size;
                    break;
                }
                else if (i == ProductSumId.Count - 1)
                {
                    ProductSumId.Add(requestId);
                    ProductSumSize.Add(request.Count * request.Product.Size);
                    break;
                }
            }

            // Добавление заявки в список заявок
            Requests.Add(request);
            AddToPlan(request);
            context.SaveChanges();
        }
        #endregion

        #region Планирование производства
        /// <summary>Добавить в производственный план</summary>
        private void AddToPlan(Request request)
        {
            LoadLinks();

            // Ищем, на каких участках возможно произвести товар
            List<Route> PotencialRoutes = [];
            List<int> MaxVolulme = [];
            foreach (var route in Routes)
                if (route.CanProduceProduct(request.ProductId, out int MaxSpeed))
                {
                    PotencialRoutes.Add(route);
                    MaxVolulme.Add(MaxSpeed);
                }

            // Ищем минимум, на скольких маршрутах придётся распределить заказ
            int complete = 0;
            List<Route> SelectedRoutes = [];
            List<int> SelectedRoutesVolume = [];
            while (complete < request.Count * request.Product.Size)
            {
                int i = MaxVolulme.IndexOf(MaxVolulme.Max());
                complete += MaxVolulme[i];

                SelectedRoutes.Add(PotencialRoutes[i]);
                SelectedRoutesVolume.Add(MaxVolulme[i]);

                MaxVolulme.RemoveAt(i);
                PotencialRoutes.RemoveAt(i);
            }

            // Добавляем в план
            for (int i = 0; i < PotencialRoutes.Count; ++i)
                Plan.Add(new()
                {
                    ProductId = request.ProductId,
                    Size = SelectedRoutesVolume[i],
                    RouteId = SelectedRoutes[i].Id,
                    ComingSoon = request.DateOfDesiredComplete,
                    Product = request.GetProduct!,
                    IsFabricating = false
                });

            // Обновляем сроки
            Plan = [.. Plan.OrderBy(p => p.ComingSoon)];

            // Стартуем (по возможности)
            SavePlan();
            StartPlan();
        }

        /// <summary>Запустить процесс по плану, если имеется такая возможность</summary>
        private void StartPlan()
        {
            for (int PlanIndex = 0; PlanIndex < Plan.Count; ++PlanIndex)
                if (!Plan[PlanIndex].IsFabricating)
                {
                    bool find = false;
                    for (int RouteIndex = 0; RouteIndex < Routes.Count; ++RouteIndex)
                        if (Routes[RouteIndex].Id == Plan[PlanIndex].RouteId)
                        {
                            find = true;
                            if (Routes[RouteIndex].ReadyToContinue)
                                Routes[RouteIndex].Start(Plan[PlanIndex], out _);
                            break;
                        }
                    if (!find)
                    {
                        UpdatePlan();
                        PlanIndex = -1;
                    }
                }
        }

        /// <summary>Обновить производственный план</summary>
        private void UpdatePlan()
        {
            /*// Чего и сколько уже в производстве
            Plan.ForEach(p =>
            {

            });*/
            SavePlan();
        }

        /// <summary>Сохранить план</summary>
        public void SavePlan()
        {
            Factory factory = context.Factories
                .Where(f => f.Id == Id)
                .Include(f => f.Plan)
                .FirstOrDefault() ?? throw new Exception("Завод не найден");
            factory.Plan = Plan;
            context.SaveChanges();
        }
        #endregion
        #endregion
    }
}