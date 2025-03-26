using Diamond.Models;
using Microsoft.AspNetCore.Mvc;

namespace Diamond.Controllers
{
    public class MyController : Controller
    {
        public IActionResult Load()
        {
            Server.NullMethod();
            return RedirectToAction("List", "Factory");
        }
        public IActionResult Save()
        {
            Server.Save(null, new(DateTime.Now));
            return RedirectToAction("List", "Factory");
        }
    }
}
