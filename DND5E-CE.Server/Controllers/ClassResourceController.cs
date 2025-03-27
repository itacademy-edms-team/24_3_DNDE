using DND5E_CE.Server.Context;
using DND5E_CE.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DND5E_CE.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClassResourceController : ControllerBase
    {
        private readonly DND5EContext _context;

        public ClassResourceController(DND5EContext context)
        {
            _context = context;
        }

        // GET: api/ClassResource
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClassResource>>> GetClassResources()
        {
            var classResourcesList = await _context.ClassResources.ToListAsync();

            if (!classResourcesList.Any())
            {
                return NotFound();
            }

            return classResourcesList;
        }

        // GET: api/ClassResource/{id:int}
        [HttpGet("{id}")]
        public async Task<ActionResult<ClassResource>> GetClassResource(int id)
        {
            if (!await CharacterExists(id))
            {
                return BadRequest($"User with ID {id} don't exist");
            }

            var classResource = await _context.ClassResources.FindAsync(id);

            if (classResource == null)
            {
                return NotFound();
            }

            return classResource;
        }

        // POST: api/ClassResource
        [HttpPost]
        public async Task<ActionResult<ClassResource>> PostClassResource(ClassResource classResource)
        {
            if (await ClassResourceExists(classResource.CharacterId))
            {
                return BadRequest("Already exists");
            }

            _context.ClassResources.Add(classResource);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetClassResource), new { character_id = classResource.CharacterId }, classResource);
        }

        // PUT api/ClassResource/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutClassResource(int id, ClassResource classResource)
        {
            if (id != classResource.CharacterId)
            {
                return BadRequest();
            }

            if (!await CharacterExists(id))
            {
                return BadRequest($"User with ID {id} don't exist");
            }

            _context.Entry(classResource).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ClassResourceExists(id))
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

        // DELETE api/ClassResource/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClassResource(int id)
        {
            var classResource = await _context.ClassResources.FindAsync(id);
            if (classResource == null)
            {
                return NotFound();
            }

            _context.ClassResources.Remove(classResource);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> ClassResourceExists(int id)
        {
            return await _context.ClassResources.AnyAsync(e => e.CharacterId == id);
        }

        private async Task<bool> CharacterExists(int id)
        {
            return await _context.Characters.AnyAsync(e => e.Id == id);
        }
    }
} 