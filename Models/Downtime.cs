﻿using Diamond.Models.Factory;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diamond.Models
{
    public class Downtime
    {
        #region Поля
        #region Обычные
        public int Id { get; set; }
        public DateTime? DowntimeStart { get; set; } // Начало простоя
        public int? DowntimeDuration { get; set; } // Продолжительность простоя
        public string? DowntimeReason { get; set; } // Причина простоя
        public DateTime? DowntimeFinish { get; set; } // Конец простоя
        #endregion

        #region Ссылочные
        [ForeignKey("RegionId")]
        public required Region Region { get; set; }
        #endregion

        #region Id ссылок
        public int RegionId { get; set; }
        #endregion
        #endregion
    }
}