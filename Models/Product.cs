using System.ComponentModel.DataAnnotations.Schema;

namespace MyCompany
{
    public class Product
    {
        #region Поля
        #region Обычные
        public int Id { get; set; }
        public string? Name { get; set; } // Наименование продукции
        public int Size { get; set; } // Размер в граммах
        public int Price { get; set; } // Цена
        #endregion

        #region Ссылочные
        [NotMapped]
        public string TechnologyProcessing { get; set; } = ""; // Пока что, наверное, не пригодится
        public List<Request>? Requests { get; set; } // Заявки

        [ForeignKey("MaterialId")]
        public Material? Material { get; set; } // На каком сырье создаётся
        #endregion

        #region Id ссылок
        public int MaterialId { get; set; } // На каком сырье создаётся
        #endregion
        #endregion

        #region Методы
        /*public bool Save()

        {
            // Если объект ещё не создан, то его надо добавить
            if (id == null)
            {
                if (DB.Insert("Products", new string[] { name, TechnologyProcessing }, out int? returnID))
                {
                    id = returnID;
                    return true;
                }
                else
                    return false;
            }

            // Если объект уже создан, то его надо просто обновить
            return DB.Replace("Products", "id", id.ToString()!,
                new string[] { "name", "TechnologyProcessing" },
                new string[] { name, processing_technology });
        }

        public bool Delete()
        {
            // Надо подтверждение существования этого продукта
            if (id == null)
                return false;

            // А теперь можно и сам завод удалить
            return DB.Delete("Products", "id", id.ToString()!);
        }*/
        #endregion
    }
}