using Diamond.Database;
using Diamond.Models;
using Diamond.Models.Factory;
using Diamond.Models.Materials;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Diamond.Controllers
{
    public class RegionController : Controller
    {
        private readonly DB context = new();

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
            /*foreach (var item in context.Materials)
                region.Materials.Add(new MaterialForRegion() { Material = item, MaterialId = item.Id, Region = region, RegionId = region.Id });*/
            ViewBag.technologys = context.Technologies.AsNoTracking().ToList();
            ViewBag.materials = context.Materials.AsNoTracking().ToList();

            return View(region);
        }
        public IActionResult Edit(int? id)
        {
            if (id == null)
                throw new Exception("Участок не найден");
            var region = context.Regions
                .Where(r => r.Id == id)
                .Include(r => r.RegionsParents).ThenInclude(r => r.Type)
                .Include(r => r.RegionsChildrens).ThenInclude(r => r.Type)
                .Include(r => r.Factory)
                .Include(r => r.Factory.Regions)
                .Include(r => r.Materials).ThenInclude(m=>m.Material)
                .Include(r => r.Downtime)
                .Include(r => r.Type)
                .First(r => r.Id == id);

            /*List<Material> AllMaterials = [.. context.Materials.AsNoTracking()];
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
            ViewBag.NewMaterials = NewMaterials;*/
            ViewBag.materials = context.Materials.AsNoTracking().ToList();
            ViewBag.technologys = context.Technologies.AsNoTracking().ToList();

            return View(region);
        }
        #endregion

        #region Управление
        public IActionResult Add(Region region, List<int> parentRegions, List<int> childrenRegions, int TypeId, List<int> selectedMaterials, List<int> selectedMaterialsPower)
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
            for (int i = 0; i < selectedMaterials.Count; ++i)
            {
                if (selectedMaterials[i] == 0)
                    continue;
                region.Materials.Add(new() { Material = context.Materials.First(m=>m.Id == selectedMaterials[i]), Power = selectedMaterialsPower[i] });
            }

            // Ссылка на тип технологической обработки
            region.Type = context.Technologies.First(t => t.Id == TypeId);

            // Добавление в БД
            context.Regions.Add(region);
            context.SaveChanges();
            Server.FactorysLoad();
            Server.Factories[region.FactoryId].UpdateAllRoutes();
            Server.Save();
            return RedirectToAction("Edit", "Factory", new { Id = region.FactoryId });
        }
        public IActionResult Update(Region region, List<int> parentRegions, List<int> childrenRegions, int TypeId, List<int> selectedMaterials, List<int> selectedMaterialsPower)
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
            for (int i = 0; i < selectedMaterials.Count; ++i)
            {
                if (selectedMaterials[i] == 0)
                    continue;

                if (DBRegion.Materials.FirstOrDefault(m => m.MaterialId == selectedMaterials[i]) == null)
                    DBRegion.Materials.Add(new() { Material = context.Materials.First(m => m.Id == selectedMaterials[i]), Power = selectedMaterialsPower[i] });
                else
                    DBRegion.Materials.Where(m => m.MaterialId == selectedMaterials[i]).Any(m => { m.Power = selectedMaterialsPower[i]; return true; });
            }

            // Ссылка на тип технологической обработки
            DBRegion.Type = context.Technologies.First(t => t.Id == TypeId);

            // Сохранение всех изменений
            context.SaveChanges();
            Server.FactorysLoad();
            Server.Factories[DBRegion.FactoryId].UpdateAllRoutes();
            Server.Save();
            return RedirectToAction("Edit", "Factory", new { Id = region.FactoryId });
        }
        public IActionResult Delete(int Id)
        {
            int FactoryId = context.Regions
                .AsNoTracking()
                .Where(r => r.Id == Id)
                .Select(r => r.FactoryId)
                .First();
            context.Regions.Where(r => r.Id == Id).ExecuteDelete();
            context.SaveChanges();
            Server.FactorysLoad();
            Server.Factories[FactoryId].UpdateAllRoutes();
            Server.Save();
            return View();
        }
        #endregion

        #region JS-запросы
        [HttpGet]
        public IActionResult GetNameTypeRegion(int regionId)
        {
            return Json(new
            {
                value = Server.context.Regions
                .AsNoTracking()
                .Include(r => r.Type)
                .First(r => r.Id == regionId)
                .Type.ToString()
            });
        }
        #endregion
    }
}