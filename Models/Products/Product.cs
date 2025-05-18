using Diamond.Models.Materials;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diamond.Models.Products
{
    public class Product
    {
        #region Поля
        #region Обычные
        public int Id { get; set; }
        public string Name { get; set; } = ""; // Наименование продукции
        public List<int> TechnologyProcessing { get; set; } = []; // Процесс обработки (последовательность id класса ProductionStage)
        #endregion

        #region Ссылочные
        public List<Package> ProductsSpecific { get; set; } = []; // Разновидности (размер/цена)
        [ForeignKey("MaterialId")]
        public Material Material { get; set; } = null!; // На каком сырье создаётся
        #endregion

        #region Id ссылок
        public int MaterialId { get; set; } // На каком сырье создаётся
        #endregion
        #endregion

        #region Методы
        public static string ToString(List<Product> products)
        {
            string s = "";
            if (products.Count > 0)
            {
                foreach (Product p in products)
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