namespace MyCompany
{
    public class Request
    {
        #region Поля
        #region Обычные
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int Count { get; set; } = 0;
        #endregion

        #region Ссылочные
        public Factory? Factory { get; set; } // Завод
        public Route? Route { get; set; } // Маршрут
        public Product Product { get; set; } = new(); // Что заказали (лучше сделать списком, но непонятно пока, как это сделать, поэтому пока что будет так для упрощения задачи - потом расширим, если что)
        #endregion

        #region Id ссылок
        #endregion
        #endregion
    }
}
