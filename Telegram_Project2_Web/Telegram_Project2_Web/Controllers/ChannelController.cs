using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Telegram_Project2_Web.Models;

namespace Telegram_Project2_Web.Controllers
{
    //다 버튼
    [Route("api/[controller]")]
    [ApiController]
    public class ChannelController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChannelController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Channel
        // 채널 목록 가져오기
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetChannels()
        {
            // 모든 채널 정보 조회 (channel_list_table)
            var channels = await _context.Channels
                .Select(c => new
                {
                    code = c.Channel_Code,
                    name = c.Channel_Name,
                    url = c.Channel_Url,
                    content = c.Channel_Chat_Content
                })
                .ToListAsync();

            return Ok(channels);
        }
        // POST: api/Channel/Input
        [HttpPost("Input")]
        public async Task<ActionResult<object>> CreateChannel([FromBody] ChannelModel channel)
        {
            if (channel == null)
            {
                return BadRequest(new { success = false, message = "채널 정보가 유효하지 않습니다." });
            }

            // 필수 필드 검증
            if (string.IsNullOrEmpty(channel.Channel_Code) ||
                string.IsNullOrEmpty(channel.Channel_Name) ||
                string.IsNullOrEmpty(channel.Channel_Url))
            {
                return BadRequest(new { success = false, message = "채널 코드, 이름, URL은 필수 입력 항목입니다." });
            }
            int channelCount = await _context.Channels.CountAsync();
            if (channelCount > 0)
            {
                return Conflict(new { success = false, message = "최대 1개만 등록 가능합니다." });
            }
            try
            {
                // 중복 체크
                var existingChannel = await _context.Channels.FirstOrDefaultAsync(c => c.Channel_Code == channel.Channel_Code);
                if (existingChannel != null)
                {
                    // 기존 채널 업데이트
                    existingChannel.Channel_Name = channel.Channel_Name;
                    existingChannel.Channel_Url = channel.Channel_Url;
                    existingChannel.Channel_Chat_Content = channel.Channel_Chat_Content;
                    _context.Entry(existingChannel).State = EntityState.Modified;
                }
                else
                {
                    // 새 채널 추가
                    _context.Channels.Add(channel);
                }
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { success = false, message = $"채널 저장 중 오류가 발생했습니다: {ex.Message}" });
            }
            try
            {
                await _context.SaveChangesAsync();

                // 저장된 채널 정보를 익명 객체로 변환하여 반환
                var savedChannel = new
                {
                    code = channel.Channel_Code,
                    name = channel.Channel_Name,
                    url = channel.Channel_Url,
                    content = channel.Channel_Chat_Content
                };

                return Ok(new { success = true, message = "채널이 성공적으로 저장되었습니다.", channel = savedChannel });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { success = false, message = $"채널 저장 중 오류가 발생했습니다: {ex.Message}" });
            }
        }
        // DELETE: api/Channel/{code}
        [HttpDelete("{code}")]
        public async Task<ActionResult<object>> DeleteChannel(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest(new { success = false, message = "채널 코드가 유효하지 않습니다." });
            }

            var channel = await _context.Channels.FindAsync(code);
            if (channel == null)
            {
                return NotFound(new { success = false, message = $"코드가 '{code}'인 채널을 찾을 수 없습니다." });
            }

            _context.Channels.Remove(channel);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "채널이 성공적으로 삭제되었습니다." });
        }
    }
}
