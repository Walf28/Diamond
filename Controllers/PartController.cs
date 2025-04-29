using Diamond.Database;
using Diamond.Models;
using Diamond.Models.Factory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Diamond.Controllers
{
    public class PartController(DB context) : Controller
    {
        private readonly DB context = context;

        #region Отображение
        [HttpGet]
        public IActionResult List(int Id)
        {
            ViewBag.FactoryName = context.Factories.AsNoTracking().First(f => f.Id == Id).Name;
            ViewBag.FactoryId = Id;
            return View(context.Plans
                .AsNoTracking()
                .Where(p => p.FactoryId == Id)
                .Include(p => p.Factory).ThenInclude(f => f.Routes)
                .Include(p => p.Route)
                .Include(p => p.Region)
                .Include(p => p.Product)
                .ToList());
        }
        [HttpGet]
        public IActionResult Create(int factoryId)
        {
            ViewBag.Products = context.ProductsSpecific.AsNoTracking()
                                                       .Include(ps => ps.ProductGroup)
                                                       .ToList();
            ViewBag.Routes = context.Routes.AsNoTracking()
                                           .Where(r => r.FactoryId == factoryId)
                                           .ToList();
            ViewBag.FactoryId = factoryId;
            return View();
        }
        [HttpGet]
        public IActionResult Edit(int Id)
        {
            Part plan = context.Plans
                .AsNoTracking()
                .Include(p => p.Factory).ThenInclude(f => f.Routes).ThenInclude(r => r.Regions).ThenInclude(r => r.Materials)
                .Include(p => p.Route).ThenInclude(r => r.Regions)
                .Include(p => p.Region)
                .First(p => p.Id == Id);
            ViewBag.ProductName = context.ProductsSpecific.Include(ps => ps.ProductGroup).First(p => p.Id == plan.ProductId).ToString();

            // Выискиваем маршруты, на которые можно заменить данный
            ViewBag.Routes = context.Factories
                .Include(f => f.Plan)
                .Include(f => f.Routes).ThenInclude(r => r.Regions).ThenInclude(r => r.Downtime)
                .Include(f => f.Regions).ThenInclude(r => r.Materials)
                .Include(f => f.Regions).ThenInclude(r=>r.Downtime)
                .First(f => f.Id == plan.FactoryId).GetRoutesToChangePlan(plan.Id);

            return View(plan);
        }
        #endregion

        #region Управление
        [HttpPost]
        public IActionResult Create(Part plan)
        {
            if (plan.RouteId != 0 && plan.ProductId != 0 && plan.Size > 0)
            {
                plan.Status = PlanStatus.QUEUE;
                plan.ComingSoon = plan.ComingSoon.ToUniversalTime();
                Server.PlanCreate(plan);
            }
            return RedirectToAction(nameof(List), new { Id = plan.FactoryId });
        }
        [HttpPost]
        public IActionResult Edit(Part plan)
        {
            Server.PlanEditRoute(plan);
            return RedirectToAction(nameof(List), new { Id = plan.FactoryId });
        }
        
        public IActionResult Delete(int Id)
        {
            int fId = context.Plans.AsNoTracking().First(p => p.Id == Id).FactoryId;
            Server.PlanDelete(Id);
            return RedirectToAction(nameof(List), new { Id = fId });
        }
        public IActionResult Approve(int Id)
        {
            int fId = context.Plans.AsNoTracking().First(p => p.Id == Id).FactoryId;
            Server.PlanApprove(Id);
            return RedirectToAction(nameof(List), new { Id = fId });
        }
        public IActionResult ApproveAll(int Id)
        {
            Server.PlanApproveAll(Id);
            return RedirectToAction(nameof(List), new { Id = Id });
        }
        public IActionResult Await(int Id)
        {
            int fId = context.Plans.AsNoTracking().First(p => p.Id == Id).FactoryId;
            Server.PlanAwait(Id);
            return RedirectToAction(nameof(List), new { Id = fId });
        }
        #endregion

        #region JS запросы
        [HttpGet]
        public IActionResult GetRoutesAndMaterailId(int factoryId, int productId)
        {
            List<Models.Factory.Route> routes = [.. context.Routes
                .AsNoTracking()
                .Where(r => r.FactoryId == factoryId)];
            List<Models.Factory.Route> routesAllow = [];
            foreach (var route in routes)
                if (route.CanProduceProduct(productId))
                    routesAllow.Add(route);
            return Json(new
            {
                routesId = routesAllow.Select(r => r.Id),
                routesContent = routesAllow.Select(r => r.GetContent),
                materialId = context.ProductsSpecific
                    .Include(ps => ps.ProductGroup)
                    .First(ps => ps.Id == productId)
                    .ProductGroup.MaterialId
            });
        }
        [HttpGet]
        public IActionResult GetVolumeRoute(int routeId, int productId)
        {
            int materialId = context.ProductsSpecific
                .AsNoTracking()
                .Include(ps => ps.ProductGroup)
                .First(ps => ps.Id == productId)
                .ProductGroup.MaterialId;
            return Json(new
            {
                maxSize = context.Routes
                    .AsNoTracking()
                    .Include(r => r.Regions).ThenInclude(r => r.Materials)
                    .First(r => r.Id == routeId)
                    .GetMaxVolumeCountProduct(materialId)
            });
        }
        [HttpGet]
        public IActionResult GetResultTimeOnRoute(int routeId, int planId)
        {
            Part? plan = context.Plans.FirstOrDefault(p => p.Id == planId);
            Models.Factory.Route? route = context.Routes
                .Where(r => r.Id == routeId)
                .Include(r => r.Factory).ThenInclude(f => f.Plan)
                .Include(r => r.Factory).ThenInclude(f => f.Routes).ThenInclude(r => r.Part)
                .Include(r => r.Factory).ThenInclude(f => f.Routes).ThenInclude(r => r.Regions).ThenInclude(r => r.Materials)
                .Include(r => r.Factory).ThenInclude(f => f.Routes).ThenInclude(r => r.Regions).ThenInclude(r => r.Downtime)
                .FirstOrDefault();
            if (plan == null || route == null)
                return Json(new { });

            double totalMinutes = route.GetTimeToCompleteFullPlan() + route.GetTimeToCompletePart(plan);
            if (double.IsInfinity(totalMinutes))
                return Json(new { });
            double totalMilliseconds = TimeSpan.FromMinutes(totalMinutes).TotalMilliseconds;
            // Сколько потребуется часов
            int hours = (int)TimeSpan.FromMilliseconds(totalMilliseconds).TotalHours;
            totalMilliseconds = TimeSpan.FromMilliseconds(totalMilliseconds).Subtract(TimeSpan.FromHours(hours)).TotalMilliseconds;
            // Сколько потребуется минут
            int minutes = (int)TimeSpan.FromMilliseconds(totalMilliseconds).TotalMinutes;
            totalMilliseconds = TimeSpan.FromMilliseconds(totalMilliseconds).Subtract(TimeSpan.FromMinutes(minutes)).TotalMilliseconds;
            // Сколько потребуется секунд
            int seconds = (int)TimeSpan.FromMilliseconds(totalMilliseconds).TotalSeconds;
            totalMilliseconds = TimeSpan.FromMilliseconds(totalMilliseconds).Subtract(TimeSpan.FromSeconds(seconds)).TotalMilliseconds;
            return Json(new { hours, minutes, seconds });
        }
        #endregion
    }
}