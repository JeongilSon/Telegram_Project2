using System.ComponentModel.DataAnnotations;

namespace Telegram_Project2_Web.Models
{
    public class AccountModel
    {
        [Key]
        public string Id { get; set; }

        [Required]
        public string Pw { get; set; }
    }
}
