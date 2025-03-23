using Diamond.Database;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diamond.Models.Factory
{
    public class Factory : FactoryObject
    {
        #region Поля
        #region Обычные
        // Это план - что и где производим
        public List<Plan> Plan { get; set; } = [];

        // Эти два - это суммарно, сколько и чего надо произвести
        public List<int> ProductsCommonId { get; set; } = []; // Что имеется в очереди
        public List<int> ProductsCommonSize { get; set; } = []; // Сколько этого надо произвести
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
                .Where(f => f.Id == Id)
                .Include(f => f.Routes).ThenInclude(r => r.Regions)
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

        /// <summary>
        /// Сколько времени необходимо маршруту, чтобы завершить план (с учётом уже имеющихся планов и пересекающихся маршрутов).
        /// Если маршрута не существует, вернётся 0.
        /// </summary>
        public double NeedTimeForRoute(int routeId)
        {
            // Поиск маршрута
            double Time = 0;
            Route? route = Routes.Where(r => r.Id == routeId).FirstOrDefault();
            if (route == null)
                return Time;

            // Подобор интересущих участков
            List<Region> regions = route.Regions;
            foreach (var region in regions)
                Time += region.GetTime();

            return Time;
        }
        #endregion

        #region Заявки
        /// <summary>Добавить заявку</summary>
        public void AddRequest(int requestId)
        {
            Request? request = context.Requests
                .AsNoTracking()
                .Where(r => r.Id == requestId)
                .Include(r => r.Product).ThenInclude(p => p.ProductGroup)
                .FirstOrDefault() ?? throw new Exception("Заказ не найден");
            request.DateOfAcceptance = DateTime.UtcNow;
            request.FactoryId = Id;
            request.Status = RequestStatus.FABRICATING;

            // Добавление заявки в список заявок
            Requests.Add(request);
            AddToPlan(request);
        }
        #endregion

        #region Планирование производства
        /// <summary>Добавить в производственный план</summary>
        private void AddToPlan(Request request)
        {
            // Добавляем в общий план
            int fullSize = request.Count * request.Product.Size;
            AddInCommonPlan(request.ProductId, fullSize);

            // Пытаемся запихнуть в свободное место в плане, если такое найдётся
            List<Plan> PlanOuttime = [];
            for (int i = 0; i < Plan.Count && fullSize > 0; ++i)
                if (!Plan[i].IsFabricating && Plan[i].ProductId == request.ProductId) // Ищем свободное местечко в тех местах, где возможно подкорректировать план
                {
                    // Если невозможно выполнить в назначенный срок вместе с этим планом, то запомним его на всякий случай
                    if (Plan[i].ComingSoon > request.DateOfDesiredComplete)
                    {
                        PlanOuttime.Add(Plan[i]);
                        continue;
                    }
                    
                    int MaxVolumeInRoute = Plan[i].Route.GetMaxVolume(Plan[i].GetMaterial!.Id);
                    if (MaxVolumeInRoute > Plan[i].Size)
                    {
                        int PlanAndSize = Plan[i].Size + fullSize;
                        if (PlanAndSize > MaxVolumeInRoute)
                        {
                            Plan[i].Size = MaxVolumeInRoute;
                            fullSize = PlanAndSize - MaxVolumeInRoute;
                        }
                        else
                        {
                            Plan[i].Size = PlanAndSize;
                            fullSize = 0;
                        }
                    }
                }
            if (fullSize == 0)
                return;

            // Ищем, на каких маршрутах возможно произвести товар
            List<Route> PotencialRoutes = [];
            foreach (var route in Routes)
                if (route.CanProduceProduct(request.GetProductGroup!.Id))
                    PotencialRoutes.Add(route);
            if (PotencialRoutes.Count == 0)
                throw new Exception("Нет маршрутов, способных выполнить заказ");

            // Заполняем план
            while (fullSize > 0)
            {
                int fastestRouteId = PotencialRoutes[0].Id;
                DateTime dateTimeOfFastestRoute = DateTime.UtcNow.AddMinutes(NeedTimeForRoute(PotencialRoutes[0].Id));
                for (int i = 1; i < PotencialRoutes.Count; ++i) // 
                {
                    DateTime dt = DateTime.UtcNow.AddMinutes(NeedTimeForRoute(PotencialRoutes[i].Id));
                    if (dateTimeOfFastestRoute > dt)
                    {
                        dateTimeOfFastestRoute = dt;
                        fastestRouteId = PotencialRoutes[i].Id;
                    }
                }

                // Проверка, не будет ли лучше по времени дополнить уже отстающий по срокам план
                DateTime? minTimePlan = PlanOuttime.Count > 0 ? PlanOuttime.Min(p => p.ComingSoon) : null;
                if ((minTimePlan != null && dateTimeOfFastestRoute < minTimePlan) || minTimePlan == null)
                {
                    int idIndex = Routes.FindIndex(r=>r.Id == fastestRouteId);
                    int volume = Routes[idIndex].GetMaxVolume(request.Product.GetMaterial!.Id);
                    Plan.Add(new()
                    {
                        Size = fullSize <= volume ? fullSize : volume,
                        ComingSoon = request.DateOfDesiredComplete,
                        Factory = this,
                        FactoryId = Id,
                        Route = Routes[idIndex],
                        RouteId = fastestRouteId,
                        ProductId = request.Product.Id,
                    });
                    Routes[idIndex].Plan.Add(Plan[^1]);
                    fullSize = fullSize <= volume ? 0 : fullSize - volume;
                }
                else
                {
                    // Определим индекс из плана, где план выполняется раньше всех
                    Plan po = PlanOuttime.First(po => po.ComingSoon == minTimePlan);
                    int index = Plan.FindIndex(p => p.Id == po.Id);

                    // Определим, какой будет размер у плана и максимальный объём
                    int PlanAndSize = Plan[index].Size + fullSize; // Общий объём
                    int volume = Plan[index].Route.GetMaxVolume(request.GetProductGroup!.MaterialId); // Максимальный объём

                    // Расчёты
                    Plan[index].Size = PlanAndSize <= volume ? PlanAndSize : volume;
                    fullSize = PlanAndSize <= volume ? 0 : (PlanAndSize - volume);
                }
            }

            // Обновляем порядок по срокам
            Plan = [.. Plan.OrderBy(p => p.ComingSoon)];
            //StartPlan();
        }

        /// <summary>Запустить процесс по плану, если имеется такая возможность</summary>
        public void StartPlan()
        {
            UpdatePlan();
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
        public void UpdatePlan()
        {
            // Тут в плане было полностью обновление плана, но пусть пока будет хоть что-то
            Plan = [.. Plan.OrderBy(p => p.ComingSoon)];
            return;
        }

        /// <summary>
        /// Добавить в общий план
        /// </summary>
        private void AddInCommonPlan(int productId, int productSize)
        {
            bool find = false;
            for (int i = 0; i < ProductsCommonId.Count; ++i)
                if (ProductsCommonId[i] == productId)
                {
                    ProductsCommonSize[i] += productSize;
                    find = true;
                    break;
                }
            if (!find)
            {
                ProductsCommonId.Add(productId);
                ProductsCommonSize.Add(productSize);
            }
        }
        /// <summary>
        /// Добавить в общий план
        /// </summary>
        private void AddInCommonPlan(List<int> productId, List<int> productSize)
        {
            for (int pIndex = 0; pIndex < productId.Count; ++pIndex)
            {
                bool find = false;
                for (int pcIndex = 0; pcIndex < ProductsCommonId.Count; ++pcIndex)
                    if (ProductsCommonId[pcIndex] == productId[pIndex])
                    {
                        ProductsCommonSize[pcIndex] += productSize[pIndex];
                        find = true;
                        break;
                    }

                if (!find)
                {
                    ProductsCommonId.Add(productId[pIndex]);
                    ProductsCommonSize.Add(productSize[pIndex]);
                }
            }
        }

        /// <summary>
        /// Убрать из плана
        /// </summary>
        private void RemoveInCommonPlan(int productId, int productSize)
        {
            for (int pcIndex = 0; pcIndex < ProductsCommonId.Count; ++pcIndex)
                if (ProductsCommonId[pcIndex] == productId)
                {
                    ProductsCommonSize[pcIndex] -= productSize;
                    if (ProductsCommonSize[pcIndex] <= 0)
                    {
                        ProductsCommonId.RemoveAt(pcIndex);
                        ProductsCommonSize.RemoveAt(pcIndex);
                    }
                    return;
                }
        }
        #endregion
        #endregion
    }
}