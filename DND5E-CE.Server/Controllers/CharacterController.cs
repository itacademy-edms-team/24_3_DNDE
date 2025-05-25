using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DND5E_CE.Server.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Authorization;
using DND5E_CE.Server.Services;
using Microsoft.EntityFrameworkCore;
using DND5E_CE.Server.Data;
using DND5E_CE.Server.DTO.App;
using DND5E_CE.Server.Models.App;
using AutoMapper;

namespace DND5E_CE.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CharacterController : ControllerBase
    {
        private readonly DND5EContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<IdentityUser> _userManager;
        ICsrfTokenService _csrfTokenService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CharacterController> _logger;

        public CharacterController(
            DND5EContext context,
            IMapper mapper,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ICsrfTokenService csrfTokenService,
            IConfiguration configuration,
            ILogger<CharacterController> logger)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
            _csrfTokenService = csrfTokenService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("characters")]
        public async Task<IActionResult> CreateCharacter([FromBody] CharacterCreateDto dto)
        {
            var uId = User.FindFirst("id")?.Value;

            if (uId == null)
            {
                _logger.LogWarning("'id' in access_token are null");
                return Unauthorized("User ID not found.");
            }

            var userData = await _context.Users
                .Where(u => u.Id == uId)
                .Select(u => new { u.UserName, u.Email })
                .FirstOrDefaultAsync();

            if (userData == null)
            {
                _logger.LogWarning("User with Id '{Id}' not found", uId);
                return NotFound("User not found.");
            }

            var sheet1 = new Sheet1Model
            {
                UserId = uId,
                Name = dto.Sheet1.Name,
                Level = dto.Sheet1.Level,
                Class = dto.Sheet1.Class
            };
            _context.Sheet1.Add(sheet1);

            Sheet2Model sheet2 = null;
            if (dto.Sheet2 != null)
            {
                sheet2 = new Sheet2Model
                {
                    UserId = uId,
                    Age = dto.Sheet2.Age,
                    Height = dto.Sheet2.Height,
                    Weight = dto.Sheet2.Weight,
                    Eyes = dto.Sheet2.Eyes,
                    Skin = dto.Sheet2.Skin,
                    Hair = dto.Sheet2.Hair,
                    Appearance = dto.Sheet2.Appearance,
                    Backstory = dto.Sheet2.Backstory,
                    AlliesAndOrganizations = dto.Sheet2.AlliesAndOrganizations,
                    AdditionalFeaturesAndTraits = dto.Sheet2.AdditionalFeaturesAndTraits,
                    Treasures = dto.Sheet2.Treasures
                };
                _context.Sheet2.Add(sheet2);
            }
            else
            {
                sheet2 = new Sheet2Model
                {
                    UserId = uId,
                };
            }

            Sheet3Model sheet3 = null;
            if (dto.Sheet3 != null)
            {
                sheet3 = new Sheet3Model
                {
                    UserId = uId,
                    SpellBondAbility = dto.Sheet3.SpellBondAbility,
                    RemainingSpellSlotsLevel1 = dto.Sheet3.RemainingSpellSlotsLevel1,
                    RemainingSpellSlotsLevel2 = dto.Sheet3.RemainingSpellSlotsLevel2,
                    RemainingSpellSlotsLevel3 = dto.Sheet3.RemainingSpellSlotsLevel3,
                    RemainingSpellSlotsLevel4 = dto.Sheet3.RemainingSpellSlotsLevel4,
                    RemainingSpellSlotsLevel5 = dto.Sheet3.RemainingSpellSlotsLevel5,
                    RemainingSpellSlotsLevel6 = dto.Sheet3.RemainingSpellSlotsLevel6,
                    RemainingSpellSlotsLevel7 = dto.Sheet3.RemainingSpellSlotsLevel7,
                    RemainingSpellSlotsLevel8 = dto.Sheet3.RemainingSpellSlotsLevel8,
                    RemainingSpellSlotsLevel9 = dto.Sheet3.RemainingSpellSlotsLevel9
                };
                _context.Sheet3.Add(sheet3);
            }
            else
            {
                sheet3 = new Sheet3Model
                {
                    UserId = uId
                };
            }

            // Save sheets to generate IDs
            await _context.SaveChangesAsync();
            _logger.LogDebug("Sheets with Id's '{sId1}', '{sId2}', '{sId3}' were created",
                sheet1.Id, sheet2.Id, sheet3.Id);
            
            var character = new CharacterModel
            {
                UserId = uId,
                Sheet1Id = sheet1.Id,
                Sheet1 = sheet1,
                Sheet2Id = sheet2.Id,
                Sheet2 = sheet2,
                Sheet3Id = sheet3.Id,
                Sheet3 = sheet3
            };
            _context.Character.Add(character);

            await _context.SaveChangesAsync();
            _logger.LogDebug("Character with Id '{cId}' created",
                character.Id);

            var response = _mapper.Map<CharacterListDto>(character);
            return CreatedAtAction(nameof(GetCharacter), new { cId = character.Id }, response);
        }

        [HttpGet("characters")]
        public async Task<IActionResult> GetCharacters()
        {
            var uId = User.FindFirst("id")?.Value;

            if (uId == null)
            {
                _logger.LogWarning("'id' in access_token are null");
                return Unauthorized("User ID not found.");
            }

            var userData = await _context.Users
                .Where(u => u.Id == uId)
                .Select(u => new { u.UserName, u.Email })
                .FirstOrDefaultAsync();

            if (userData == null)
            {
                _logger.LogDebug("User with Id {Id} not found", uId);
                return NotFound("User not found.");
            }

            var characters = await _context.Character
                .Include(c => c.Sheet1)
                .Where(c => c.UserId == uId)
                .ToListAsync();

            if (!characters.Any())
            {
                _logger.LogDebug("Characters bound with User Id '{Id}' not found.", uId);
                return NotFound("Characters not found.");
            }

            var characterListDto = _mapper.Map<List<CharacterListDto>>(characters);
            return Ok(characterListDto);
        }

        [HttpGet("characters/{cId}")]
        public async Task<IActionResult> GetCharacter(int cId)
        {
            var uId = User.FindFirst("id")?.Value;

            if (uId == null)
            {
                _logger.LogWarning("'sub' in access_token are null");
                return Unauthorized("User ID not found.");
            }

            var userData = await _context.Users
                .Where(u => u.Id == uId)
                .Select(u => new { u.UserName, u.Email })
                .FirstOrDefaultAsync();

            if (userData == null)
            {
                _logger.LogDebug("User with Id '{Id}' not found", uId);
                return NotFound("User not found.");
            }

            var character = await _context.Character
                .Include(c => c.Sheet1)
                .Include(c => c.Sheet2)
                .Include(c => c.Sheet3)
                .Where(c => c.UserId == uId && c.Id == cId)
                .FirstOrDefaultAsync();

            if (character == null)
            {
                _logger.LogDebug("Character with Id '{}' bound with User Id '{}' not found.",
                    cId, uId);
                return NotFound("Character not found.");
            }

            var characterDto = _mapper.Map<CharacterDto>(character);
            return Ok(characterDto);
        }

        [HttpDelete("characters/{id}")]
        public async Task<IActionResult> DeleteCharacter()
        {
            var uId = User.FindFirst("id")?.Value;

            if (uId == null)
            {
                _logger.LogWarning("'sub' in access_token are null");
                return Unauthorized("User ID not found.");
            }

            var userData = await _context.Users
                .Where(u => u.Id == uId)
                .Select(u => new { u.UserName, u.Email })
                .FirstOrDefaultAsync();

            if (userData == null)
            {
                _logger.LogDebug("User with Id '{Id}' not found", uId);
                return NotFound("User not found.");
            }

            var characterToDelete = await _context.Character.FirstOrDefaultAsync(c => c.UserId == uId);

            if (characterToDelete == null)
            {
                _logger.LogDebug("Character with User Id '{uId}' not found.",
                    uId);
                return NotFound($"Character with User Id '{uId}' not found.");
            }

            _context.Character.Remove(characterToDelete);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
