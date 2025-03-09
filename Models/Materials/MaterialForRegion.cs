using System.ComponentModel.DataAnnotations.Schema;

namespace MyCompany
{
    public class MaterialForRegion
    {
        #region Поля
        #region Обычные
        public int Id { get; set; }
        public int Power { get; set; } = 0; // Производительность под данное сырьё
        #endregion

        #region Ссылочные
        public Material Material { get; set; } = new() { Name = "" }; // Сырьё (родитель)
        public Region Region { get; set; } = new() { Factory = new() }; // Участок
        #endregion

        #region Id ссылок
        public int MaterialId { get; set; }
        public int RegionId { get; set; }
        #endregion
        #endregion
    }
}