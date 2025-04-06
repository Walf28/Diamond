using Diamond.Database;
using Diamond.Models.Materials;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Diamond.Controllers
{
    public class MaterialController(DB context) : Controller
    {
        private readonly DB context = context;

        #region Представление
        public IActionResult List()
        {
            return View(context.Materials.AsNoTracking().ToList());
        }
        public IActionResult Create()
        {
            return View();
        }
        public async Task<IActionResult> Detail(int? Id)
        {
            if (Id == null)
                NotFound();
            var f = await context.Materials
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == Id);
            return View(f);
        }
        #endregion

        #region Управление
        public async Task<IActionResult> Add(Material material)
        {
            await context.Materials.AddAsync(material);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(List));
        }
        public async Task<IActionResult> Update(Material material)
        {
            await context.Materials
                .Where(m => m.Id == material.Id)
                .ExecuteUpdateAsync(f => f.SetProperty(sp => sp.Name, material.Name));
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(List));
        }
        public async Task<IActionResult> Delete(int Id)
        {
            await context.Materials.Where(m => m.Id == Id).ExecuteDeleteAsync();
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(List));
        }
        #endregion
    }
}