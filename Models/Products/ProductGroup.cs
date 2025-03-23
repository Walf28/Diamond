using Diamond.Models.Factory;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diamond.Models
{
    public class ProductGroup
    {
        #region Поля
        #region Обычные
        public int Id { get; set; }
        public string Name { get; set; } = ""; // Наименование продукции
        #endregion

        #region Ссылочные
        public List<Technology> TechnologyProcessing { get; set; } = []; // Процесс обработки
        public List<ProductSpecific> ProductsSpecific { get; set; } = []; // Разновидности (размер/цена)
        [ForeignKey("MaterialId")]
        public Material Material { get; set; } = null!; // На каком сырье создаётся
        #endregion

        #region Id ссылок
        public int MaterialId { get; set; } // На каком сырье создаётся
        #endregion
        #endregion

        #region Методы
        public static string ToString(List<ProductGroup> products)
        {
            string s = "";
            if (products.Count > 0)
            {
                foreach (ProductGroup p in products)
                    s += $"{p.Name}, ";
                s = s.Remove(s.Length - 2);
            }
            return s;
        }
        public override string ToString()
        {
            return Name;
        }
        #endregion
    }
}