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
            Server.Save();
            return RedirectToAction("List", "Factory");
        }
        public IActionResult NullAll()
        {
            Server.NullALL();
            return RedirectToAction("List", "Factory");
        }
    }
}
