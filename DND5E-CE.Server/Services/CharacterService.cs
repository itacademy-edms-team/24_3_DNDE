using AutoMapper;
using DND5E_CE.Server.Data;
using DND5E_CE.Server.DTO.App;
using DND5E_CE.Server.Models.App;
using DND5E_CE.Server.Models.App.Sheet1;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace DND5E_CE.Server.Services
{
    public interface ICharacterService
    {
        Task<CharacterModel> CreateCharacterAsync(CharacterCreateDto dto, string userId);

        Task<CharacterModel> GetCharacterAsync(Guid characterId, string userId);

        Task<ICollection<CharacterModel>> GetCharactersAsync(string userId);

        Task DeleteCharacterAsync(Guid characterId, string userId);
    }

    public class CharacterService : ICharacterService
    {
        private readonly DND5EContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CharacterService> _logger;

        public CharacterService(DND5EContext context, IMapper mapper, ILogger<CharacterService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CharacterModel> CreateCharacterAsync(CharacterCreateDto dto, string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("'userId' is null or empty");
                throw new UnauthorizedAccessException("User ID not found.");
            }

            // Beginning transaction
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Creating Sheet1
                // Mapping of non-service fields for Sheet1 dependant tables
                var ability = new AbilityModel();
                var abilitySaveThrow = new AbilitySaveThrowModel();
                var skill = new SkillModel();
                var hitPoint = new HitPointModel();
                var hitDice = new HitDiceModel();
                var deathSaveThrow = new DeathSaveThrowModel();
                var inventoryGold = new InventoryGoldModel();
                var classResource = new ClassResourceModel();

                // Mapping of non-service fields for Sheet1
                var sheet1 = _mapper.Map<Sheet1Model>(dto.Sheet1);

                // Configuring relationships (service fields) between Sheet1 and its dependant tables
                sheet1.AbilityId = ability.Id;
                sheet1.Ability = ability;
                sheet1.SkillId = skill.Id;
                sheet1.Skill = skill;
                sheet1.HitPointId = hitPoint.Id;
                sheet1.HitPoint = hitPoint;
                sheet1.HitDiceId = hitDice.Id;
                sheet1.HitDice = hitDice;
                sheet1.DeathSaveThrowId = deathSaveThrow.Id;
                sheet1.DeathSaveThrow = deathSaveThrow;
                sheet1.AbilitySaveThrowId = abilitySaveThrow.Id;
                sheet1.AbilitySaveThrow = abilitySaveThrow;
                sheet1.Tool = new List<ToolModel>();
                sheet1.OtherTool = new List<OtherToolModel>();

                sheet1.Attack = new List<AttackModel>();
                sheet1.GlobalDamageModifier = new List<GlobalDamageModifierModel>();
                sheet1.InventoryGoldId = inventoryGold.Id;
                sheet1.InventoryGold = inventoryGold;
                sheet1.InventoryItem = new List<InventoryItemModel>();

                sheet1.ClassResourceId = classResource.Id;
                sheet1.ClassResource = classResource;
                sheet1.OtherResource = new List<OtherResourceModel>();

                // Create empty character and provide userId
                var character = new CharacterModel();
                character.UserId = userId;
                // "character.User = user" is no need because user already exists

                // Configuring relationships (service fields) between Character and Sheet1
                character.Sheet1Id = sheet1.Id;
                character.Sheet1 = sheet1;
                sheet1.CharacterId = character.Id;
                sheet1.Character = character;

                // Creating Sheet2
                var sheet2 = new Sheet2Model();
                character.Sheet2Id = sheet2.Id;
                character.Sheet2 = sheet2;
                sheet2.CharacterId = character.Id;
                sheet2.Character = character;

                // Creating Sheet3
                var sheet3 = new Sheet3Model();
                character.Sheet3Id = sheet3.Id;
                character.Sheet3 = sheet3;
                sheet3.CharacterId = character.Id;
                sheet3.Character = character;

                await _context.Character.AddAsync(character);

                await _context.SaveChangesAsync();
                
                // End(Fixation) of transaction
                await transaction.CommitAsync();
                _logger.LogDebug("Character with Id '{CharacterId}' created", character.Id);

                return character;
            }
            catch (KeyNotFoundException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning("Not found: {Message}", ex.Message);
                throw;
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to save character with Id");
                throw new InvalidOperationException("Failed to create character due to database error.", ex);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Unexpected error during character creation");
                throw new InvalidOperationException("Unexpected error during character creation.", ex);
            }
        }

        public async Task<CharacterModel> GetCharacterAsync(Guid characterId, string userId)
        {
            var character = await _context.Character
                .Include(c => c.Sheet1)
                    .ThenInclude(s => s.Ability)
                .Include(c => c.Sheet1)
                    .ThenInclude(s => s.Skill)
                .Include(c => c.Sheet1)
                    .ThenInclude(s => s.HitPoint)
                .Include(c => c.Sheet1)
                    .ThenInclude(s => s.HitDice)
                .Include(c => c.Sheet1)
                    .ThenInclude(s => s.DeathSaveThrow)
                .Include(c => c.Sheet1)
                    .ThenInclude(s => s.AbilitySaveThrow)
                .Include(c => c.Sheet1)
                    .ThenInclude(s => s.Tool)
                .Include(c => c.Sheet1)
                    .ThenInclude(s => s.OtherTool)
                .Include(c => c.Sheet1)
                    .ThenInclude(s => s.Attack)
                .Include(c => c.Sheet1)
                    .ThenInclude(s => s.GlobalDamageModifier)
                .Include(c => c.Sheet1)
                    .ThenInclude(s => s.InventoryGold)
                .Include(c => c.Sheet1)
                    .ThenInclude(s => s.InventoryItem)
                .Include(c => c.Sheet1)
                    .ThenInclude(s => s.ClassResource)
                .Include(c => c.Sheet1)
                    .ThenInclude(s => s.OtherResource)
                .Include(c => c.Sheet2)
                .Include(c => c.Sheet3)
                .FirstOrDefaultAsync(c => c.Id.Equals(characterId));

            if (character == null)
            {
                _logger.LogWarning("Character with id '{characterId}' not found", characterId);
                throw new KeyNotFoundException("Character not found.");
            }

            return character;
        }

        public async Task<ICollection<CharacterModel>> GetCharactersAsync(string userId)
        {
            var characters = await _context.Character
                .Where(c => c.UserId.Equals(userId))
                .Include(c => c.Sheet1)
                    .ThenInclude(s => s.Ability)
                .Include(c => c.Sheet1)
                    .ThenInclude(s => s.Skill)
                .Include(c => c.Sheet1)
                    .ThenInclude(s => s.HitPoint)
                .Include(c => c.Sheet1)
                    .ThenInclude(s => s.HitDice)
                .Include(c => c.Sheet1)
                    .ThenInclude(s => s.DeathSaveThrow)
                .Include(c => c.Sheet1)
                    .ThenInclude(s => s.AbilitySaveThrow)
                .Include(c => c.Sheet1)
                    .ThenInclude(s => s.Tool)
                .Include(c => c.Sheet1)
                    .ThenInclude(s => s.OtherTool)
                .Include(c => c.Sheet1)
                    .ThenInclude(s => s.Attack)
                .Include(c => c.Sheet1)
                    .ThenInclude(s => s.GlobalDamageModifier)
                .Include(c => c.Sheet1)
                    .ThenInclude(s => s.InventoryGold)
                .Include(c => c.Sheet1)
                    .ThenInclude(s => s.InventoryItem)
                .Include(c => c.Sheet1)
                    .ThenInclude(s => s.ClassResource)
                .Include(c => c.Sheet1)
                    .ThenInclude(s => s.OtherResource)
                .Include(c => c.Sheet2)
                .Include(c => c.Sheet3)
                .ToListAsync();

            if (!characters.Any())
            {
                _logger.LogWarning("No characters found for User '{UserId}'", userId);
                throw new KeyNotFoundException("Characters not found.");
            }

            return characters;
        }

        public async Task DeleteCharacterAsync(Guid characterId, string userId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Find character
                var character = await GetCharacterAsync(characterId, userId);

                // Check user own rights
                if (character.UserId != userId)
                {
                    _logger.LogWarning("User '{UserId}' not authorized to delete Character '{CharacterId}'",
                        userId, characterId);
                    throw new UnauthorizedAccessException("User not authorized to delete this character.");
                }

                // Deleting character (Cascade deletion configured in DBContext)
                _context.Character.Remove(character);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                _logger.LogDebug("Character '{CharacterId}' deleted", characterId);
            }
            catch (KeyNotFoundException)
            {
                await transaction.RollbackAsync();
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                await transaction.RollbackAsync();
                throw;
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to delete Character '{CharacterId}'",
                    characterId);
                throw new InvalidOperationException("Failed to delete character due to database error.", ex);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Unexpected error during character deletion '{CharacterId}'",
                    characterId);
                throw new InvalidOperationException("Unexpected error during character deletion", ex);
            }
        }
    }
}
