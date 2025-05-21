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
    public class LinkController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LinkController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Link
        // 링크 목록 가져오기
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LinkTrackingModel>>> GetLinks()
        {
            // 모든 링크 트래킹 정보를 가져옵니다.
            var links = await _context.LinkTrackings
                .OrderByDescending(l => l.ClickTime)
                .ToListAsync();

            return Ok(links);
        }
        //// POST: api/Link/Input
        //// 링크 정보 입력하기
        //[HttpPost("Input")]
        //public async Task<ActionResult<LinkTrackingModel>> PostLink([FromBody] LinkTrackingModel linkData)
        //{
        //    if (linkData == null)
        //    {
        //        return BadRequest("링크 데이터가 없습니다.");
        //    }

        //    // 링크 클릭 시간 설정
        //    linkData.ClickTime = DateTime.Now;
            
        //    // 기존에 같은 사용자가 같은 링크를 클릭한 적이 있는지 확인
        //    var existingClick = await _context.LinkTrackings
        //        .FirstOrDefaultAsync(l => l.UserId == linkData.UserId && l.LinkUrl == linkData.LinkUrl);
            
        //    if (existingClick != null)
        //    {
        //        // 이미 클릭한 경우, 사용자 정보 업데이트
        //        await UpdateUserLinkStatus(linkData.UserId, linkData.LinkId, false);
        //    }
        //    else
        //    {
        //        // 처음 클릭한 경우, 사용자 정보 업데이트
        //        await UpdateUserLinkStatus(linkData.UserId, linkData.LinkId, true);
        //    }

        //    // 링크 트래킹 데이터 저장
        //    _context.LinkTrackings.Add(linkData);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction(nameof(GetLinkById), new { id = linkData.Id }, linkData);
        //}     
    }
}
