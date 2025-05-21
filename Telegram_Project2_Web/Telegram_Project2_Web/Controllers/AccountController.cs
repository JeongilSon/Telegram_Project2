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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Id) || string.IsNullOrEmpty(request.pw))
            {
                return BadRequest("아이디와 비밀번호를 입력하세요.");
            }

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == request.Id);
            Console.WriteLine(account);
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
        public string Id { get; set; }
        public string pw { get; set; }
    }
}
