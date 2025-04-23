using System.ComponentModel.DataAnnotations.Schema;

namespace Diamond.Models.Factory
{
    public class Downtime
    {
        #region Поля
        #region Обычные
        public int Id { get; set; }
        public DateTime DowntimeStart { get; set; } = DateTime.UtcNow; // Начало простоя
        public double DowntimeDuration { get; set; } = double.PositiveInfinity; // Продолжительность простоя (в минутах)
        public string? DowntimeReason { get; set; } = ""; // Причина простоя
        public DateTime? DowntimeFinish { get; set; } = null; // Конец простоя
        #endregion

        #region Ссылочные
        [ForeignKey("RegionId")]
        public required Region Region { get; set; }
        #endregion

        #region Id ссылок
        public int RegionId { get; set; }
        #endregion

        /*#region ВременнЫе
        [NotMapped]
        private System.Timers.Timer Timer { get; set; } = new();
        #endregion*/
        #endregion

        #region Свойства
        public double SetDowntimeDuration
        {
            set
            {
                if (value < 0)
                    return;
                DowntimeDuration = value;
                DowntimeFinish = DowntimeStart.AddMinutes(value);
            }
        }
        public DateTime? SetDowntimeFinish
        {
            set
            {
                if (value == null)
                {
                    DowntimeFinish = value;
                    DowntimeDuration = double.PositiveInfinity;
                }
                else
                {
                    if (value < DowntimeStart)
                        return;
                    DowntimeFinish = value;
                    SetDowntimeDuration = DowntimeFinish!.Value.Subtract(DowntimeStart).TotalMinutes;
                }
            }
        }
        public DateTime GetDowntimeStartLocal => DowntimeStart.ToLocalTime();
        public DateTime? GetDowntimeFinishLocal => DowntimeFinish?.ToLocalTime();
        public bool IsDowntimeNow => DowntimeStart.ToUniversalTime() < DateTime.UtcNow
            && (DowntimeFinish == null || DowntimeFinish.Value.ToUniversalTime() > DateTime.UtcNow);
        #endregion

        #region Методы
        public bool IsDowntime(DateTime dateTime)
        {
             return DowntimeStart.ToUniversalTime() < dateTime.ToUniversalTime() && 
                (DowntimeFinish == null || DowntimeFinish.Value.ToUniversalTime() > dateTime.ToUniversalTime());
        }
        #endregion
    }
}