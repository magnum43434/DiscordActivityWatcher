using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.ApplicationDbContext;
using Library.Models;
using Library.Utils;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MonthTimeSpentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MonthTimeSpentController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/MonthTimeSpent
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<MonthTimeSpent>>> GetMonthTimeSpent()
        {
            return await _context.MonthTimeSpent.ToListAsync();
        }

        // GET: api/MonthTimeSpent/5
        [HttpGet("single")]
        public async Task<ActionResult<MonthTimeSpent>> GetMonthTimeSpent(Guid userId, ulong guildId, int monthId)
        {
            var monthTimeSpent = await _context.MonthTimeSpent.FirstOrDefaultAsync(x => x.UserId == userId && x.GuildId == guildId && x.MonthId == monthId);

            if (monthTimeSpent == null)
            {
                return NotFound();
            }

            return monthTimeSpent;
        }

        [HttpGet("lastMonthId")]
        public ActionResult<int> GetLastMonthId()
        {
            return MonthIdGenerator.GetLastMonthId();
        }
        
        [HttpGet("currentMonthId")]
        public ActionResult<int> GetCurrentMonthId()
        {
            return MonthIdGenerator.GetCurrentMonthId();
        }

        // PUT: api/MonthTimeSpent/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMonthTimeSpent(Guid id, MonthTimeSpent monthTimeSpent)
        {
            if (id != monthTimeSpent.Id)
            {
                return BadRequest();
            }

            _context.Entry(monthTimeSpent).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MonthTimeSpentExists(id))
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

        // POST: api/MonthTimeSpent
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<MonthTimeSpent>> PostMonthTimeSpent(MonthTimeSpent monthTimeSpent)
        {
            _context.MonthTimeSpent.Add(monthTimeSpent);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMonthTimeSpent", new { id = monthTimeSpent.Id }, monthTimeSpent);
        }

        // DELETE: api/MonthTimeSpent/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMonthTimeSpent(Guid id)
        {
            var monthTimeSpent = await _context.MonthTimeSpent.FindAsync(id);
            if (monthTimeSpent == null)
            {
                return NotFound();
            }

            _context.MonthTimeSpent.Remove(monthTimeSpent);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MonthTimeSpentExists(Guid id)
        {
            return _context.MonthTimeSpent.Any(e => e.Id == id);
        }
    }
}
