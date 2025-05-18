using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.ApplicationDbContext;
using Library.Models;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimeSpentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TimeSpentController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/TimeSpent
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TimeSpent>>> GetTimeSpent()
        {
            return await _context.TimeSpent.ToListAsync();
        }

        // GET: api/TimeSpent/5
        [HttpGet("guildId/{guildId}")]
        public async Task<ActionResult<IEnumerable<TimeSpent>>> GetTimeSpentByGuild(ulong guildId)
        {
            return await _context.TimeSpent.Where(x => x.GuildId == guildId).ToListAsync();
        }

        [HttpGet("userId/{userId}")]
        public async Task<ActionResult<IEnumerable<TimeSpent>>> GetTimeSpentByUserId(Guid userId)
        {
           return await _context.TimeSpent.Where(x => x.UserId == userId).ToListAsync();
        }

        [HttpGet("userId/{userId}/guildId/{guildId}")]
        public async Task<ActionResult<TimeSpent>> GetTimeSpentByUserIdAndGuildId(Guid userId, ulong guildId)
        {
            var timeSpent = await _context.TimeSpent.FirstOrDefaultAsync(x => x.UserId == userId && x.GuildId == guildId);

            if (timeSpent == null)
            {
                return NotFound();
            }
            
            return timeSpent;
        }
        
        [HttpGet("topten/{guildId}")]
        public async Task<ActionResult<IEnumerable<TimeSpent>>> GetTopTenUsers(ulong guildId)
        {
            return await _context.TimeSpent.Where(x => x.GuildId == guildId).OrderByDescending(u => u.MinutesActiv).Take(10).ToListAsync();
        }

        // PUT: api/TimeSpent/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTimeSpent(Guid id, TimeSpent timeSpent)
        {
            if (id != timeSpent.Id)
            {
                return BadRequest();
            }

            _context.Entry(timeSpent).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TimeSpentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/TimeSpent
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TimeSpent>> PostTimeSpent(TimeSpent timeSpent)
        {
            _context.TimeSpent.Add(timeSpent);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTimeSpent", new { id = timeSpent.Id }, timeSpent);
        }

#if DEBUG
        // DELETE: api/TimeSpent/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTimeSpent(Guid id)
        {
            var timeSpent = await _context.TimeSpent.FindAsync(id);
            if (timeSpent == null)
            {
                return NotFound();
            }

            _context.TimeSpent.Remove(timeSpent);
            await _context.SaveChangesAsync();

            return NoContent();
        }
#endif

        private bool TimeSpentExists(Guid id)
        {
            return _context.TimeSpent.Any(e => e.Id == id);
        }
    }
}
