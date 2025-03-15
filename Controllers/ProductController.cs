using Diamond.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Diamond.Controllers
{
    public class ProductController(DB context) : Controller
    {
        private readonly DB context = context;

        #region Группа продукции
        #region Отображение
        public IActionResult ListGroup()
        {
            return View(context.ProductsGroup.AsNoTracking().Include(pg => pg.Material).ToList());
        }
        [HttpGet]
        public IActionResult CreateGroup()
        {
            List<Material> materials = [.. context.Materials];
            ViewBag.materials = materials;
            return View();
        }
        [HttpGet]
        public IActionResult EditGroup(int? Id)
        {
            if (Id == null)
                NotFound();

            var product = context.ProductsGroup
                .AsNoTracking()
                .First(p => p.Id == Id);

            List<Material> materials = [.. context.Materials];
            ViewBag.materials = materials;

            return View(product);
        }
        #endregion

        #region Управление
        [HttpPost]
        public IActionResult CreateGroup(ProductGroup product)
        {
            context.ProductsGroup.Add(product);
            context.SaveChanges();
            return RedirectToAction(nameof(ListGroup));
        }
        [HttpPost]
        public IActionResult EditGroup(ProductGroup product)
        {
            ProductGroup? productGroup = context.ProductsGroup
                .Where(pg => pg.Id == product.Id)
                .FirstOrDefault();
            if (productGroup == null)
            {
                NotFound();
                return RedirectToAction(nameof(ListGroup));
            }

            productGroup.Name = product.Name;
            productGroup.MaterialId = product.MaterialId;

            context.SaveChanges();
            return RedirectToAction(nameof(ListGroup));
        }
        public IActionResult DeleteGroup(int? id)
        {
            if (id != null)
            {
                context.ProductsGroup.Where(pg => pg.Id == id).ExecuteDelete();
                context.SaveChanges();
            }
            return RedirectToAction(nameof(ListGroup));
        }
        #endregion
        #endregion

        #region Конкретная продукция
        #region Отображение
        public IActionResult ListSpecific(int? groupId)
        {
            if (groupId == null)
            {
                NotFound();
                return RedirectToAction(nameof(ListGroup));
            }
            return View(context.ProductsGroup
                .AsNoTracking()
                .Where(pg => pg.Id == groupId)
                .Include(pg => pg.ProductsSpecific)
                .First());
        }
        [HttpGet]
        public IActionResult CreateSpecific(int? groupId)
        {
            if (groupId == null)
            {
                NotFound();
                return RedirectToAction(nameof(ListGroup));
            }

            ProductSpecific ps = new() { ProductGroupId = (int)groupId };
            ps.ProductGroup = ps.GetProductGroup!;

            return View(ps);
        }
        [HttpGet]
        public IActionResult EditSpecific(int? productId)
        {
            if (productId == null)
            {
                NotFound();
                return RedirectToAction(nameof(ListGroup));
            }

            var product = context.ProductsSpecific
                .AsNoTracking()
                .Where(ps => ps.Id == productId)
                .Include(ps => ps.ProductGroup)
                .First();

            return View(product);
        }
        #endregion

        #region Управление
        [HttpPost]
        public IActionResult CreateSpecific(ProductSpecific product)
        {
            product.ProductGroup = context.ProductsGroup.Where(pg=>pg.Id == product.ProductGroupId).First();
            context.ProductsSpecific.Add(product);
            context.SaveChanges();
            return RedirectToAction(nameof(ListSpecific), new { groupId = product.ProductGroupId });
        }
        [HttpPost]
        public IActionResult EditSpecific(ProductSpecific product)
        {
            var DBProduct = context.ProductsSpecific
                .Where(ps => ps.Id == product.Id)
                .FirstOrDefault();
            if (DBProduct == null)
            {
                NotFound();
                return RedirectToAction(nameof(ListGroup));
            }

            DBProduct.Size = product.Size;
            DBProduct.Price = product.Price;

            context.SaveChanges();
            return RedirectToAction(nameof(ListSpecific), new { groupId = product.ProductGroupId });
        }
        public IActionResult DeleteSpecific(int? productId, int? id)
        {
            if (id != null)
            {
                context.ProductsSpecific.Where(ps => ps.Id == id).ExecuteDelete();
                context.SaveChanges();
            }
            return RedirectToAction(nameof(ListSpecific), new { groupId = productId });
        }
        #endregion
        #endregion
    }
}