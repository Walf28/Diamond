using Diamond.Database;
using Diamond.Models.Orders;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diamond.Models.Factory
{
    public class Factory
    {
        #region Поля
        #region Обычные
        public int Id { get; set; } // Номер объекта в БД

        [Required(ErrorMessage = "Название обязательно")]
        [StringLength(50, ErrorMessage = "Название должно быть не длиннее 50 символов")]
        public string Name { get; set; } = ""; // Название объекта

        /*// Эти два - это суммарно, сколько и чего надо произвести
        public List<int> ProductsCommonId { get; set; } = []; // Что имеется в очереди
        public List<int> ProductsCommonSize { get; set; } = []; // Сколько этого надо произвести*/
        #endregion

        #region Ссылочные
        [NotMapped]
        private readonly DB context = new();
        public List<Part> Plan { get; set; } = []; // Это план - что и где производим
        public Warehouse Warehouse { get; set; } = new();
        public List<Route> Routes { get; set; } = [];
        public List<Region> Regions { get; set; } = [];
        public List<Order> Orders { get; set; } = [];
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
                .Include(f => f.Orders.Where(r => r.Status == OrderStatus.FABRICATING))
                .FirstOrDefault();
            if (factory == null)
                return;
            Routes = factory.Routes;
            Regions = factory.Regions;
            Orders = factory.Orders;
        }
        #endregion

        #region Информационные
        /// <summary>
        /// Найти все маршруты
        /// </summary>
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

        /// <summary>
        /// Найти неиспользуемые маршруты
        /// </summary>
        public List<Route> FindUnusingRoutes()
        {
            if (Regions.Count == 0)
                Regions = [.. context.Regions
                    .AsNoTracking()
                    .Where(r => r.FactoryId == Id)
                    .Include(r => r.RegionsParents)
                    .Include(r => r.RegionsChildrens)];
            if (Routes.Count == 0)
                Routes = [.. context.Routes
                    .AsNoTracking()
                    .Where(r => r.FactoryId == Id)
                    .Include(r=>r.Regions).ThenInclude(r => r.RegionsParents)
                    .Include(r=>r.Regions).ThenInclude(r => r.RegionsChildrens)];

            List<Route> routes = [];
            foreach (var r in Regions)
            {
                if (r.RegionsParents.Count > 0 || r.RegionsChildrens.Count == 0)
                    continue;
                List<Route> ThisRoutes = FindRouteFor(r);

                // Нахождение используемых маршрутов
                for (int i = 0; i < ThisRoutes.Count; ++i)
                    foreach (var UsedRoute in Routes)
                        if (ThisRoutes[i].RegionsRoute.SequenceEqual(UsedRoute.RegionsRoute))
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

        /// <summary>
        /// Найти маршруты, начинающиеся с данного участка
        /// </summary>
        private List<Route> FindRouteFor(Region region, List<Route>? routes = null, List<Region>? list = null)
        {
            list ??= [];
            list.Add(region);
            routes ??= [];

            // Конец
            if (region.RegionsChildrens.Count == 0)
                routes.Add(new() 
                {
                    Regions = list, 
                    RegionsRoute = list.Select(r => r.Id).ToList(),
                    Factory = this
                });

            // Просмотр всех последующих участков
            foreach (var children in region.RegionsChildrens)
                routes = FindRouteFor(children, [.. routes], [.. list]);

            return routes;
        }

        /// <summary>
        /// Маршруты, которые могут завершить выполнение данного плана
        /// </summary>
        public List<Route> GetRoutesCanCompletePlan(Part part)
        {
            if (part.ProductId == 0)
                return [];
            List<Route> routes = Routes.Where(r => r.CanProduceProduct(part.Product.Id)).ToList();
            return routes;
        }

        /// <summary>
        /// На какие маршруты можно заменить маршрут из данного плана.
        /// </summary>
        public List<Route> GetRoutesToChangePlan(int partId)
        {
            // Проверка на существование такой партии
            Part? part = Plan.FirstOrDefault(p => p.Id == partId);
            if (part == null)
                return [];

            // Уберём из вариантов уже применяемый маршрут
            var allRoutes = GetRoutesCanCompletePlan(part);
            _ = allRoutes.RemoveAll(al => al.Id == part.RouteId);

            // Возвращаем всё, если партия ещё не начала движение по маршруту
            if (part.Region == null)
                return allRoutes; // Конец
            
            // Найдём маршруты, которые могут стать заменой данному, если надо
            List<Route> halfRoutes = FindRouteFor(part.Region);
            List<Route> resultRoutes = [];
            foreach (var variantRoute in halfRoutes)
                foreach (var route in allRoutes)
                    if (!resultRoutes.Contains(route) &&
                        variantRoute.RegionsRoute.All(vr => route.RegionsRoute.Contains(vr)))
                        resultRoutes.Add(route);
            return resultRoutes;
        }

        /// <summary>
        /// Может ли завод выполнить данный заказ.
        /// </summary>
        public bool CanFullCompleteOrder(Order order)
        {
            foreach(OrderPart op in order.OrderParts)
            {
                bool can = false;
                foreach (var r in Routes)
                    if (r.CanProduceProduct(op.Product.Id))
                    {
                        can = true;
                        break;
                    }
                if (!can)
                    return false;
            }
            return true;
        }
        #endregion

        #region Заявки
        /// <summary>
        /// Добавить заявку
        /// </summary>
        public void AddRequest(int requestId)
        {
            Order? request = context.Orders
                .AsNoTracking()
                .Where(r => r.Id == requestId)
                .Include(r => r.OrderParts).ThenInclude(op=> op.Product).ThenInclude(p => p.ProductGroup)
                .FirstOrDefault() ?? throw new Exception("Заказ не найден");
            request.DateOfAcceptance = DateTime.UtcNow;
            request.FactoryId = Id;
            request.Status = OrderStatus.FABRICATING;

            // Добавление заявки в список заявок
            Orders.Add(request);
            AddToPlan(request);
        }
        #endregion

        #region Планирование производства
        /// <summary>
        /// Добавить в производственный план
        /// </summary>
        private void AddToPlan(Order request)
        {
            foreach (var orderPart in request.OrderParts)
            {
                // Добавляем в общий план
                int productSize = orderPart.Product.Size;
                int fullSize = orderPart.Count * productSize;

                // Пытаемся запихнуть в свободное место в плане, если такое найдётся.
                List<Part> PlanOuttime = [];
                for (int i = 0; i < Plan.Count && fullSize > 0; ++i)
                {
                    // Первичная проверка, можно ли произвести по данному плану данную продукцию
                    if (Plan[i].Status != PartStatus.AWAIT_CONFIRMATION || Plan[i].ProductId != orderPart.PackageId)
                        continue;

                    // Проверка с подсчётами, есть ли место для дополнения плана
                    // volumeCountInRoute - количество продукции, которое может быть произведено по маршруту из этого плана
                    // volumeSizeInRoute - общий объём сколько можно отправить в маршрут по данному плану
                    int volumeCountInRoute = Plan[i].Route.GetMaxVolumeCountProduct(orderPart.PackageId);
                    int volumeSizeInRoute = volumeCountInRoute * productSize;
                    if (volumeSizeInRoute <= Plan[i].Size)
                        continue;

                    // Если невозможно выполнить в назначенный срок вместе с этим планом, то просто запомним его на всякий случай
                    if (Plan[i].ComingSoon > request.DateOfDesiredComplete)
                    {
                        PlanOuttime.Add(Plan[i]);
                        continue;
                    }

                    // Дополняем наконец-то
                    int PlanAndSize = Plan[i].Size + fullSize;
                    if (PlanAndSize > volumeSizeInRoute)
                    {
                        fullSize -= volumeSizeInRoute - Plan[i].Size;
                        Plan[i].Size = volumeSizeInRoute;
                    }
                    else
                    {
                        Plan[i].Size = PlanAndSize;
                        fullSize = 0;
                        break;
                    }
                }
                if (fullSize == 0)
                    return;

                // Ищем, на каких маршрутах возможно произвести товар
                List<Route> PotencialRoutes = [];
                foreach (var route in Routes)
                    if (!route.IsHaveDowntimeRegion() && 
                        route.CanProduceProduct(orderPart.Product.ProductId) && 
                        route.GetMaxVolumeCountProduct(orderPart.PackageId) > 0)
                        PotencialRoutes.Add(route);
                if (PotencialRoutes.Count == 0)
                    throw new Exception("Нет маршрутов, способных выполнить заказ");

                // Заполняем план
                while (fullSize > 0)
                {
                    int fastestRouteId = PotencialRoutes[0].Id;
                    DateTime dateTimeOfFastestRoute = DateTime.UtcNow.AddMinutes(fastestRouteId);
                    for (int i = 1; i < PotencialRoutes.Count; ++i)
                    {
                        DateTime dt = DateTime.UtcNow.AddMinutes(PotencialRoutes[i].GetTimeToCompleteFullPlan());
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
                        int idIndex = Routes.FindIndex(r => r.Id == fastestRouteId);
                        int materialId = orderPart.Product.ProductGroup.MaterialId;
                        int volumeProductCount = Routes[idIndex].GetMaxVolumeCountProduct(orderPart.PackageId);
                        int needCount = fullSize / productSize;
                        int volumeMaterialSize = volumeProductCount * orderPart.Count;
                        Plan.Add(new()
                        {
                            //Size = fullSize <= volumeMaterialSize ? fullSize : volumeMaterialSize,
                            Size = needCount <= volumeProductCount ? needCount * productSize : volumeProductCount * productSize,
                            ComingSoon = request.DateOfDesiredComplete,
                            Factory = this,
                            Route = Routes[idIndex],
                            Product = orderPart.Product,
                            FactoryId = Id,
                            RouteId = fastestRouteId,
                            ProductId = orderPart.PackageId,
                            MaterialId = materialId,
                            Status = PartStatus.AWAIT_CONFIRMATION,
                            Order = request
                        });
                        Routes[idIndex].Part.Add(Plan[^1]);
                        //fullSize = fullSize <= volumeMaterialSize ? 0 : fullSize - volumeMaterialSize;
                        fullSize = needCount <= volumeProductCount ? 0 : fullSize - Plan[^1].Size;
                    }
                    else
                    {
                        // Определим индекс из плана, где план выполняется раньше всех
                        Part po = PlanOuttime.First(po => po.ComingSoon == minTimePlan);
                        int index = Plan.FindIndex(p => p.Id == po.Id);

                        // Определим, какой будет размер у плана и максимальный объём
                        int PlanAndSize = Plan[index].Size + fullSize; // Общий объём
                        int volumeSizeInRoute = Plan[index].Route.GetMaxVolumeCountProduct(orderPart.Product.ProductGroup.MaterialId) * productSize; // Максимальный объём

                        // Дополняем наконец-то
                        if (PlanAndSize > volumeSizeInRoute)
                        {
                            fullSize -= volumeSizeInRoute - Plan[index].Size;
                            Plan[index].Size = volumeSizeInRoute;
                        }
                        else
                        {
                            Plan[index].Size = PlanAndSize;
                            fullSize = 0;
                            break;
                        }

                        // Убираем из списка
                        PlanOuttime.Remove(po);
                    }
                }
            }

            // Сохраняем изменения, чтобы получить Id
            Server.Save();
        }

        /// <summary>
        /// Добавить в производственный план
        /// </summary>
        private void ChangePlan(int planId, List<Part> plans)
        {
            int pIndex = Plan.FindIndex(p => p.Id == planId);
            if (pIndex < 0)
                return;

            Plan.RemoveAt(pIndex);
            Plan.AddRange(plans);
            UpdatePlan();

            Server.Save();
        }

        /// <summary>
        /// Запустить процесс по плану, если имеется такая возможность
        /// </summary>
        public void StartPlan()
        {
            UpdatePlan();
            for (int PlanIndex = 0; PlanIndex < Plan.Count; ++PlanIndex)
                if (Plan[PlanIndex].Status == PartStatus.QUEUE && !Plan[PlanIndex].Route.IsHaveDowntimeRegion())
                    Plan[PlanIndex].Route.Start(Plan[PlanIndex]);
        }

        /// <summary>
        /// Обновить производственный план
        /// </summary>
        public void UpdatePlan()
        {
            // Тут в плане было полностью обновление плана, но пусть пока будет хоть что-то
            Plan = [.. Plan.OrderBy(p => p.ComingSoon)];
            return;
        }

        /// <summary>
        /// Выполнение плана было завершено
        /// </summary>
        public void CompletePlan(int planId)
        {
            int planIndex = Plan.FindIndex(p => p.Id == planId);
            if (planIndex < 0)
                return;

            Warehouse ??= new()
            {
                FactoryId = Id,
                Factory = this
            };

            Plan[planIndex].Status = PartStatus.DONE;
            Warehouse.AddProduct(Plan[planIndex], true);
            Plan.RemoveAt(planIndex);
        }

        /*/// <summary>
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
        }*/
        #endregion

        #region Управление
        /// <summary>
        /// Запустить все участки
        /// </summary>
        public void LaunchAllRegions()
        {
            for (int i = 0; i < Regions.Count; ++i)
                Regions[i].Launch();
        }

        /// <summary>
        /// Обновить все маршруты
        /// </summary>
        public void UpdateAllRoutes()
        {
            // Удаление "сломанных" маршрутов
            Routes.RemoveAll(r => !r.IsWorking);

            // Добавление новых маршрутов
            List<Route> UnusingRoutes = FindUnusingRoutes();
            Routes.AddRange(UnusingRoutes);
        }
        #endregion

        #region Статические и переопределяющие
        public override string ToString()
        {
            return Name;
        }
        #endregion
        #endregion
    }
}