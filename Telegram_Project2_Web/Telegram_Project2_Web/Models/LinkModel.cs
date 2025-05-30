using System.ComponentModel.DataAnnotations;

namespace Telegram_Project2_Web.Models
{
    public class LinkModel
    {
        [Key]
        public string? Link_Url { get; set; }

        [Required]
        public string? Link_Name{ get; set; }

        [Required]
        public string? Link_Chat_Content { get; set; }
    }
}
