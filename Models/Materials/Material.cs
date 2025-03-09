namespace MyCompany
{
    public class Material
    {
        #region Поля
        #region Обычные
        public int Id { get; set; }
        public required string Name { get; set; } = "Без названия"; // Наименование сырья
        #endregion

        #region Ссылочные
        public List<MaterialForRegion>? Materials { get; set; }
        public List<Product>? Products { get; set; }
        #endregion

        #region Id ссылок

        #endregion
        #endregion
    }
}