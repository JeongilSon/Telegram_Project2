using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Telegram_Project2_Web.Models;

namespace Telegram_Project2_Web.Controllers
{
    //나 버튼
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public QuestionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/question
        //모든 질문 가져오기
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> GetQuestions()
        {
            var questions = await _context.Users
                .OrderByDescending(q => q.User_Question)
                .ToListAsync();

            return Ok(questions);
        }
        // POST: api/Question/UserQuestion
        // 유저 별 답변 응답 등록
        [HttpPost("UserQuestion")]
        public async Task<IActionResult> UpdateUserQuestion([FromBody] QuestionRequest request)
        {
            if (request== null || string.IsNullOrEmpty(request.ChatId) || string.IsNullOrEmpty(request.UserQuestion))
            {
                return BadRequest("사용자 ID와 내용은 필수입니다.");
            }

            // 사용자 찾기
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Chat_ID == request.ChatId);
            if (user == null)
            {
                return NotFound("해당 사용자를 찾을 수 없습니다.");
            }

            // 사용자 질문 업데이트
            user.User_Question = request.UserQuestion;
            _context.Entry(user).State = EntityState.Modified;
            // 변경사항 저장
            await _context.SaveChangesAsync();

            return Ok(user);
        }
    }
    public class QuestionRequest
    {
        public string ChatId { get; set; }
        public string UserQuestion { get; set; }
    }
}

