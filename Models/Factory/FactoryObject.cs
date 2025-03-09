using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MyCompany
{
    public abstract class FactoryObject
    {
        #region поля
        public int Id { get; set; } // Номер объекта в БД

        [Required(ErrorMessage = "Название обязательно")]
        [StringLength(50, ErrorMessage = "Название должно быть не длиннее 50 символов")]
        public string Name { get; set; } = ""; // Название объекта
        #endregion

        #region Методы
        public override string ToString()
        {
            return Name;
        }
        #endregion
    }
}