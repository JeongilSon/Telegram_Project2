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

        }
        // GET: api/Mission/{id}
        // 특정 유저 미션 정보 가져오기
        [HttpGet("{id}")]
        public async Task<ActionResult<MissionModel>> GetUserMission(int id)
        {

        }

        // POST: api/Mission
        // 미션 등록하기
        [HttpPost]
        public async Task<ActionResult<MissionModel>> MissionPost(MissionModel attendance)
        {

        }
    }
}
