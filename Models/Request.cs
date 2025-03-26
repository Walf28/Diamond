using Diamond.Database;
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
        public int Count { get; set; } = 1; // Сколько заказали
        public int CountComplete = 0; // Сколько выполнено от заказа
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
        public Factory.Factory? Factory { get; set; } // Завод
        [ForeignKey("ProductId")]
        public ProductSpecific Product { get; set; } = null!; // Что заказали (лучше сделать списком, но непонятно пока, как это сделать, поэтому пока что будет так для упрощения задачи - потом расширим, если что)
        #endregion

        #region Id ссылок
        public int? FactoryId { get; set; }
        public int ProductId { get; set; }
        #endregion
        #endregion

        #region Свойства
        public ProductSpecific? GetProductSpecific => context.ProductsSpecific.Where(ps => ps.Id == ProductId).FirstOrDefault();
        public ProductGroup? GetProductGroup => context.ProductsGroup.Where(pg => pg.Id == context.ProductsSpecific.First(ps=>ps.Id == ProductId).ProductGroupId).FirstOrDefault();
        public string GetProductName
        {
            get
            {
                try
                {
                    return context.ProductsSpecific
                        .AsNoTracking()
                        .Where(ps => ps.Id == ProductId)
                        .Include(p => p.ProductGroup)
                        .First()
                        .ProductGroup
                        .Name;
                }
                catch
                {
                    return "";
                }
            }
        }
        public DateTime GetDateOfReceiptLocal => DateOfReceipt.ToLocalTime();
        public DateTime GetDateOfDesiredCompleteLocal => DateOfDesiredComplete.ToLocalTime();
        public DateTime? GetDateOfAcceptanceLocal => DateOfAcceptance?.ToLocalTime();
        public DateTime? GetDateOfCompleteLocal => DateOfComplete?.ToLocalTime();
        #endregion
    }
}