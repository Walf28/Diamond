using Diamond.Database;
using Diamond.Models;
using Diamond.Models.Orders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Diamond.Controllers
{
    public class OrderController(DB context) : Controller
    {
        private readonly DB context = context;

        #region Отображение
        public IActionResult List()
        {
            List<Order> orders = [.. context.Orders
                .AsNoTracking()
                .Include(r => r.OrderParts).ThenInclude(op=>op.Product).ThenInclude(p => p.ProductGroup)
                .OrderBy(r => r.Id)];
            return View(orders);
        }
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Products = context.Package
                .AsNoTracking()
                .Include(ps => ps.ProductGroup)
                .ToList();
            return View();
        }
        [HttpGet]
        public IActionResult Detail(int? requestId)
        {
            if (requestId == null)
            {
                NotFound();
                return RedirectToAction(nameof(List));
            }

            Order? request = context.Orders
                .AsNoTracking()
                .Where(r => r.Id == requestId)
                .Include(r => r.Factory)
                .Include(r => r.OrderParts).ThenInclude(p => p.Product).ThenInclude(ps => ps.ProductGroup)
                .FirstOrDefault();
            if (request == null)
            {
                NotFound();
                return RedirectToAction(nameof(List));
            }
            if (request.Factory == null)
                ViewBag.Factories = Server.Factories.Values.ToList();

            return View(request);
        }
        #endregion

        #region Управление
        [HttpPost]
        public IActionResult Create(Order request)
        {
            request.DateOfReceipt = DateTime.UtcNow;
            request.OrderParts.ForEach(op =>
            {
                op.Product = context.Package.Where(ps => ps.Id == op.ProductId).First();
            });
            request.DateOfDesiredComplete = DateTime.SpecifyKind(request.DateOfDesiredComplete, DateTimeKind.Utc);

            context.Orders.Add(request);
            context.SaveChanges();
            return RedirectToAction(nameof(List));
        }
        [HttpPost]
        public IActionResult Detail(Order request)
        {
            Server.AddRequest(request);
            Server.Save();
            return RedirectToAction(nameof(List));
        }
        #endregion
    }
}