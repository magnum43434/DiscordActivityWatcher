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
    public class ActivitiesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ActivitiesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Activity
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Activity>>> GetActivities()
        {
            return await _context.Activities.ToListAsync();
        }

        // GET: api/Activity/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Activity>> GetActivity(Guid id)
        {
            var activity = await _context.Activities.FindAsync(id);

            if (activity == null)
            {
                return NotFound();
            }
            
            return activity;
        }

        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<Activity>>> GetUserActivities(Guid userId)
        {
            return await _context.Activities.Where(a => a.UserId == userId).ToListAsync();
        }
        
        [HttpGet("guild")]
        public async Task<ActionResult<IEnumerable<Activity>>> GetGuildActivities(Guid userId, ulong guildId)
        {
            return await _context.Activities.Where(a => a.UserId == userId && a.GuildId == guildId).ToListAsync();
        }
        
        [HttpGet("guildIds")]
        public async Task<ActionResult<IEnumerable<ulong>>> GetGuildIds(Guid userId) {
            return await _context.Activities.Where(a => a.UserId == userId).Select(a => a.GuildId).ToListAsync();
        }
        
        // POST: api/Activity
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Activity>> PostActivity(Activity activity)
        {
            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetActivity", new { id = activity.Id }, activity);
        }

#if DEBUG
        // DELETE: api/Activity/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteActivity(Guid id)
        {
            var activity = await _context.Activities.FindAsync(id);
            if (activity == null)
            {
                return NotFound();
            }

            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync();

            return NoContent();
        }
#endif
    }
}
