using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyCompany;

namespace MyCompany.Controllers
{
    public class FactoryController(DB context) : Controller
    {
        private readonly DB context = context;

        #region Список/подробности
        public IActionResult List()
        {
            return View(context.Factories.AsNoTracking().ToList());
        }
        public IActionResult Edit(int? Id)
        {
            if (Id == null)
                return RedirectToAction(nameof(List));
            var f = context.Factories
                .AsNoTracking()
                .Include(f => f.Regions)
                .FirstOrDefault(f => f.Id == Id);
            return View(f);
        }
        #endregion

        #region Создание, обновление и удаление
        public IActionResult Create()
        {
            return View();
        }
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
        #endregion
    }
}