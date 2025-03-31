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

    public enum RegionStatus
    {
        [Display(Name = "Выключен")]
        OFF,
        [Display(Name = "Свободен")]
        FREE,
        [Display(Name = "Идёт переналадка свободного участка")]
        FREE_READJUSTMENT,
        [Display(Name = "Идёт переналадка под план")]
        READJUSTMENT,
        [Display(Name = "Ожидает загрузки")]
        AWAIT_DOWNLOAD,
        [Display(Name = "Ожидает запуска")]
        AWAITING_LAUNCH,
        [Display(Name = "Работает")]
        IN_WORKING,
        [Display(Name = "Ожидает разгрузки")]
        AWAIT_UNLOADING,
        [Display(Name = "Не работает")]
        DOWNTIME,
        [Display(Name = "Синхронизация с маршрутами")]
        DOWNTIME_FINISH
    }

    public enum PlanStatus
    {
        [Display(Name = "Ожидает подтверждения")]
        AWAIT_CONFIRMATION,
        [Display(Name = "В очереди")]
        QUEUE,
        [Display(Name = "В производстве")]
        PRODUCTION,
        [Display(Name = "Не может продолжить производство")]
        STOP,
        [Display(Name = "Ожидает возобновления производства")]
        PAUSE,
        [Display(Name = "Выполнен")]
        DONE
    }
}