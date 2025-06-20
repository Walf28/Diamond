using Diamond.Database;
using Diamond.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Diamond.Controllers
{
    public class TechnologyController : Controller
    {
        private readonly DB context = new();

        #region Представление
        [HttpGet]
        public IActionResult List()
        {
            return View(context.ProductionStage.AsNoTracking().ToList());
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Edit(int? Id)
        {
            if (Id == null)
                NotFound();
            var t = context.ProductionStage
                .AsNoTracking()
                .FirstOrDefault(t => t.Id == Id);
            return View(t);
        }
        #endregion

        #region Управление
        [HttpPost]
        public IActionResult Create(ProductionStage technology)
        {
            context.ProductionStage.Add(technology);
            context.SaveChanges();
            return RedirectToAction(nameof(List));
        }
        [HttpPost]
        public IActionResult Edit(ProductionStage technology)
        {
            context.ProductionStage
                .Where(m => m.Id == technology.Id)
                .ExecuteUpdate(f => f.SetProperty(sp => sp.Name, technology.Name));
            context.SaveChanges();
            return RedirectToAction(nameof(List));
        }
        [HttpPost]
        public IActionResult Delete(int Id)
        {
            context.ProductionStage.Where(m => m.Id == Id).ExecuteDelete();
            context.SaveChanges();
            return RedirectToAction(nameof(List));
        }
        #endregion
    }
}