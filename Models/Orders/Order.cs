using Diamond.Database;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diamond.Models.Orders
{
    public enum OrderStatus
    {
        [Display(Name = "Рассматривается")]
        ANALYZE,
        [Display(Name = "Изготавливается")]
        FABRICATING,
        [Display(Name = "В доставке")]
        DELIVERY,
        [Display(Name = "Выполнена")]
        COMPLETE
    }

    public class Order
    {
        #region Поля
        #region Обычные
        public int Id { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Разрешены только положительные значения")]
        public DateTime DateOfReceipt { get; set; } // Дата поступления заявки
        public DateTime DateOfDesiredComplete { get; set; } // Дата желаемого выполнения заявки
        public DateTime? DateOfAcceptance { get; set; } // Дата принятия заявки
        public DateTime? DateOfComplete { get; set; } // Дата завершения выполнения заявки
        public OrderStatus Status { get; set; } = 0; // Статус заявки
        #endregion

        #region Ссылочные
        [NotMapped]
        public readonly DB context = new();
        [ForeignKey(nameof(FactoryId))]
        public Factory.Factory? Factory { get; set; } // Завод
        public List<OrderPart> OrderParts { get; set; } = [];
        #endregion

        #region Id ссылок
        public int? FactoryId { get; set; }
        #endregion
        #endregion

        #region Свойства
        public DateTime GetDateOfReceiptLocal => DateOfReceipt.ToLocalTime();
        public DateTime GetDateOfDesiredCompleteLocal => DateOfDesiredComplete.ToLocalTime();
        public DateTime? GetDateOfAcceptanceLocal => DateOfAcceptance?.ToLocalTime();
        public DateTime? GetDateOfCompleteLocal => DateOfComplete?.ToLocalTime();
        #endregion

        #region Методы
        public void UpdateStatus()
        {
            switch (Status)
            {
                case OrderStatus.FABRICATING:
                    foreach (var op in OrderParts)
                        if (op.CountComplete != op.Count)
                            return;
                    Status = OrderStatus.DELIVERY;
                    DateOfComplete = DateTime.UtcNow;
                    return;
                default: return;
            }
        }
        #endregion
    }
}