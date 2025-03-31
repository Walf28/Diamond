using Diamond.Database;
using Diamond.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Diamond.Controllers
{
    public class RequestController(DB context) : Controller
    {
        private readonly DB context = context;

        #region Отображение
        public IActionResult List()
        {
            List<Request> requests = [.. context.Requests
                .AsNoTracking()
                .Include(r => r.Product).ThenInclude(p => p.ProductGroup)
                .OrderBy(r => r.Id)];
            return View(requests);
        }
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Products = context.ProductsSpecific
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

            Request? request = context.Requests
                .AsNoTracking()
                .Where(r => r.Id == requestId)
                .Include(r => r.Factory)
                .Include(r => r.Product)
                .FirstOrDefault();
            if (request == null)
            {
                NotFound();
                return RedirectToAction(nameof(List));
            }
            if (request.Factory == null)
                ViewBag.Factories = context.Factories.AsNoTracking().ToList();

            return View(request);
        }
        #endregion

        #region Управление
        [HttpPost]
        public IActionResult Create(Request request)
        {
            request.DateOfReceipt = DateTime.UtcNow;
            request.Product = context.ProductsSpecific.Where(ps => ps.Id == request.ProductId).First();
            request.DateOfDesiredComplete = DateTime.SpecifyKind(request.DateOfDesiredComplete, DateTimeKind.Utc);

            context.Requests.Add(request);
            context.SaveChanges();
            return RedirectToAction(nameof(List));
        }
        [HttpPost]
        public IActionResult Detail(Request request)
        {
            Server.AddRequest(request);
            return RedirectToAction(nameof(List));
        }
        #endregion
    }
}