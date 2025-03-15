using System.Diagnostics;
using Diamond.Models;
using Microsoft.AspNetCore.Mvc;

namespace Diamond.Controllers
{
    public class HomeController : Controller
    {
        #region База
        //private DbContext context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(/*DbContext context,*/ ILogger<HomeController> logger)
        {
            //this.context = context;
            _logger = logger;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        #endregion

        #region Фабрика
        public IActionResult FactoryList()
        {
            List<Factory> list = new List<Factory>();
            return View(list);
        }
        public IActionResult FactoryCreate()
        {
            return View();
        }
        #endregion
    }
}