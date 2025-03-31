using Diamond.Database;
using Diamond.Models;
using Diamond.Models.Factory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Diamond.Controllers
{
    public class PlanController(DB context) : Controller
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
            Plan plan = context.Plans
                .AsNoTracking()
                .Include(p => p.Route).ThenInclude(r => r.Regions)
                .Include(p => p.Region)
                .First(p => p.Id == Id);
            ViewBag.ProductName = context.ProductsSpecific.Include(ps => ps.ProductGroup).First(p => p.Id == plan.ProductId).ToString();

            // Выискиваем маршруты, на которые можно заменить данный
            ViewBag.Routes = context.Factories
                .Include(f => f.Plan)
                .Include(f => f.Routes).ThenInclude(r => r.Regions).ThenInclude(r => r.Downtime)
                .Include(f => f.Regions).ThenInclude(r => r.Materials)
                .Include(f => f.Regions)
                .First(f => f.Id == plan.FactoryId).GetRoutesToChangePlan(plan.Id);

            return View(plan);
        }
        #endregion

        #region Управление
        [HttpPost]
        public IActionResult Create(Plan plan)
        {
            plan.Status = PlanStatus.QUEUE;
            plan.ComingSoon = plan.ComingSoon.ToUniversalTime();
            Server.PlanCreate(plan);
            return RedirectToAction(nameof(List), new { Id = plan.FactoryId });
        }
        [HttpPost]
        public IActionResult Edit(Plan plan)
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
                    .GetMaxVolume(materialId)
            });
        }
        #endregion
    }
}