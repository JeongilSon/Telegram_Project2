using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telegram_Project2_Web.Models
{
    public class ChannelModel
    {
        [Key]
        [Column("channel_code")]
        public string? Channel_Code { get; set; }

        [Required]
        [Column("channel_name")]
        public string? Channel_Name { get; set; }

        [Column("channel_url")]
        public string? Channel_Url { get; set; }

        public string? Channel_Chat_Content { get; set; }
    }
}
