using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyCompany;

namespace Diamond.Controllers
{
    public class ProductController(DB context) : Controller
    {
        private readonly DB context = context;

        #region Отображение
        public IActionResult List()
        {
            return View(context.Products.AsNoTracking().ToList());
        }
        public IActionResult Create()
        {
            List<Material> materials = context.Materials.ToList();
            ViewBag.materials = materials;
            return View();
        }
        public IActionResult Edit(int? Id)
        {
            if (Id == null)
                NotFound();
            var product = context.Products
                .AsNoTracking()
                .FirstOrDefault(p => p.Id == Id);
            List<Material> materials = context.Materials.ToList();
            ViewBag.materials = materials;
            return View(product);
        }
        #endregion

        #region Управление
        public IActionResult Add(Product product)
        {
            product.Material = context.Materials.Where(m => m.Id == product.MaterialId).First();
            context.Products.Add(product);
            context.SaveChanges();
            return RedirectToAction(nameof(List));
        }
        public IActionResult Update(Product product)
        {
            context.Products
                .Where(m => m.Id == product.Id)
                .ExecuteUpdate(f => f
                .SetProperty(sp => sp.Name, product.Name)
                .SetProperty(sp => sp.Size, product.Size)
                .SetProperty(sp => sp.Price, product.Price)
                .SetProperty(sp => sp.MaterialId, product.MaterialId));
            context.SaveChanges();
            return RedirectToAction(nameof(List));
        }
        public IActionResult Delete(int Id)
        {
            context.Products.Where(m => m.Id == Id).ExecuteDelete();
            context.SaveChanges();
            return RedirectToAction(nameof(List));
        }
        #endregion
    }
}