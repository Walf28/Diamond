using Diamond.Database;
using Diamond.Models;
using Diamond.Models.Factory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Diamond.Controllers
{
    public class FactoryController : Controller
    {
        private readonly DB context = new();

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
            var f = Server.Factories[Id!.Value];
            return View(f);
        }
        #endregion

        #region Управление
        public IActionResult Add(Factory factory)
        {
            context.Factories.Add(factory);
            context.SaveChanges();
            _ = Server.Factories.TryAdd(factory.Id, factory);
            return RedirectToAction(nameof(List));
        }
        public IActionResult Update(Factory factory)
        {
            context.Factories
                .Where(f => f.Id == factory.Id)
                .ExecuteUpdate(f => f.SetProperty(sp => sp.Name, factory.Name));
            context.SaveChanges();
            Server.Load();
            return RedirectToAction(nameof(List));
        }
        public IActionResult Delete(int Id)
        {
            context.Factories.Where(f => f.Id == Id).ExecuteDelete();
            context.SaveChanges();
            return RedirectToAction(nameof(List));
        }

        public IActionResult DeleteRegion(int ModelId, int RegionId)
        {
            context.Regions.Where(r => r.Id == RegionId).ExecuteDelete();
            context.SaveChanges();
            return UpdateAllRoutes(ModelId);
        }
        public IActionResult SetDowntime(int RegionId)
        {
            Server.DowntimeCreate(RegionId);
            return RedirectToAction(nameof(Edit), new
            {
                Id = Server.context.Regions.First(r => r.Id == RegionId).FactoryId
            });
        }
        public IActionResult UpdateAllRoutes(int id)
        {
            Server.Load();
            Factory f = Server.Factories[id];
            f.UpdateAllRoutes();
            context.Factories.Update(f);
            context.SaveChanges();
            Server.Load();
            return RedirectToAction(nameof(Edit), new { Id = id });
        }
        #endregion
    }
}