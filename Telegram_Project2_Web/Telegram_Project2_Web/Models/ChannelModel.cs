using System;
using System.ComponentModel.DataAnnotations;

namespace Telegram_Project2_Web.Models
{
    public class ChannelModel
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string ChannelCode { get; set; }        
        public string ChannelName { get; set; }
        public string ChannelUrl { get; set; }

        [Required]
        public bool IsSubscribed { get; set; }
    }
}
