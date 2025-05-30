using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Telegram_Project2_Web.Models;

namespace LiveScoreChatServiceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }
        // GET: api/Account
        // 링크 목록 가져오기
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccountModel>>> GetAccounts()
        {
            // 모든 링크 트래킹 정보를 가져옵니다.
            var accounts = await _context.Accounts.OrderByDescending(l => l.Id).ToListAsync();
            return Ok(accounts);
        }
        // POST: api/Account/Input
        // 링크 정보 등록
        [HttpPost("Input")]
        public async Task<ActionResult<AccountModel>> PostAccount([FromBody] AccountModel accountData)
        {
            if (accountData == null)
            {
                return BadRequest("계정 정보가 유효하지 않습니다.");
            }
            // 계정 정보를 데이터베이스에 추가합니다.
            _context.Accounts.Add(accountData);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAccounts), new { id = accountData.Id}, accountData);
        }
        // DELETE: api/Account/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> DeleteAccount(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { success = false, message = "링크 이름이 유효하지 않습니다." });
            }

            var account = await _context.Accounts.FirstOrDefaultAsync(l => l.Id == id);
            if (account == null)
            {
                return NotFound(new { success = false, message = $"이름이 '{id}'인 아이디를 찾을 수 없습니다." });
            }

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "링크가 성공적으로 삭제되었습니다." });
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Id) || string.IsNullOrEmpty(request.pw))
            {
                return BadRequest("아이디와 비밀번호를 입력하세요.");
            }

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == request.Id);
            if (account == null || account.Pw != request.pw)
            {
                return Unauthorized("아이디 또는 비밀번호가 일치하지 않습니다.");
            }

            // 로그인 성공
            return Ok(new { success = true, message = "로그인 성공" });
        }
    }

    public class LoginRequest
    {        
        public required string Id { get; set; }
        public required string pw { get; set; }
    }
}
