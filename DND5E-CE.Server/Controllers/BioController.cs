using DND5E_CE.Server.Context;
using DND5E_CE.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DND5E_CE.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BioController : ControllerBase
    {
        private readonly DND5EContext _context;

        public BioController(DND5EContext context)
        {
            _context = context;
        }

        // GET: api/Bio/
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Bio>>> GetBios()
        {
            var bioList = await _context.Bios.ToListAsync();

            if (!bioList.Any())
            {
                return NotFound();
            }

            return bioList;
        }

        // GET: api/Bio/{id:int}
        [HttpGet("{id}")]
        public async Task<ActionResult<Bio>> GetBio(int id)
        {
            if (!await CharacterExists(id))
            {
                return NotFound($"Character with ID {id} not exist in DB");
            }

            var bio = await _context.Bios.FindAsync(id);

            if (bio == null)
            {
                return NotFound();
            }

            return bio;
        }

        // POST: api/Bio
        [HttpPost]
        public async Task<ActionResult<Bio>> PostBio(Bio bio)
        {
            if (await BioExists(bio.CharacterId))
            {
                return BadRequest("Already exist");
            }

            _context.Bios.Add(bio);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBio), new { character_id = bio.CharacterId }, bio);
        }

        // PUT api/Bio/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBio(int id, Bio bio)
        {
            if (id != bio.CharacterId)
            {
                return BadRequest();
            }

            if (!await CharacterExists(id))
            {
                return BadRequest($"User with ID {id} don't exist");
            }

            _context.Entry(bio).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await BioExists(id))
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

        // DELETE api/Bio/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBio(int id)
        {
            var bio = await _context.Bios.FindAsync(id);
            if (bio == null)
            {
                return NotFound();
            }

            _context.Bios.Remove(bio);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> CharacterExists(int id)
        {
            return await _context.Characters.AnyAsync(e => e.Id == id);
        }

        private async Task<bool> BioExists(int id)
        {
            return await _context.Bios.AnyAsync(e => e.Character.Id == id);
        }
    }
} 