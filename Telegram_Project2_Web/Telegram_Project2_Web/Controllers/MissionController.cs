using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Telegram_Project2_Web.Models;

namespace Telegram_Project2_Web.Controllers
{
    //라 버튼
    [Route("api/[controller]")]
    [ApiController]
    public class MissionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MissionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Mission
        // 미션 목록 가져오기
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MissionModel>>> GetAllMission()
        {
            var missions = await _context.Missions.OrderByDescending(m => m.Mission_Name).ToListAsync();

            return Ok(missions);
        }
        // GET: api/Mission/{id}
        // 특정 유저 미션 달성 여부 가져오기
        [HttpGet("{id}")]
        public async Task<ActionResult<MissionModel>> GetUserMission(int id)
        {
            var mission = await _context.Missions.FindAsync(id);
            if (mission == null)
            {
                return NotFound();
            }
            return Ok(mission);
        }

        // POST: api/Mission
        [HttpPost]
        public async Task<ActionResult<object>> PostMission([FromBody] MissionModel mission)
        {
            try
            {
                // 입력 검증
                if (string.IsNullOrEmpty(mission.Mission_Name))
                {
                    return BadRequest(new { success = false, message = "미션 이름은 필수 항목입니다." });
                }

                // 중복 미션 이름 확인
                var existingMission = await _context.Missions
                    .FirstOrDefaultAsync(m => m.Mission_Name == mission.Mission_Name);

                int missionCount = await _context.Missions.CountAsync();
                if (missionCount > 0)
                {
                    return Conflict(new { success = false, message = "최대 1개만 등록 가능합니다." });
                }

                if (existingMission != null)
                {
                    return Conflict(new { success = false, message = "이미 같은 이름의 미션이 존재합니다." });
                }



                // 미션 저장
                _context.Missions.Add(mission);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "미션이 성공적으로 등록되었습니다." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"미션 등록 중 오류가 발생했습니다: {ex.Message}" });
            }
        }
        // DELETE: api/Mission/{name}
        [HttpDelete("{name}")]
        public async Task<ActionResult<object>> DeleteMission(string name)
        {
            try
            {
                // 미션 테이블에서 해당 이름의 미션 조회
                var mission = await _context.Missions
                    .FirstOrDefaultAsync(m => m.Mission_Name == name);

                if (mission == null)
                {
                    return NotFound(new { success = false, message = "미션을 찾을 수 없습니다." });
                }

                // 미션 삭제
                _context.Missions.Remove(mission);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "미션이 성공적으로 삭제되었습니다." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"미션 삭제 중 오류가 발생했습니다: {ex.Message}" });
            }
        }
    }
}
