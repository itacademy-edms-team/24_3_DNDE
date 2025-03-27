using DND5E_CE.Server.Context;
using DND5E_CE.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DND5E_CE.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpellController : ControllerBase
    {
        private readonly DND5EContext _context;

        public SpellController(DND5EContext context)
        {
            _context = context;
        }

        // GET: api/Spell
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Spell>>> GetSpells()
        {
            var spellList = await _context.Spells.ToListAsync();

            if (!spellList.Any())
            {
                return NotFound();
            }

            return spellList;
        }

        // GET: api/Spell/byid/{id:int}
        [HttpGet("/byid/{id}")]
        public async Task<ActionResult<Spell>> GetSpell(int id)
        {
            var spell = await _context.Spells.FindAsync(id);

            if (spell == null)
            {
                return NotFound();
            }

            return spell;
        }

        // GET: api/Spell/bycharacter/{id:int}
        [HttpGet("/bycharacter/{id}")]
        public async Task<ActionResult<IEnumerable<Spell>>> GetSpellsByCharacter(int id)
        {
            var spells = await _context.Spells
                .Where(s => s.CharacterId == id)
                .ToListAsync();

            if (!spells.Any())
            {
                return NotFound();
            }

            return spells;
        }

        // POST: api/Spell
        [HttpPost]
        public async Task<ActionResult<Spell>> PostSpell(Spell spell)
        {
            if (!await CharacterExists(spell.CharacterId))
            {
                return BadRequest($"Character with ID {spell.CharacterId} does not exist");
            }

            _context.Spells.Add(spell);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSpell), new { id = spell.Id }, spell);
        }

        // PUT api/Spell/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSpell(int id, Spell spell)
        {
            if (id != spell.Id)
            {
                return BadRequest();
            }

            if (!await CharacterExists(spell.CharacterId))
            {
                return BadRequest($"Character with ID {spell.CharacterId} does not exist");
            }

            _context.Entry(spell).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await SpellExists(id))
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

        // DELETE api/Spell/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSpell(int id)
        {
            var spell = await _context.Spells.FindAsync(id);
            if (spell == null)
            {
                return NotFound();
            }

            _context.Spells.Remove(spell);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> SpellExists(int id)
        {
            return await _context.Spells.AnyAsync(e => e.Id == id);
        }

        private async Task<bool> CharacterExists(int id)
        {
            return await _context.Characters.AnyAsync(e => e.Id == id);
        }
    }
} 