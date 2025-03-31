using Diamond.Database;
using Diamond.Models;
using Diamond.Models.Factory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Diamond.Controllers
{
    public class FactoryController(DB context) : Controller
    {
        private readonly DB context = context;

        #region Отображение
        public IActionResult List()
        {
            return View(context.Factories.AsNoTracking().ToList());
        }
        public IActionResult Create()
        {
            return View();
        }
        public IActionResult Edit(int? Id)
        {
            if (Id == null)
                return RedirectToAction(nameof(List));
            var f = context.Factories
                .Include(f => f.Regions.OrderBy(r => r.Id)).ThenInclude(r => r.Materials)
                .Include(f => f.Routes).ThenInclude(ps => ps.Regions).ThenInclude(ps => ps.Downtime)
                .Include(f => f.Requests.Where(r => r.Status == RequestStatus.FABRICATING))
                .Include(f => f.Plan).ThenInclude(p => p.Product).ThenInclude(ps => ps.ProductGroup)
                .Include(f => f.Plan).ThenInclude(p => p.Route).ThenInclude(ps => ps.Regions)
                .Include(f => f.Plan).ThenInclude(p => p.Region)
                .First(f => f.Id == Id);

            // Наименование продукции
            ViewBag.PSNames = context.Factories
                .AsNoTracking()
                .Include(f => f.Requests.Where(r => r.Status == RequestStatus.FABRICATING))
                .ThenInclude(r => r.Product)
                .ThenInclude(ps => ps.ProductGroup)
                .First()
                .Requests.Select(r => r.Product)
                .Select(p => p.ProductGroup)
                .Select(pg => pg.Name).ToList();
            return View(f);
        }
        #endregion

        #region Управление
        public async Task<IActionResult> Add(Factory factory)
        {
            await context.Factories.AddAsync(factory);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(List));
        }
        public async Task<IActionResult> Update(Factory factory)
        {
            await context.Factories
                .Where(f => f.Id == factory.Id)
                .ExecuteUpdateAsync(f => f.SetProperty(sp => sp.Name, factory.Name));
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(List));
        }
        public async Task<IActionResult> Delete(int Id)
        {
            await context.Factories.Where(f => f.Id == Id).ExecuteDeleteAsync();
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(List));
        }

        public IActionResult DeleteRegion(int ModelId, int RegionId)
        {
            context.Regions.Where(r => r.Id == RegionId).ExecuteDelete();
            context.SaveChanges();
            return RedirectToAction(nameof(Edit), new { Id = ModelId });
        }
        public IActionResult SetDowntime(int RegionId)
        {
            Server.DowntimeCreate(RegionId);
            return RedirectToAction(nameof(Edit), new
            {
                Id = context.Regions.First(r => r.Id == RegionId).FactoryId
            });
        }
        #endregion
    }
}