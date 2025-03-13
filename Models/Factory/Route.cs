using System.ComponentModel.DataAnnotations.Schema;

namespace MyCompany
{
    public class Route: FactoryObject
    {
        #region Поля
        #region Обычные
        public int MaxPower { get; set; } = 0; // Мощность маршрута
        #endregion

        #region Ссылочные
        [NotMapped]
        private readonly DB context = new();

        // Завод
        [ForeignKey("FactoryId")]
        public Factory Factory { get; set; } = new();
        // Участки данного маршрута
        public List<Region> Regions { get; set; } = [];
        // Заказы на данном маршруте
        public List<Request>? Requests { get; set; }
        #endregion

        #region Id ссылок
        public int FactoryId { get; set; }
        #endregion
        #endregion

        #region Свойства
        public string GetContent
        {
            get
            {
                string content = "";
                foreach (var region in Regions)
                {
                    content += $"{region.Name} -> ";
                }
                content = content.Remove(content.Length - 4);
                return content;
            }
        }
        #endregion

        #region Функции
        /*public void UpdateMaxPower() // Придётся изменить класс тогда (а это в любом случае придётся сделать)
        {
            if (Regions.Count == 0)
            {
                Regions = [.. context.Regions.Where(r=> r.FactoryId == FactoryId)];
                if (Regions.Count == 0)
                    return;
            }

            MaxPower = int.MaxValue;
            foreach (var region in Regions)
            {
                if (region.po)
            }
        }*/
        #endregion
    }
}