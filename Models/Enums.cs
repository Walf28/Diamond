using System.ComponentModel.DataAnnotations;

namespace Diamond.Models
{
    public enum Technology
    {
        [Display(Name = "Отсутствует")]
        NONE = 0,
        [Display(Name = "Отгрузочный терминал")]
        SHIPPING_TERMINAL = 1,
        [Display(Name = "Терминал сырья")]
        RAW_TERMINAL = 2,
        [Display(Name = "Сортировка")]
        SORTING = 3,
        [Display(Name = "Мойка")]
        WASHING = 4,
        [Display(Name = "Засолка")]
        SALTING = 5,
        [Display(Name = "Обжарка")]
        ROASTING = 6,
        [Display(Name = "Упаковка")]
        PACKAGING = 7,
        [Display(Name = "Бункер")]
        SILO = 8,
    }

    public enum RequestStatus
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
}
