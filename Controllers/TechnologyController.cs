using Diamond.Database;
using Diamond.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Diamond.Controllers
{
    public class TechnologyController : Controller
    {
        private readonly DB context = Server.context;

        #region Представление
        [HttpGet]
        public IActionResult List()
        {
            return View(context.Technologies.AsNoTracking().ToList());
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
            var t = context.Technologies
                .AsNoTracking()
                .FirstOrDefault(t => t.Id == Id);
            return View(t);
        }
        #endregion

        #region Управление
        [HttpPost]
        public IActionResult Create(Technology technology)
        {
            context.Technologies.Add(technology);
            context.SaveChanges();
            return RedirectToAction(nameof(List));
        }
        [HttpPost]
        public IActionResult Edit(Technology technology)
        {
            context.Technologies
                .Where(m => m.Id == technology.Id)
                .ExecuteUpdate(f => f.SetProperty(sp => sp.Name, technology.Name));
            context.SaveChanges();
            return RedirectToAction(nameof(List));
        }
        [HttpPost]
        public IActionResult Delete(int Id)
        {
            context.Technologies.Where(m => m.Id == Id).ExecuteDelete();
            context.SaveChanges();
            return RedirectToAction(nameof(List));
        }
        #endregion
    }
}