using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelebotButtonTest.Models
{
    public class LiveChatModel
    {
        public int Id { get; set; } // 메시지 ID   
        public string Chat_ID { get; set; } // 채팅 ID
        public string Message_Content { get; set; } // 채팅 내용
        public string UserName { get; set; }
        public DateTime Timestamp { get; set; } // 채팅 날짜
        public bool IsFromAdmin { get; set; }  // 메시지 방향 (관리자/유저)
        public bool IsRead { get; set; }  // 읽음 여부        
    }
}
