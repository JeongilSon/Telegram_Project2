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
                    code = c.ChannelCode,
                    name = c.ChannelName,
                    url = c.ChannelUrl
                })
                .ToListAsync();

            return Ok(channels);
        }

        //// POST: api/Channel/notify
        //// 구독 확인 후 사용자에게 알림 전송
        //[HttpPost("notify")]
        //public async Task<ActionResult<object>> NotifyUser([FromBody] NotifyUserRequest request)
        //{
        //}
    }
}
