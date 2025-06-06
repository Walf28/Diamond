﻿using Diamond.Database;
using Diamond.Models.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Diamond.Controllers
{
    public class ProductController(DB context) : Controller
    {
        private readonly DB context = context;

        #region Группа продукции
        #region Отображение
        public IActionResult ListGroup()
        {
            return View(context.ProductsGroup.AsNoTracking().Include(pg => pg.Material).OrderBy(pg=>pg.Id).ToList());
        }
        [HttpGet]
        public IActionResult CreateGroup()
        {
            //List<Material> materials = [.. context.Materials.AsNoTracking()];
            ViewBag.materials = context.Materials.AsNoTracking().ToList();
            ViewBag.technologys = context.ProductionStage.AsNoTracking().ToList();
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

            //List<Material> materials = [.. context.Materials];
            //ViewBag.materials = materials;
            ViewBag.materials = context.Materials.AsNoTracking().ToList();
            ViewBag.technologys = context.ProductionStage.AsNoTracking().ToList();

            return View(product);
        }
        #endregion

        #region Управление
        [HttpPost]
        public IActionResult CreateGroup(Product product)
        {
            product.TechnologyProcessing.RemoveAll(tp => tp == 0);
            context.ProductsGroup.Add(product);
            context.SaveChanges();
            return RedirectToAction(nameof(ListGroup));
        }
        [HttpPost]
        public IActionResult EditGroup(Product product)
        {
            Product? productGroup = context.ProductsGroup
                .Where(pg => pg.Id == product.Id)
                .FirstOrDefault();
            if (productGroup == null)
            {
                NotFound();
                return RedirectToAction(nameof(ListGroup));
            }

            productGroup.Name = product.Name;
            productGroup.MaterialId = product.MaterialId;
            productGroup.TechnologyProcessing = product.TechnologyProcessing.Where(tp => tp != 0).ToList();

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

            Package ps = new() { ProductId = (int)groupId };
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

            var product = context.Package
                .AsNoTracking()
                .Where(ps => ps.Id == productId)
                .Include(ps => ps.ProductGroup)
                .First();

            return View(product);
        }
        #endregion

        #region Управление
        [HttpPost]
        public IActionResult CreateSpecific(Package product)
        {
            product.ProductGroup = context.ProductsGroup.Where(pg=>pg.Id == product.ProductId).First();
            context.Package.Add(product);
            context.SaveChanges();
            return RedirectToAction(nameof(ListSpecific), new { groupId = product.ProductId });
        }
        [HttpPost]
        public IActionResult EditSpecific(Package product)
        {
            var DBProduct = context.Package
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
            return RedirectToAction(nameof(ListSpecific), new { groupId = product.ProductId });
        }
        public IActionResult DeleteSpecific(int? productId, int? id)
        {
            if (id != null)
            {
                context.Package.Where(ps => ps.Id == id).ExecuteDelete();
                context.SaveChanges();
            }
            return RedirectToAction(nameof(ListSpecific), new { groupId = productId });
        }
        #endregion
        #endregion
    }
}