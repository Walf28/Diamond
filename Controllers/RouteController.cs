﻿using Diamond.Database;
using Diamond.Models.Factory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Diamond.Controllers
{
    public class RouteController(DB context) : Controller
    {
        private readonly DB context = context;

        #region Отображение
        public IActionResult List(int Id)
        {
            Factory f = context.Factories
                .AsNoTracking()
                .Where(f => f.Id == Id)
                .First();
            f.Routes = [.. context.Routes.AsNoTracking().Where(r => r.FactoryId == Id).Include(r => r.Regions)];
            List<Models.Factory.Route> OtherRoutes = f.FindUnusingRoutes();

            ViewBag.FactoryName = f.Name;
            ViewBag.FactoryId = Id;
            ViewBag.ExistRoutes = f.Routes;
            ViewBag.OtherRoutes = OtherRoutes;

            return View();
        }
        #endregion

        #region Управление
        [HttpPost]
        public IActionResult Update(int FactoryId, List<int> check, List<string> name, List<int[]> regions)
        {
            var DBFactory = context.Factories
                .Where(f => f.Id == FactoryId)
                .Include(r => r.Routes)
                .First();
            DBFactory.Routes.Clear();
            context.SaveChanges();

            /*List<Models.Factory.Route> routes = [];
            foreach (int c in check)
            {
                routes.Add(new()
                {
                    Name = name[c] ?? "",
                    Factory = DBFactory,
                });
                foreach (var region in regions[c])
                    routes[^1].Regions.Add(context.Regions.Where(r => r.Id == region).First());
            }
            DBFactory.Routes.AddRange(routes);*/
            DBFactory.Routes.Clear();
            foreach (int c in check)
            {
                DBFactory.Routes.Add(new()
                {
                    Name = name[c] ?? "",
                    Factory = DBFactory,
                });
                foreach (var region in regions[c])
                {
                    DBFactory.Routes[^1].Regions.Add(context.Regions.Where(r => r.Id == region).First());
                    DBFactory.Routes[^1].RegionsRoute.Add(region);
                }
                DBFactory.Routes[^1].Regions.OrderBy(r => r.Routes[^1].RegionsRoute);
            }

            // Сохранение всех изменений
            context.SaveChanges();
            return RedirectToAction("Edit", "Factory", new { Id = FactoryId });
        }
        public async Task<IActionResult> Delete(int Id)
        {
            await context.Regions.Where(r => r.Id == Id).ExecuteDeleteAsync();
            await context.SaveChangesAsync();
            return View();
        }
        #endregion
    }
}