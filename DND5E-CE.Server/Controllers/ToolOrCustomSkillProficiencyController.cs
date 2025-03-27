using DND5E_CE.Server.Context;
using DND5E_CE.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DND5E_CE.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ToolOrCustomSkillProficiencyController : ControllerBase
    {
        private readonly DND5EContext _context;

        public ToolOrCustomSkillProficiencyController(DND5EContext context)
        {
            _context = context;
        }

        // GET: api/ToolOrCustomSkillProficiency
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ToolOrCustomSkillProficiency>>> GetToolOrCustomSkillProficiencies()
        {
            var toolOrCustomSkillProficiencyList = await _context.ToolOrCustomSkillProficiencies.ToListAsync();

            if (!toolOrCustomSkillProficiencyList.Any())
            {
                return NotFound();
            }

            return toolOrCustomSkillProficiencyList;
        }

        // GET: api/ToolOrCustomSkillProficiency/byid/{id:int}
        [HttpGet("/byid/{id}")]
        public async Task<ActionResult<ToolOrCustomSkillProficiency>> GetToolOrCustomSkillProficiency(int id)
        {
            var toolOrCustomSkillProficiency = await _context.ToolOrCustomSkillProficiencies.FindAsync(id);

            if (toolOrCustomSkillProficiency == null)
            {
                return NotFound();
            }

            return toolOrCustomSkillProficiency;
        }

        // GET: api/ToolOrCustomSkillProficiency/bycharacter/{characterId}
        [HttpGet("/bycharacter/{characterId}")]
        public async Task<ActionResult<IEnumerable<ToolOrCustomSkillProficiency>>> GetToolOrCustomSkillProficienciesByCharacter(int characterId)
        {
            var toolOrCustomSkillProficiencies = await _context.ToolOrCustomSkillProficiencies
                .Where(t => t.CharacterId == characterId)
                .ToListAsync();

            if (!toolOrCustomSkillProficiencies.Any())
            {
                return NotFound();
            }

            return toolOrCustomSkillProficiencies;
        }

        // POST: api/ToolOrCustomSkillProficiency
        [HttpPost]
        public async Task<ActionResult<ToolOrCustomSkillProficiency>> PostToolOrCustomSkillProficiency(ToolOrCustomSkillProficiency toolOrCustomSkillProficiency)
        {
            if (!await CharacterExists(toolOrCustomSkillProficiency.CharacterId))
            {
                return BadRequest($"Character with ID {toolOrCustomSkillProficiency.CharacterId} does not exist");
            }

            _context.ToolOrCustomSkillProficiencies.Add(toolOrCustomSkillProficiency);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetToolOrCustomSkillProficiency), new { id = toolOrCustomSkillProficiency.Id }, toolOrCustomSkillProficiency);
        }

        // PUT api/ToolOrCustomSkillProficiency/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutToolOrCustomSkillProficiency(int id, ToolOrCustomSkillProficiency toolOrCustomSkillProficiency)
        {
            if (id != toolOrCustomSkillProficiency.Id)
            {
                return BadRequest();
            }

            if (!await CharacterExists(toolOrCustomSkillProficiency.CharacterId))
            {
                return BadRequest($"Character with ID {toolOrCustomSkillProficiency.CharacterId} does not exist");
            }

            _context.Entry(toolOrCustomSkillProficiency).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ToolOrCustomSkillProficiencyExists(id))
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

        // DELETE api/ToolOrCustomSkillProficiency/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteToolOrCustomSkillProficiency(int id)
        {
            var toolOrCustomSkillProficiency = await _context.ToolOrCustomSkillProficiencies.FindAsync(id);
            if (toolOrCustomSkillProficiency == null)
            {
                return NotFound();
            }

            _context.ToolOrCustomSkillProficiencies.Remove(toolOrCustomSkillProficiency);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> ToolOrCustomSkillProficiencyExists(int id)
        {
            return await _context.ToolOrCustomSkillProficiencies.AnyAsync(e => e.Id == id);
        }

        private async Task<bool> CharacterExists(int id)
        {
            return await _context.Characters.AnyAsync(e => e.Id == id);
        }
    }
} 