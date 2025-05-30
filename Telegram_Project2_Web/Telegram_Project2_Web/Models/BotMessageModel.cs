using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Telegram_Project2_Web.Models
{
    [Table("bot_message_table")]
    public class BotMessageModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }

        [JsonPropertyName("welcome_message")]
        [Column("welcome_message")]
        public string? Welcome_Message { get; set; }

        [JsonPropertyName("question_message")]
        [Column("question_message")]
        public string? Question_Message { get; set; }
    }
}