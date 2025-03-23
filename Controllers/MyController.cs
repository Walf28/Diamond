using Diamond.Models.Factory;
using Microsoft.AspNetCore.Mvc;

namespace Diamond.Controllers
{
    public class MyController : Controller
    {
        public IActionResult Load()
        {
            Server.FactoryLoad();
            return RedirectToAction("List", "Factory");
        }

        public IActionResult Save()
        {
            Server.Save();
            return RedirectToAction("List", "Factory");
        }
    }
}
