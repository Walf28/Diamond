using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diamond.Models
{
    public class Request
    {
        #region Поля
        #region Обычные
        public int Id { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Разрешены только положительные значения")]
        public int Count { get; set; } = 1;
        public DateTime DateOfReceipt { get; set; } // Дата поступления заявки
        public DateTime DateOfDesiredComplete { get; set; } // Дата желаемого выполнения заявки
        public DateTime? DateOfAcceptance { get; set; } // Дата принятия заявки
        public DateTime? DateOfComplete { get; set; } // Дата завершения выполнения заявки
        public RequestStatus Status { get; set; } = 0; // Статус заявки
        #endregion

        #region Ссылочные
        [NotMapped]
        public readonly DB context = new();
        [ForeignKey("FactoryId")]
        public Factory? Factory { get; set; } // Завод
        [ForeignKey("ProductId")]
        public ProductSpecific Product { get; set; } = null!; // Что заказали (лучше сделать списком, но непонятно пока, как это сделать, поэтому пока что будет так для упрощения задачи - потом расширим, если что)
        #endregion

        #region Id ссылок
        public int? FactoryId { get; set; }
        public int ProductId { get; set; }
        #endregion
        #endregion

        #region Свойства
        public string GetProductName
        {
            get => context.ProductsSpecific
                    .Where(ps => ps.Id == ProductId)
                    .Include(p => p.ProductGroup)
                    .First()
                    .ProductGroup
                    .Name;
        }
        /*public DateTime GetDateOfReceiptLocal => DateTime.SpecifyKind(DateOfReceipt, DateTimeKind.Utc);
        public DateTime GetDateOfDesiredCompleteLocal => DateTime.SpecifyKind(DateOfDesiredComplete, DateTimeKind.Utc);
        public DateTime? GetDateOfAcceptanceLocal => DateOfAcceptance != null ? DateTime.SpecifyKind((DateTime)DateOfAcceptance, DateTimeKind.Utc) : null;
        public DateTime? GetDateOfCompleteLocal => DateOfComplete != null ? DateTime.SpecifyKind((DateTime)DateOfComplete, DateTimeKind.Utc) : null;*/
        public DateTime GetDateOfReceiptLocal => DateOfReceipt.ToLocalTime();
        public DateTime GetDateOfDesiredCompleteLocal => DateOfDesiredComplete.ToLocalTime();
        public DateTime? GetDateOfAcceptanceLocal => DateOfAcceptance != null ? DateOfAcceptance.Value.ToLocalTime() : null;
        public DateTime? GetDateOfCompleteLocal => DateOfComplete != null ? DateOfComplete.Value.ToLocalTime() : null;
        #endregion
    }
}