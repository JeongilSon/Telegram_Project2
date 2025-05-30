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
        public string? Mission_Name { get; set; }

        [Required]
        public MissionTypeEnum Mission_Type { get; set; } // Daily, Mission, Event        
        public int Mission_Rewords { get; set; } = 0;
        [Required]
        public string? Mission_Chat_Content { get; set; }
    }
}
