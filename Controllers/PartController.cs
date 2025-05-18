using Diamond.Database;
using Diamond.Models;
using Diamond.Models.Factory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Route = Diamond.Models.Factory.Route;

namespace Diamond.Controllers
{
    public class PartController(DB context) : Controller
    {
        private readonly DB context = context;

        #region Отображение
        [HttpGet]
        public IActionResult List(int Id)
        {
            ViewBag.FactoryName = Server.Factories[Id].Name; // context.Factories.AsNoTracking().First(f => f.Id == Id).Name;
            ViewBag.FactoryId = Id;
            /*return View(context.Plans
                .AsNoTracking()
                .Where(p => p.FactoryId == Id)
                .Include(p => p.Factory).ThenInclude(f => f.Routes)
                .Include(p => p.Route)
                .Include(p => p.Region).ThenInclude(pgId=>pgId!.Materials)
                .Include(p => p.Product)
                .ToList());*/
            return View(Server.Factories[Id].Plan);
        }
        [HttpGet]
        public IActionResult Create(int factoryId)
        {
            ViewBag.Products = context.Package.AsNoTracking()
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
            /*Part part = context.Plans
                .AsNoTracking()
                .Include(p => p.Factory).ThenInclude(f => f.Routes).ThenInclude(r => r.Regions).ThenInclude(r => r.Materials)
                .Include(p => p.Route).ThenInclude(r => r.Regions).ThenInclude(r => r.RegionsParents)
                .Include(p => p.Route).ThenInclude(r => r.Regions).ThenInclude(r => r.RegionsChildrens)
                .Include(p => p.Region).ThenInclude(r => r!.RegionsParents)
                .Include(p => p.Region).ThenInclude(r => r!.RegionsChildrens)
                .Include(p => p.Region).ThenInclude(r => r!.Routes)
                .First(p => p.Id == Id);*/
            int factoryId = context.Plans.AsNoTracking().First(p=>p.Id == Id).FactoryId;
            Part part = Server.Factories[factoryId].Plan.First(p=>p.Id == Id);
            ViewBag.ProductName = context.Package.Include(ps => ps.ProductGroup).First(p => p.Id == part.ProductId).ToString();

            // Выискиваем маршруты, на которые можно заменить данный
            /*if (part.Region != null)
                ViewBag.Routes = part.Region.Routes.Where(r=>r.CanProduceProduct(part.ProductId) && r.Id != part.RouteId).ToList();
            else
                ViewBag.Routes = part.Factory.Routes.Where(r=>r.CanProduceProduct(part.ProductId) && r.Id != part.RouteId).ToList();*/
            /*context.Factories
                .Include(f => f.Plan)
                .Include(f => f.Routes).ThenInclude(r => r.Regions).ThenInclude(r => r.Downtime)
                .Include(f => f.Regions).ThenInclude(r => r.Materials)
                .Include(f => f.Regions).ThenInclude(r=>r.Downtime)
                .Include(f => f.Regions).ThenInclude(r=>r.RegionsChildrens)
                .Include(f => f.Regions).ThenInclude(r=>r.RegionsParents)
                .First(f => f.Id == part.FactoryId).GetRoutesToChangePlan(part.Id);*/

            return View(part);
        }
        #endregion

        #region Управление
        [HttpPost]
        public IActionResult Create(Part plan)
        {
            if (plan.RouteId != 0 && plan.ProductId != 0 && plan.Size > 0)
            {
                plan.Status = PartStatus.QUEUE;
                plan.ComingSoon = plan.ComingSoon.ToUniversalTime();
                Server.PlanCreate(plan);
            }
            return RedirectToAction(nameof(List), new { Id = plan.FactoryId });
        }
        [HttpPost]
        public IActionResult Edit(Part plan)
        {
            Part p = context.Plans.First(p => p.Id == plan.Id);
            p.Route = context.Routes.First(p => p.Id == plan.RouteId);
            context.SaveChanges();
            Server.Load();
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
            var pgId = context.Package.Where(ps => ps.Id == productId).Select(ps => ps.ProductId).ToList()[0];
            List<Models.Factory.Route> routes = Server.Factories[factoryId].Routes.Where(r=>r.CanProduceProduct(pgId)).ToList();
            return Json(new
            {
                routesId = routes.Select(r => r.Id),
                routesContent = routes.Select(r => r.GetContent),
                materialId = context.Package
                    .Include(ps => ps.ProductGroup)
                    .First(ps => ps.Id == productId)
                    .ProductGroup.MaterialId
            });
        }
        [HttpGet]
        public IActionResult GetVolumeRoute(int routeId, int productId)
        {
            int materialId = context.Package
                .AsNoTracking()
                .Include(ps => ps.ProductGroup)
                .First(ps => ps.Id == productId)
                .ProductGroup.MaterialId;
            return Json(new
            {
                maxSize = context.Routes
                    .AsNoTracking()
                    .Include(r => r.Regions).ThenInclude(r => r.Materials)
                    .Include(r => r.Regions).ThenInclude(r => r.RegionsChildrens)
                    .First(r => r.Id == routeId)
                    .GetMaxVolumeCountProduct(materialId)
            });
        }
        [HttpGet]
        public IActionResult GetResultTimeOnRoute(int routeId, int planId)
        {
            Part? plan = context.Plans.AsNoTracking().FirstOrDefault(p => p.Id == planId);
            Route? route = Server.Factories.Values.FirstOrDefault(f => f.Routes.FirstOrDefault(r => r.Id == routeId) != null)?.Routes.FirstOrDefault(r => r.Id == routeId);
            if (plan == null || route == null)
                return Json(new { });

            //double totalMinutes = route.GetTimeToCompleteFullPlan() + route.GetTimeToCompletePart(plan);
            double totalMinutes = route.GetTimeToCompleteFullPlan(plan.ComingSoon);
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