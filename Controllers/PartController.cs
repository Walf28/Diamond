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
            ViewBag.Orders = context.Orders.AsNoTracking()
                .Where(o => o.FactoryId == factoryId)
                .ToList();
            return View();
        }
        [HttpGet]
        public IActionResult Edit(int Id)
        {
            int factoryId = context.Plans.AsNoTracking().First(p=>p.Id == Id).FactoryId;
            Part part = Server.Factories[factoryId].Plan.First(p=>p.Id == Id);
            ViewBag.AllRoutes = Server.Factories[factoryId].Routes.Where(r=>r.CanProduceProduct(part.ProductId)).ToList();
            ViewBag.Routes = part.Region == null ? ViewBag.AllRoutes : part.Factory.Routes.Where(r => r.CanProduceProduct(part.ProductId)).ToList();
            ViewBag.ProductName = context.Package.Include(ps => ps.ProductGroup).First(p => p.Id == part.ProductId).ToString();
            ViewBag.Orders = context.Orders.AsNoTracking()
                .Where(o => o.FactoryId == factoryId)
                .ToList();
            
            return View(part);
        }
        [HttpGet]
        public IActionResult Gant(int factoryId)
        {
            ViewBag.FactoryId = factoryId;
            ViewBag.Parts = Server.Factories[factoryId].Plan.Where(p => p.Status != PartStatus.AWAIT_CONFIRMATION).ToList();
            return View();
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
                plan.Order = context.Orders.First(o => o.Id == plan.OrderId);
                
                context.Plans.Add(plan);
                context.SaveChanges();
                Server.Load();
                //Server.PlanCreate(plan);
            }
            return RedirectToAction(nameof(List), new { Id = plan.FactoryId });
        }
        [HttpPost]
        public IActionResult Edit(Part plan, int variantRoute, int selectedRoute0, int selectedRoute1)
        {
            Part p = context.Plans.First(p => p.Id == plan.Id);
            p.Order = context.Orders.First(o => o.Id == plan.OrderId);
            if (variantRoute == 0)
                p.Route = context.Routes.First(p => p.Id == selectedRoute0);
            else
                p.Route = context.Routes.First(p => p.Id == selectedRoute1);

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
            return RedirectToAction(nameof(List), new { Id });
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
            //var pgId = context.Package.Where(ps => ps.Id == productId).Select(ps => ps.ProductId).ToList()[0];
            List<Models.Factory.Route> routes = Server.Factories[factoryId].Routes.Where(r=>r.CanProduceProduct(productId)).ToList();
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
            Part? part = context.Plans.AsNoTracking().FirstOrDefault(p => p.Id == planId);
            Route? route = Server.Factories.Values.FirstOrDefault(f => f.Routes.FirstOrDefault(r => r.Id == routeId) != null)?.Routes.FirstOrDefault(r => r.Id == routeId);
            if (part == null || route == null)
                return Json(new { });

            //double totalMinutes = route.GetTimeToCompleteFullPlan() + route.GetTimeToCompletePart(part);
            double totalMinutes = route.GetTimeToCompleteFullPlan(part.ComingSoon);
            if (part.RouteId != route.Id && totalMinutes != double.PositiveInfinity)
                totalMinutes += route.GetTimeToCompletePart(part);
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