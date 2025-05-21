using System;
using System.ComponentModel.DataAnnotations;

namespace Telegram_Project2_Web.Models
{
    public class UserModel
    {
        [Key]
        [Required(ErrorMessage = "ChatID는 필수입니다.")]
        public string ChatID { get; set; }

        [Required(ErrorMessage = "TelegramID는 필수입니다.")]
        public string TelegramID { get; set; }

        [Required(ErrorMessage = "NickName는 필수입니다.")]
        public string NickName { get; set; }

        [Required(ErrorMessage = "UserQuestion는 필수입니다.")]
        public string UserQuestion { get; set; }

        [Required(ErrorMessage = "LinkMove는 필수입니다.")]
        public bool LinkMove { get; set; }

        [Required(ErrorMessage = "ChannelMove는 필수입니다.")]
        public bool ChannelMove { get; set; }

        [Required(ErrorMessage = "Attendance는 필수입니다.")]
        public MissionModel MissionInfo{ get; set; }        
    }
}
