using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Telegram_Project2_Web.Models;

namespace Telegram_Project2_Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BotMessageController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BotMessageController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/BotMessage
        [HttpGet]
        public async Task<ActionResult<object>> GetBotMessage()
        {
            try
            {
                var botMessage = await _context.BotMessages.FirstOrDefaultAsync();

                if (botMessage == null)
                {
                    return Ok(new
                    {
                        welcome_message = "",
                        question_message = ""
                    });
                }

                return Ok(new
                {
                    welcome_message = botMessage.Welcome_Message,
                    question_message = botMessage.Question_Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"메시지 조회 중 오류가 발생했습니다: {ex.Message}" });
            }
        }

        // POST: api/BotMessage
        [HttpPost]
        public async Task<ActionResult<object>> PostBotMessage([FromBody] BotMessageModel botMessage)
        {
            try
            {
                bool hasAnyData = await _context.BotMessages.AnyAsync();

                if (hasAnyData)
                {
                    var existingMessage = await _context.BotMessages.FirstOrDefaultAsync();
                    existingMessage.Welcome_Message = botMessage.Welcome_Message;
                    existingMessage.Question_Message = botMessage.Question_Message;
                    _context.Entry(existingMessage).State = EntityState.Modified;
                }
                else
                {
                    // Id를 명시적으로 null로 설정 (AUTO_INCREMENT를 위해)
                    botMessage.Id = null;
                    _context.BotMessages.Add(botMessage);
                }

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "봇 메시지가 성공적으로 저장되었습니다." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"메시지 저장 중 오류가 발생했습니다: {ex.Message}" });
            }
        }
    }
}
