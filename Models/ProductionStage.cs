using Diamond.Models.Factory;

namespace Diamond.Models
{
    public class ProductionStage
    {
        #region Поля
        #region Обычные
        public int Id { get; set; }
        public string Name { get; set; } = ""; // Наименование технологии
        #endregion

        #region Ссылочные
        public List<Region> Regions { get; set; } = null!;
        #endregion
        #endregion

        #region Методы
        public override string ToString()
        {
            return Name;
        }
        #endregion
    }
}