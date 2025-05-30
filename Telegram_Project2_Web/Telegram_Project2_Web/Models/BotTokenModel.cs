using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Telegram_Project2_Web.Models
{
    [Table("bot_token_table")]
    public class BotTokenModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  // 추가
        [Column("id")]
        public int Id { get; set; }
        [Column("chat_bot_token")]  // 실제 DB 컬럼명과 일치
        public string? Chat_bot_token { get; set; }

        [Required]
        [Column("main_bot_token")]  // 실제 DB 컬럼명과 일치
        public string? Main_bot_token { get; set; }        
    }
}
