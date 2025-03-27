using DND5E_CE.Server.Context;
using DND5E_CE.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DND5E_CE.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttackAndSpellcastingController : ControllerBase
    {
        private readonly DND5EContext _context;

        public AttackAndSpellcastingController(DND5EContext context)
        {
            _context = context;
        }

        // GET: api/AttackAndSpellcasting
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AttackAndSpellcasting>>> GetAttackAndSpellcastings()
        {
            var attackAndSpellcastingList = await _context.AttackAndSpellcastings.ToListAsync();

            if (!attackAndSpellcastingList.Any())
            {
                return NotFound();
            }

            return attackAndSpellcastingList;
        }

        // GET: api/AttackAndSpellcasting/byid/{id:int}
        [HttpGet("/byid/{id}")]
        public async Task<ActionResult<AttackAndSpellcasting>> GetAttackAndSpellcastingById(int id)
        {
            var attackAndSpellcasting = await _context.AttackAndSpellcastings.FindAsync(id);

            if (attackAndSpellcasting == null)
            {
                return NotFound();
            }

            return attackAndSpellcasting;
        }

        // GET: api/AttackAndSpellcasting/bycharacter/{id:int}
        [HttpGet("/bycharacter/{id}")]
        public async Task<ActionResult<IEnumerable<AttackAndSpellcasting>>> GetAttackAndSpellcastingByCId(int id)
        {
            var attackAndSpellcastingList = await _context.AttackAndSpellcastings.Where(a => a.CharacterId == id).ToListAsync();

            if (!attackAndSpellcastingList.Any())
            {
                return NotFound();
            }

            return attackAndSpellcastingList;
        }

        // POST: api/AttackAndSpellcasting
        [HttpPost]
        public async Task<ActionResult<AttackAndSpellcasting>> PostAttackAndSpellcasting(AttackAndSpellcasting attackAndSpellcasting)
        {
            if (!await CharacterExists(attackAndSpellcasting.CharacterId))
            {
                return BadRequest($"Character with ID {attackAndSpellcasting.CharacterId} does not exist");
            }

            _context.AttackAndSpellcastings.Add(attackAndSpellcasting);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAttackAndSpellcastingById), new { id = attackAndSpellcasting.Id }, attackAndSpellcasting);
        }

        // PUT api/AttackAndSpellcasting/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAttackAndSpellcasting(int id, AttackAndSpellcasting attackAndSpellcasting)
        {
            if (id != attackAndSpellcasting.Id)
            {
                return BadRequest();
            }

            if (!await CharacterExists(attackAndSpellcasting.CharacterId))
            {
                return BadRequest($"Character with ID {attackAndSpellcasting.CharacterId} does not exist");
            }

            _context.Entry(attackAndSpellcasting).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await AttackAndSpellcastingExists(id))
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

        // DELETE api/AttackAndSpellcasting/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAttackAndSpellcasting(int id)
        {
            var attackAndSpellcasting = await _context.AttackAndSpellcastings.FindAsync(id);
            if (attackAndSpellcasting == null)
            {
                return NotFound();
            }

            _context.AttackAndSpellcastings.Remove(attackAndSpellcasting);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> AttackAndSpellcastingExists(int id)
        {
            return await _context.AttackAndSpellcastings.AnyAsync(e => e.Id == id);
        }

        private async Task<bool> CharacterExists(int id)
        {
            return await _context.Characters.AnyAsync(e => e.Id == id);
        }
    }
} 