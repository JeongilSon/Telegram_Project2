using System;
using System.ComponentModel.DataAnnotations;

namespace Telegram_Project2_Web.Models
{
    public enum MissionTypeEnum
    {
        Daily,
        Mission,
        Event
    }
    public class MissionModel
    {
        [Key]
        public int MissionId { get; set; }

        [Required]
        public DateTime CheckInTime { get; set; } = DateTime.UtcNow;        
        public MissionTypeEnum MissionType { get; set; } // Daily, Mission, Event
        public string MissionName { get; set; }        
        public bool IsCompleted { get; set; } = false;
        public int RewardPoints { get; set; } = 0;
    }
}
