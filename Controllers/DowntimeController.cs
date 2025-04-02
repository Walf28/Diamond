using Diamond.Database;
using Diamond.Models;
using Diamond.Models.Factory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Diamond.Controllers
{
    public class DowntimeController(DB context) : Controller
    {
        private readonly DB context = context;

        #region Отображение
        [HttpGet]
        public IActionResult Create(int? Id)
        {
            if (Id == null)
                return new EmptyResult();
            Region region = context.Regions.AsNoTracking().First(r => r.Id == Id);
            ViewBag.RegionId = region.Id;
            ViewBag.RegionName = region.Name;
            return View();
        }
        [HttpGet]
        public IActionResult Edit(int? Id)
        {
            if (Id == null)
                return new EmptyResult();
            return View(context.Downtimes.Include(d => d.Region).First(d => d.Id == Id));
        }
        #endregion

        #region Управление
        [HttpPost]
        public IActionResult Create(Downtime downtime)
        {
            downtime.DowntimeStart = downtime.DowntimeStart.ToUniversalTime();
            downtime.DowntimeFinish = downtime.DowntimeFinish?.ToUniversalTime();
            Server.DowntimeSet(downtime);
            return RedirectToAction("Edit", "Factory", new { Id = context.Regions.First(r => r.Id == downtime.RegionId).FactoryId });
        }
        [HttpPost]
        public IActionResult Edit(Downtime downtime)
        {
            Server.DowntimeSet(downtime);
            return RedirectToAction("Edit", "Factory", new { Id = context.Regions.First(r => r.Id == downtime.RegionId).FactoryId });
        }
        public IActionResult StopNow(Downtime downtime)
        {
            Server.DowntimeStop(downtime.RegionId);
            return RedirectToAction("Edit", "Factory", new { Id = context.Regions.First(r => r.Id == downtime.RegionId).FactoryId });
        }
        #endregion

        #region JS-запросы
        [HttpGet]
        public IActionResult GetDuration(DateTime dtStart, DateTime dtFinish)
        {
            double Duration = dtFinish.ToLocalTime().Subtract(dtStart.ToLocalTime()).TotalMinutes;
            return Json(new { value = Duration });
        }
        [HttpGet]
        public IActionResult GetDtFinish(DateTime dtStart, int duration)
        {
            string dtFinish = dtStart.AddMinutes(duration).ToString("yyyy-MM-ddTHH:mm");
            return Json(new { value = dtFinish });
        }
        #endregion
    }
}