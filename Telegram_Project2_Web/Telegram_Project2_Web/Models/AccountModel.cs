using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telegram_Project2_Web.Models
{
    public class AccountModel
    {
        [Key]
        [Column("id")]
        public string? Id { get; set; }

        [Required]
        [Column("pw")]
        public string? Pw { get; set; }
    }
}
