using DND5E_CE.Server.Context;
using DND5E_CE.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DND5E_CE.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OtherProfiencyOrLanguageController : ControllerBase
    {
        private readonly DND5EContext _context;

        public OtherProfiencyOrLanguageController(DND5EContext context)
        {
            _context = context;
        }

        // GET: api/OtherProfiencyOrLanguage
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OtherProfiencyOrLanguage>>> GetOtherProfiencyOrLanguages()
        {
            var otherProfiencyOrLanguageList = await _context.OtherProfiencyOrLanguages.ToListAsync();

            if (!otherProfiencyOrLanguageList.Any())
            {
                return NotFound();
            }

            return otherProfiencyOrLanguageList;
        }

        // GET: api/OtherProfiencyOrLanguage/byid/{id:int}
        [HttpGet("/byid/{id}")]
        public async Task<ActionResult<OtherProfiencyOrLanguage>> GetOtherProfiencyOrLanguage(int id)
        {
            var otherProfiencyOrLanguage = await _context.OtherProfiencyOrLanguages.FindAsync(id);

            if (otherProfiencyOrLanguage == null)
            {
                return NotFound();
            }

            return otherProfiencyOrLanguage;
        }

        // GET: api/OtherProfiencyOrLanguage/bycharacter/{id:int}
        [HttpGet("/bycharacter/{characterId}")]
        public async Task<ActionResult<IEnumerable<OtherProfiencyOrLanguage>>> GetOtherProfiencyOrLanguagesByCharacter(int characterId)
        {
            var otherProfiencyOrLanguageList = await _context.OtherProfiencyOrLanguages
                .Where(o => o.CharacterId == characterId)
                .ToListAsync();

            if (!otherProfiencyOrLanguageList.Any())
            {
                return NotFound();
            }

            return otherProfiencyOrLanguageList;
        }

        // POST: api/OtherProfiencyOrLanguage
        [HttpPost]
        public async Task<ActionResult<OtherProfiencyOrLanguage>> PostOtherProfiencyOrLanguage(OtherProfiencyOrLanguage otherProfiencyOrLanguage)
        {
            if (!await CharacterExists(otherProfiencyOrLanguage.CharacterId))
            {
                return BadRequest($"Character with ID {otherProfiencyOrLanguage.CharacterId} does not exist");
            }

            _context.OtherProfiencyOrLanguages.Add(otherProfiencyOrLanguage);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOtherProfiencyOrLanguage), new { id = otherProfiencyOrLanguage.Id }, otherProfiencyOrLanguage);
        }

        // PUT api/OtherProfiencyOrLanguage/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOtherProfiencyOrLanguage(int id, OtherProfiencyOrLanguage otherProfiencyOrLanguage)
        {
            if (id != otherProfiencyOrLanguage.Id)
            {
                return BadRequest();
            }

            if (!await CharacterExists(otherProfiencyOrLanguage.CharacterId))
            {
                return BadRequest($"Character with ID {otherProfiencyOrLanguage.CharacterId} does not exist");
            }

            _context.Entry(otherProfiencyOrLanguage).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await OtherProfiencyOrLanguageExists(id))
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

        // DELETE api/OtherProfiencyOrLanguage/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOtherProfiencyOrLanguage(int id)
        {
            var otherProfiencyOrLanguage = await _context.OtherProfiencyOrLanguages.FindAsync(id);
            if (otherProfiencyOrLanguage == null)
            {
                return NotFound();
            }

            _context.OtherProfiencyOrLanguages.Remove(otherProfiencyOrLanguage);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> OtherProfiencyOrLanguageExists(int id)
        {
            return await _context.OtherProfiencyOrLanguages.AnyAsync(e => e.Id == id);
        }

        private async Task<bool> CharacterExists(int id)
        {
            return await _context.Characters.AnyAsync(e => e.Id == id);
        }
    }
} 