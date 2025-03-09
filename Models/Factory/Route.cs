namespace MyCompany
{
    public class Route : FactoryObject
    {
        #region Поля
        #region Обычные
        public int MaxPower { get; set; } = 0; // Мощность маршрута
        #endregion

        #region Ссылочные
        // Завод
        public Factory Factory { get; set; } = new();
        // Участки данного маршрута
        public List<Region> Regions { get; set; } = [];
        // Заказы на данном маршруте
        public List<Request>? Requests { get; set; }
        #endregion

        #region Id ссылок
        #endregion
        #endregion
    }
}