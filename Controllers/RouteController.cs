using Diamond.Database;
using Diamond.Models;
using Diamond.Models.Factory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Diamond.Controllers
{
    public class RouteController : Controller
    {
        private readonly DB context = Server.context;

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

            // Нахождение маршрутов
            List<Models.Factory.Route> routes = [];
            foreach (int c in check)
            {
                // Создание маршрута
                Models.Factory.Route route = new()
                {
                    Name = name[c] ?? "",
                    Factory = DBFactory,
                };
                foreach (var region in regions[c])
                {
                    route.Regions.Add(context.Regions.Where(r => r.Id == region).First());
                    route.RegionsRoute.Add(region);
                }

                routes.Add(route);
            }

            // Добавление маршрутов
            foreach (var route in routes)
            {
                bool find = false;
                foreach (var r in DBFactory.Routes)
                    if (route.RegionsRoute == r.RegionsRoute)
                    {
                        find = true;
                        break;
                    }
                if (!find)
                    DBFactory.Routes.Add(route);
            }

            // Удаление маршрутов
            for (int i = 0; i < DBFactory.Routes.Count; ++i)
            {
                bool find = false;
                foreach (var route in routes)
                    if (route.RegionsRoute == DBFactory.Routes[i].RegionsRoute)
                    {
                        find = true;
                        break;
                    }

                if (!find)
                {
                    DBFactory.Routes.RemoveAt(i);
                    --i;
                }
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