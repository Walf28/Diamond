using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MyCompany.Controllers
{
    public class RegionController(DB context) : Controller
    {
        private readonly DB context = context;

        #region Отображение
        public IActionResult List()
        {
            return View(context.Regions.AsNoTracking().ToList());
        }
        public async Task<IActionResult> Create(Factory factory)
        {
            Region region = new()
            {
                FactoryId = factory.Id,
                Factory = await context.Factories
                    .AsNoTracking()
                    .Where(f => f.Id == factory.Id)
                    .Include(f => f.Regions)
                    .FirstAsync(),
            };
            
            region.Materials = [];
            foreach (var item in context.Materials)
                region.Materials.Add(new MaterialForRegion() { Material = item, MaterialId = item.Id, Region = region, RegionId = region.Id });
            
            return View(region);
        }
        public IActionResult Edit(int? Id)
        {
            if (Id == null)
                throw new Exception("Участок не найден");
            var r = context.Regions
                .Where(r => r.Id == Id)
                .Include(r => r.RegionsParents)
                .Include(r => r.RegionsChildrens)
                .Include(r => r.Factory)
                .Include(r => r.Factory.Regions)
                .Include(r => r.Materials)
                .First(r => r.Id == Id);

            foreach (var item in context.Materials)
            {
                bool find = false;
                for (int i = 0; i < r.Materials!.Count; ++i)
                {
                    if (r.Materials[i].MaterialId == item.Id)
                    {
                        find = true;
                        r.Materials[i].Material = item;
                        break;
                    }
                }
                if (!find)
                    r.Materials.Add(new MaterialForRegion() { Material = item, MaterialId = item.Id, Region = r, RegionId = r.Id });
            }

            return View(r);
        }
        #endregion

        #region Управление
        public async Task<IActionResult> Add(Region region)
        {
            region.RegionsParents = [];
            region.RegionsChildrens = [];

            // Добавление подчинённых участков
            if (region.RegionsChildrensId != null && region.RegionsChildrensId.Count > 0)
                foreach (var cId in region.RegionsChildrensId)
                    region.RegionsChildrens.Add(context.Regions.Where(r => r.Id == cId).First());
            
            // Добавление родительских участков
            if (region.RegionsParentsId != null && region.RegionsParentsId.Count > 0)
                foreach (var pId in region.RegionsParentsId)
                    region.RegionsParents.Add(context.Regions.Where(r => r.Id == pId).First());

            // Добавление в БД
            await context.Regions.AddAsync(region);
            await context.SaveChangesAsync();
            return RedirectToAction("Edit", "Factory", new { Id = region.FactoryId });
        }
        public async Task<IActionResult> Update(Region region)
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

            // Обновление подчинённых участков
            DBRegion.RegionsChildrens!.Clear();
            if (region.RegionsChildrensId != null && region.RegionsChildrensId.Count > 0)
                foreach (var cId in region.RegionsChildrensId)
                    DBRegion.RegionsChildrens!.Add(context.Regions.Where(r => r.Id == cId).First());

            // Обновление родительских участков
            DBRegion.RegionsParents!.Clear();
            if (region.RegionsParentsId != null && region.RegionsParentsId.Count > 0)
                foreach (var pId in region.RegionsParentsId)
                    DBRegion.RegionsParents!.Add(context.Regions.Where(r => r.Id == pId).First());

            // Обновление производительности
            DBRegion.Materials!.Clear();
            if (region.Materials != null && region.Materials.Count > 0)
                foreach (var mat in region.Materials)
                    DBRegion.Materials.Add(mat);

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
    }
}