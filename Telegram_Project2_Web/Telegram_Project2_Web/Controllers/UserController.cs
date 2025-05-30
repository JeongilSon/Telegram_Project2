using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Telegram_Project2_Web.Models;

namespace Telegram_Project2_Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/User
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserModel>>> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        // GET: api/User/{chatId}
        [HttpGet("{chatId}")]
        public async Task<ActionResult<UserModel>> GetUser(string chatId)
        {
            var user = await _context.Users.FindAsync(chatId);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // POST: api/User/{chatId}/MissionComplete
        [HttpPost("{chatId}/MissionComplete")]
        public async Task<IActionResult> UpdateMissionComplete(string chatId, [FromBody] bool isComplete)
        {
            var user = await _context.Users.FindAsync(chatId);

            if (user == null)
            {
                return NotFound();
            }

            user.Mission_Complete = isComplete ? 1 : 0;

            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(user);  // 업데이트된 사용자 정보 반환
        }

        // POST: api/User/{chatId}/ChannelMove
        [HttpPost("{chatId}/ChannelMove")]
        public async Task<IActionResult> UpdateChannelMove(string chatId, [FromBody] bool isMoved)
        {
            var user = await _context.Users.FindAsync(chatId);

            if (user == null)
            {
                return NotFound();
            }

            user.Channel_Move = isMoved ? 1 : 0;

            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        // POST: api/User/{chatId}/LinkMove
        [HttpPost("{chatId}/LinkMove")]
        public async Task<IActionResult> UpdateLinkMove(string chatId, [FromBody] bool isMoved)
        {
            var user = await _context.Users.FindAsync(chatId);

            if (user == null)
            {
                return NotFound();
            }

            user.Link_Move = isMoved ? 1 : 0;

            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(user);
        }
        // POST: api/User/Register
        // 사용자 등록하기
        [HttpPost("Register")]
        public async Task<ActionResult<UserModel>> RegisterUser([FromBody]UserModel user)
        {
            if (user == null || string.IsNullOrEmpty(user.Chat_ID))
            {
                return BadRequest("유효하지 않은 사용자 정보입니다.");
            }

            // 이미 등록된 사용자인지 확인
            var existingUser = await _context.Users.FindAsync(user.Chat_ID);
            if (existingUser != null)
            {
                // 이미 존재하는 사용자면 정보 업데이트
                _context.Entry(existingUser).State = EntityState.Detached;
                _context.Entry(user).State = EntityState.Modified;
            }
            else
            {
                // 신규 사용자면 추가
                _context.Users.Add(user);
            }

            // 초기값 설정 - 필요한 경우
            user.Link_Move = 0;
            user.Channel_Move = 0;
            user.Mission_Complete = 0;

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { chatId = user.Chat_ID }, user);
        }
        //// POST: api/User/Update/{chatId}
        //[HttpPost("Update/{chatId}")]
        //public async Task<IActionResult> UpdateUser(string chatId, UserUpdateModel updateModel)
        //{
        //    var user = await _context.Users.FindAsync(chatId);

        //    if (user == null)
        //    {
        //        return NotFound();
        //    }

        //    // 제공된 값만 업데이트
        //    if (updateModel.MissionComplete.HasValue)
        //        user.mission_complete = updateModel.MissionComplete.Value ? 1 : 0;

        //    if (updateModel.ChannelMove.HasValue)
        //        user.channel_move = updateModel.ChannelMove.Value ? 1 : 0;

        //    if (updateModel.LinkMove.HasValue)
        //        user.link_move = updateModel.LinkMove.Value ? 1 : 0;

        //    if (!string.IsNullOrEmpty(updateModel.Nickname))
        //        user.nickname = updateModel.Nickname;

        //    if (!string.IsNullOrEmpty(updateModel.UserQuestion))
        //        user.user_question = updateModel.UserQuestion;

        //    _context.Entry(user).State = EntityState.Modified;
        //    await _context.SaveChangesAsync();

        //    return Ok(user);  // 업데이트된 사용자 정보 반환
        //}
    }
}
