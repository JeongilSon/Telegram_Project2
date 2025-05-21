using System;
using System.ComponentModel.DataAnnotations;

namespace Telegram_Project2_Web.Models
{
    public class LiveChatModel
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; }
        
        public string TelegramUsername { get; set; }
        
        [Required]
        public string Message { get; set; }
        
        [Required]
        public DateTime MessageTime { get; set; } = DateTime.UtcNow;
        
        public bool IsFromAdmin { get; set; } = false;
        
        public string AdminId { get; set; }
        
        public bool IsRead { get; set; } = false;
        
        public DateTime? ReadTime { get; set; }
        
        public string ChatSessionId { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // 추가 메타데이터
        public string MessageType { get; set; } = "Text"; // Text, Image, File, etc.
        
        public string FileUrl { get; set; }
        
        public string FileName { get; set; }
        
        public int? FileSize { get; set; }
    }
}
