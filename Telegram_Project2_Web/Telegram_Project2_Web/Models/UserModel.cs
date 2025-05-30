using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
namespace Telegram_Project2_Web.Models
{
    public class UserModel
    {
        [Key]
        [Required(ErrorMessage = "ChatID는 필수입니다.")]
        public string? Chat_ID { get; set; }

        [Required(ErrorMessage = "TelegramID는 필수입니다.")]
        public string? Telegram_ID { get; set; }

        [Required(ErrorMessage = "NickName는 필수입니다.")]
        public string? NickName { get; set; }

        [Required(ErrorMessage = "UserQuestion는 필수입니다.")]
        public string? User_Question { get; set; }

        [Required(ErrorMessage = "LinkMove는 필수입니다.")]
        public int Link_Move { get; set; }

        [Required(ErrorMessage = "ChannelMove는 필수입니다.")]
        public int Channel_Move { get; set; }

        [Required(ErrorMessage = "Mission_Complete는 필수입니다.")]
        public int Mission_Complete { get; set; }
    }
}
