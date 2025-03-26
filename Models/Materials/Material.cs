using Diamond.Models.Factory;
using Diamond.Models.Materials;

namespace Diamond.Models
{
    public class Material
    {
        #region Поля
        #region Обычные
        public int Id { get; set; }
        public string Name { get; set; } = ""; // Наименование сырья
        #endregion

        #region Ссылочные
        public List<MaterialForRegion>? Materials { get; set; }
        public List<ProductGroup>? Products { get; set; }
        public List<Region> RegionsOptions { get; set; } = [];
        public List<Plan> Plans { get; set; } = [];
        public List<MaterialWarehouse> MaterialWarehouses { get; set; } = [];
        #endregion
        #endregion

        #region Методы
        public static string ToString(List<Material> materials)
        {
            string s = "";
            
            if (materials.Count > 0)
            {
                foreach (var material in materials)
                    s += $"{material.Name}, ";
                s = s.Remove(s.Length - 2);
            }

            return s;
        }
        #endregion
    }
}