using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Telegram_Project2_Web.Models;
using System.Text.Json;

namespace Telegram_Project2_Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LivechatController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;

        public LivechatController(AppDbContext context)
        {
            _context = context;
            _httpClient = new HttpClient();
        }

        // POST: api/Livechat/Send
        [HttpPost("Send")]
        public async Task<ActionResult<object>> SendMessage([FromBody] LivechatModel message)
        {
            // 유효성 검사
            if (string.IsNullOrEmpty(message.Chat_ID) || string.IsNullOrEmpty(message.Message_Content))
                return BadRequest(new { success = false, message = "필수 정보가 누락되었습니다." });

            // 메시지 타임스탬프 설정
            message.Timestamp = DateTime.Now;
            message.IsRead = false;

            // 데이터베이스에 저장
            _context.LiveChats.Add(message);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "메시지가 저장되었습니다." });
        }

        // 2. 관리자 응답용 함수는 다른 경로 사용
        [HttpPost("AdminReply")]
        public async Task<IActionResult> SendAdminReply([FromBody] AdminMessageDto message)
        {
            // 메시지 저장
            var chatMessage = new LivechatModel
            {
                Chat_ID = message.chatId,
                Message_Content = message.text,
                Timestamp = DateTime.Now,
                IsFromAdmin = true,
                IsRead = false,
                Username = "관리자"
            };

            _context.LiveChats.Add(chatMessage);
            await _context.SaveChangesAsync();

            // 참고: 텔레그램 봇이 주기적으로 DB를 확인하여 관리자 메시지를 전송합니다.
            return Ok(new { success = true, message = "메시지가 저장되었습니다. 봇이 곧 전송할 예정입니다." });
        }

        // 텔레그램 봇으로부터 메시지 수신
        [HttpPost("ReceiveFromBot")]
        public async Task<IActionResult> ReceiveFromBot([FromBody] BotMessageDto message)
        {
            // 메시지 저장
            var chatMessage = new LivechatModel
            {
                Chat_ID = message.chatId,
                Message_Content = message.text,
                Username = message.username,
                Timestamp = DateTime.Now,
                IsFromAdmin = false,
                IsRead = false
            };
            
            _context.LiveChats.Add(chatMessage);
            await _context.SaveChangesAsync();
            
            return Ok(new { success = true });
        }        
        
        // 특정 사용자와의 메시지 기록 조회
        [HttpGet("{chatId}")]
        public async Task<IActionResult> GetMessages(string chatId, [FromQuery] DateTime? since = null)
        {
            var query = _context.LiveChats.Where(m => m.Chat_ID == chatId);
            
            // 특정 시간 이후의 메시지만 조회 (폴링 최적화)
            if (since.HasValue)
            {
                query = query.Where(m => m.Timestamp > since.Value);
            }
            
            var messages = await query
                .OrderBy(m => m.Timestamp)
                .Select(m => new
                {
                    id = m.Id,
                    content = m.Message_Content,
                    username = m.Username ?? (m.IsFromAdmin ? "관리자" : "사용자"),
                    timestamp = m.Timestamp,
                    isFromAdmin = m.IsFromAdmin,
                    isRead = m.IsRead
                })
                .ToListAsync();
                
            return Ok(messages);
        }
        
        // 모든 채팅 사용자 목록 조회
        [HttpGet("Users")]
        public async Task<IActionResult> GetChatUsers()
        {
            var chatUsers = await _context.LiveChats
                .GroupBy(m => m.Chat_ID)
                .Select(g => new
                {
                    chatId = g.Key,
                    lastMessage = g.OrderByDescending(m => m.Timestamp).FirstOrDefault().Message_Content,
                    lastTimestamp = g.Max(m => m.Timestamp),
                    username = g.OrderByDescending(m => m.Timestamp)
                             .FirstOrDefault(m => !string.IsNullOrEmpty(m.Username) && !m.IsFromAdmin).Username ?? "사용자",
                    unreadCount = g.Count(m => !m.IsRead && !m.IsFromAdmin)
                })
                .OrderByDescending(u => u.lastTimestamp)
                .ToListAsync();
                
            return Ok(chatUsers);
        }
        
        // 메시지 읽음 처리
        [HttpPut("{chatId}/MarkAsRead")]
        public async Task<IActionResult> MarkAsRead(string chatId)
        {
            var unreadMessages = await _context.LiveChats
                .Where(m => m.Chat_ID == chatId && !m.IsRead && !m.IsFromAdmin)
                .ToListAsync();
                
            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }
            
            await _context.SaveChangesAsync();
            
            return Ok(new { success = true, count = unreadMessages.Count });
        }

        // 채팅 대화 삭제 API
        [HttpDelete("{chatId}")]
        public async Task<IActionResult> DeleteChat(string chatId)
        {
            var messages = await _context.LiveChats
                .Where(m => m.Chat_ID == chatId)
                .ToListAsync();
                
            if (messages.Count == 0)
            {
                return NotFound(new { success = false, message = "채팅 내역이 없습니다." });
            }
            
            _context.LiveChats.RemoveRange(messages);
            await _context.SaveChangesAsync();
            
            return Ok(new { success = true, message = "채팅 내역이 삭제되었습니다.", count = messages.Count });
        }
        
        // 처리되지 않은 관리자 메시지 조회 (봇에서 사용)
        [HttpGet("UnprocessedAdminMessages")]
        public async Task<IActionResult> GetUnprocessedAdminMessages()
        {
            var unprocessedMessages = await _context.LiveChats
                .Where(m => m.IsFromAdmin && !m.IsRead)
                .OrderBy(m => m.Timestamp)
                .Select(m => new
                {
                    Id = m.Id,
                    Chat_ID = m.Chat_ID,
                    Message_Content = m.Message_Content,
                    Timestamp = m.Timestamp
                })
                .ToListAsync();
                
            return Ok(unprocessedMessages);
        }
        
        // 관리자 메시지 처리 완료 표시 (봇에서 사용)
        [HttpPut("MarkAsProcessed/{messageId}")]
        public async Task<IActionResult> MarkAsProcessed(int messageId)
        {
            var message = await _context.LiveChats.FindAsync(messageId);
            
            if (message == null)
            {
                return NotFound(new { success = false, message = "메시지를 찾을 수 없습니다." });
            }
            
            message.IsRead = true;
            await _context.SaveChangesAsync();
            
            return Ok(new { success = true, message = "메시지가 처리 완료로 표시되었습니다." });
        }
    }

    // DTO 클래스들
    public class BotMessageDto
    {
        public string chatId { get; set; }
        public string text { get; set; }
        public string username { get; set; }
        public DateTime timestamp { get; set; }
    }
    
    public class AdminMessageDto
    {
        public string chatId { get; set; }
        public string text { get; set; }
    }
}
