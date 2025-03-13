using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MyCompany.Controllers
{
    public class RouteController(DB context) : Controller
    {
        private readonly DB context = context;

        #region Отображение
        public IActionResult List(int id)
        {
            Factory f = context.Factories
                .AsNoTracking()
                .Where(f => f.Id == id)
                .First();
            List<Route> routes = f.FindAllRoutes();

            ViewBag.FactoryName = f.Name;
            ViewBag.FactoryId = f.Id;

            return View(routes);
        }
        #endregion

        #region Управление
        public IActionResult Update(List<Route> routes, int idfactory, bool[] check)
        {
            if (routes.Count > 0)
            {
                var DBFactory = context.Factories
                    .Where(f => f.Id == idfactory)
                    .First();

                DBFactory.Routes = routes;

                // Сохранение всех изменений
                context.SaveChanges();
            }
            return RedirectToAction("Edit", "Factory", new { Id = idfactory });
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