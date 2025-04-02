using Diamond.Database;
using Diamond.Models.Factory;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diamond.Models.Materials
{
    public class MaterialForRegion
    {
        #region Поля
        #region Обычные
        public int Id { get; set; }
        public int Power { get; set; } = 0; // Производительность под данное сырьё
        #endregion

        #region Ссылочные
        [NotMapped]
        public readonly DB context = new();
        public Material Material { get; set; } = null!; // Сырьё (родитель)
        public Region Region { get; set; } = null!; // Участок
        #endregion

        #region Id ссылок
        public int MaterialId { get; set; }
        public int RegionId { get; set; }
        #endregion
        #endregion

        #region Свойства
        public string GetName
        {
            get
            {
                Material ??= context.Materials.AsNoTracking().Where(m => m.Id == MaterialId).First();
                return Material.Name;
            }
        }
        #endregion

        #region Методы
        public static string ToString(List<MaterialForRegion> materials)
        {
            string s = "";
            if (materials.Count > 0)
            {
                foreach (var material in materials)
                    s += $" | {material.GetName} - {material.Power}";
                s = s.Remove(0, 3);
            }
            return s;
        }
        #endregion
    }
}