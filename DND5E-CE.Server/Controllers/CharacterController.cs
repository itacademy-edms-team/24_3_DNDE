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
        private readonly ICharacterService _characterService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CharacterController> _logger;

        public CharacterController(
            DND5EContext context,
            IMapper mapper,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ICharacterService characterService,
            IConfiguration configuration,
            ILogger<CharacterController> logger)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
            _characterService = characterService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("characters")]
        public async Task<IActionResult> CreateCharacter([FromBody] CharacterCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for CharacterCreateDto");
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst("id")?.Value;
            if (userId == null)
            {
                _logger.LogWarning("'id' in access_token are null");
                return Unauthorized("User ID not found.");
            }

            try
            {
                var character = await _characterService.CreateCharacterAsync(dto, userId);
                var response = _mapper.Map<CharacterDto>(character);
                return CreatedAtAction(
                    nameof(GetCharacter),
                    new { cId = character.Id },
                    response
                );
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Not found: {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Operation failed");
                return StatusCode(500, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating character");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("characters")]
        public async Task<IActionResult> GetCharacters()
        {
            var userId = User.FindFirst("id")?.Value;
            if (userId == null)
            {
                _logger.LogWarning("'id' in access_token are null");
                return Unauthorized("User ID not found.");
            }

            try
            {
                var characters = await _characterService.GetCharactersAsync(userId);
                var response = _mapper.Map<ICollection<CharacterSelectItemDto>>(characters);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Not found: {errMsg}",
                    ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving characters");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("characters/{cId}")]
        public async Task<IActionResult> GetCharacter(Guid cId)
        {
            var userId = User.FindFirst("id")?.Value;
            if (userId == null)
            {
                _logger.LogWarning("'id' in access_token are null");
                return Unauthorized("User ID not found.");
            }

            try
            {
                var character = await _characterService.GetCharacterAsync(cId, userId);
                var response = _mapper.Map<CharacterDto>(character);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Not found: {errMsg}",
                    ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving characters");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpDelete("characters/{cId}")]
        public async Task<IActionResult> DeleteCharacter(Guid cId)
        {
            var userId = User.FindFirst("id")?.Value;
            if (userId == null)
            {
                _logger.LogWarning("'id' in access_token are null");
                return Unauthorized("User ID not found.");
            }

            try
            {
                await _characterService.DeleteCharacterAsync(cId, userId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Not found: {errMsg}"
                    , ex.Message);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized: {errMsg}",
                    ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Operation failed");
                return StatusCode(500, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving characters");
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
