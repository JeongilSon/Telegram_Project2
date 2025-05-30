using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Telegram_Project2_Web.Models;

namespace Telegram_Project2_Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BotController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BotController(AppDbContext context)
        {
            _context = context;
        }       
        
        // GET: api/Bot/tokens
        // 현재 설정된 봇 토큰들 가져오기
        [HttpGet("tokens")]
        public async Task<ActionResult<BotTokenModel>> GetBotTokens()
        {
            try
            {
                var botToken = await _context.BotTokens.FirstOrDefaultAsync();
                
                if (botToken == null)
                {
                    return Ok(new BotTokenModel 
                    { 
                        Chat_bot_token = "", 
                        Main_bot_token = "" 
                    });
                }
                
                return Ok(botToken);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        // ChatBot 토큰만 저장/업데이트
        [HttpPost("chatbot")]
        public async Task<IActionResult> SaveChatBotToken([FromBody] ChatBotTokenRequest request)
        {
            try
            {
                var botToken = await _context.BotTokens.FirstOrDefaultAsync();

                if (botToken == null)
                {
                    // 새로 생성
                    botToken = new BotTokenModel
                    {
                        Id = 1,
                        Chat_bot_token = request.ChatBotToken,
                    };
                    _context.BotTokens.Add(botToken);
                }
                else
                {
                    // 기존 것 업데이트
                    botToken.Chat_bot_token = request.ChatBotToken;
                }

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "ChatBot 토큰이 저장되었습니다." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // MainBot 토큰만 저장/업데이트
        [HttpPost("mainbot")]
        public async Task<IActionResult> SaveMainBotToken([FromBody] MainBotTokenRequest request)
        {
            try
            {
                var botToken = await _context.BotTokens.FirstOrDefaultAsync();

                if (botToken == null)
                {
                    // 새로 생성
                    botToken = new BotTokenModel
                    {
                        Id = 1,
                        Main_bot_token = request.MainBotToken                        
                    };
                    _context.BotTokens.Add(botToken);
                }
                else
                {
                    // 기존 것 업데이트
                    botToken.Main_bot_token = request.MainBotToken;
                }

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "MainBot 토큰이 저장되었습니다." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
    // 요청 모델들
    public class ChatBotTokenRequest
    {
        public string? ChatBotToken { get; set; }
    }

    public class MainBotTokenRequest
    {
        public string? MainBotToken { get; set; }
    }
}

