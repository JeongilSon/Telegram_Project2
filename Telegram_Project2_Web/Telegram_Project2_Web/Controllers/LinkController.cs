using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Telegram_Project2_Web.Models;

namespace Telegram_Project2_Web.Controllers
{
    //가 버튼
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
        public async Task<ActionResult<IEnumerable<LinkModel>>> GetLinks()
        {
            // 모든 링크 트래킹 정보를 가져옵니다.
            var links = await _context.Links
                .OrderByDescending(l => l.Link_Url)
                .ToListAsync();

            return Ok(links);
        }
        // POST: api/Link/Input
        // 링크 정보 등록
        [HttpPost("Input")]
        public async Task<ActionResult<LinkModel>> PostLink([FromBody] LinkModel linkData)
        {
            if (linkData == null)
            {
                return BadRequest("링크 정보가 유효하지 않습니다.");
            }
            // 등록된 링크 개수 확인
            int linkCount = await _context.Links.CountAsync();
            if (linkCount > 0)
            {
                return Conflict(new { success = false, message = "최대 1개만 등록 가능합니다." });
            }

            // 링크 정보를 데이터베이스에 추가합니다.
            _context.Links.Add(linkData);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetLinks), new { id = linkData.Link_Url }, linkData);
        }
        // DELETE: api/Link/{name}
        [HttpDelete("{name}")]
        public async Task<ActionResult<object>> DeleteLink(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return BadRequest(new { success = false, message = "링크 이름이 유효하지 않습니다." });
            }

            var link = await _context.Links.FirstOrDefaultAsync(l => l.Link_Name == name);
            if (link == null)
            {
                return NotFound(new { success = false, message = $"이름이 '{name}'인 링크를 찾을 수 없습니다." });
            }

            _context.Links.Remove(link);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "링크가 성공적으로 삭제되었습니다." });
        }
    }
}
