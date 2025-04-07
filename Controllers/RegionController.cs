using Diamond.Database;
using Diamond.Models.Factory;
using Diamond.Models.Materials;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Diamond.Controllers
{
    public class RegionController(DB context) : Controller
    {
        private readonly DB context = context;

        #region Отображение
        public IActionResult List()
        {
            return View(context.Regions.AsNoTracking().ToList());
        }
        public IActionResult Create(Factory factory)
        {
            Region region = new()
            {
                FactoryId = factory.Id,
                Factory = context.Factories
                    .AsNoTracking()
                    .Where(f => f.Id == factory.Id)
                    .Include(f => f.Regions)
                    .First(),
                Materials = []
            };
            foreach (var item in context.Materials)
                region.Materials.Add(new MaterialForRegion() { Material = item, MaterialId = item.Id, Region = region, RegionId = region.Id });

            return View(region);
        }
        public IActionResult Edit(int? Id)
        {
            if (Id == null)
                throw new Exception("Участок не найден");
            var region = context.Regions
                .Where(r => r.Id == Id)
                .Include(r => r.RegionsParents)
                .Include(r => r.RegionsChildrens)
                .Include(r => r.Factory)
                .Include(r => r.Factory.Regions)
                .Include(r => r.Materials)
                .Include(r => r.Downtime)
                .First(r => r.Id == Id);

            List<Material> AllMaterials = [.. context.Materials];
            List<Material> NewMaterials = [];
            foreach (var am in AllMaterials)
            {
                bool find = false;
                foreach (var om in region.Materials!)
                {
                    if (om.MaterialId == am.Id)
                    {
                        find = true;
                        break;
                    }
                }

                if (!find)
                    NewMaterials.Add(am);
            }
            ViewBag.NewMaterials = NewMaterials;

            return View(region);
        }
        #endregion

        #region Управление
        public async Task<IActionResult> Add(Region region, List<int> parentRegions, List<int> childrenRegions)
        {
            region.RegionsParents = [];
            region.RegionsChildrens = [];

            // Добавление родительских участков
            if (parentRegions != null && parentRegions.Count > 0)
                foreach (var pId in parentRegions.Where(value => value > 0))
                    region.RegionsParents.Add(context.Regions.Where(r => r.Id == pId).First());

            // Добавление подчинённых участков
            if (childrenRegions != null && childrenRegions.Count > 0)
                foreach (var cId in childrenRegions.Where(value => value > 0))
                    region.RegionsChildrens.Add(context.Regions.Where(r => r.Id == cId).First());

            // Добавление производительности
            if (region.Materials != null)
                for (int i = 0; i < region.Materials.Count; ++i)
                {
                    if (region.Materials[i].Power <= 0)
                    {
                        region.Materials.RemoveAt(i);
                        --i;
                    }
                    else
                        region.Materials[i].Material = context.Materials.Where(m => m.Id == region.Materials[i].MaterialId).First();
                }

            // Добавление в БД
            await context.Regions.AddAsync(region);
            await context.SaveChangesAsync();
            return RedirectToAction("Edit", "Factory", new { Id = region.FactoryId });
        }
        public async Task<IActionResult> Update(Region region, List<int> parentRegions, List<int> childrenRegions)
        {
            var DBRegion = context.Regions
                .Where(r => r.Id == region.Id)
                .Include(r => r.RegionsParents)
                .Include(r => r.RegionsChildrens)
                .Include(r => r.Materials)
                .First();

            // Обновление в БД
            DBRegion.Name = region.Name;
            DBRegion.Type = region.Type;
            DBRegion.TransitTime = region.TransitTime;

            // Добавление родительских участков
            DBRegion.RegionsParents!.Clear();
            if (parentRegions != null && parentRegions.Count > 0)
                foreach (var pId in parentRegions.Where(value => value > 0))
                    DBRegion.RegionsParents.Add(context.Regions.Where(r => r.Id == pId).First());

            // Добавление подчинённых участков
            DBRegion.RegionsChildrens!.Clear();
            if (childrenRegions != null && childrenRegions.Count > 0)
                foreach (var cId in childrenRegions.Where(value => value > 0))
                    DBRegion.RegionsChildrens.Add(context.Regions.Where(r => r.Id == cId).First());

            // Обновление производительности
            for (int i = 0; i < region.Materials.Count; ++i)
            {
                if (region.Materials[i].Power <= 0)
                {
                    region.Materials.RemoveAt(i);
                    --i;
                }
                else
                    region.Materials[i].Material = context.Materials.Where(m => m.Id == region.Materials[i].MaterialId).First();
            }
            DBRegion.Materials = region.Materials;

            // Сохранение всех изменений
            await context.SaveChangesAsync();
            return RedirectToAction("Edit", "Factory", new { Id = region.FactoryId });
        }
        public async Task<IActionResult> Delete(int Id)
        {
            await context.Regions.Where(r => r.Id == Id).ExecuteDeleteAsync();
            await context.SaveChangesAsync();
            return View();
        }
        #endregion

        #region JS-запросы
        [HttpGet]
        public IActionResult GetNameTypeRegion(int regionId)
        {
            return Json(new
            {
                value = context.Regions
                .AsNoTracking()
                .First(r => r.Id == regionId)
                .Type.ToString()
            });
        }
        #endregion
    }
}