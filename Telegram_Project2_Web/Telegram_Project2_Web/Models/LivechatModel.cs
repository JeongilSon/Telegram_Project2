using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telegram_Project2_Web.Models
{
    public class LivechatModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }  // Primary Key 추가

        [Required]
        public string Chat_ID { get; set; }  // 텔레그램 채팅 ID

        [Required]
        public string Message_Content { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        public bool IsFromAdmin { get; set; }  // 메시지 방향 (관리자/유저)

        public bool IsRead { get; set; }  // 읽음 여부
        public string? Username { get; set; }
    }
}
