import { AbilityDto, AbilitySaveThrowDto, AttackDto, ClassResourceDto, GlobalDamageModifierDto, InventoryGoldDto, InventoryItemDto, OtherToolDto, SkillDto, ToolDto } from "../types/api";
import { AbilityType, Attack, AttackDamage, AttackInfo, AttackSavingThrow, Characteristic, ClassResource, GlobalDamageModifier, Inventory, InventoryGold, InventoryItem, OtherTool, OtherToolType, ProficiencyType, ResourceResetType, SaveThrow, Skill, SkillType, Tool } from "../types/state";

export function mapAbilityDtoToAbilities(dto: AbilityDto): Record<AbilityType, Characteristic>
{
  return {
    strength: { base: dto.strengthBase },
    dexterity: { base: dto.dexterityBase },
    constitution: { base: dto.constitutionBase },
    intelligence: { base: dto.intelligenceBase },
    wisdom: { base: dto.wisdomBase },
    charisma: { base: dto.charismaBase }
  };
}

export function mapAbilitySaveThrowDtoToSaveThrows(dto: AbilitySaveThrowDto): Record<AbilityType, SaveThrow>
{
  return {
    strength: { isProficient: dto.isSaveThrowStrengthProficient },
    dexterity: { isProficient: dto.isSaveThrowDexterityProficient },
    constitution: { isProficient: dto.isSaveThrowConstitutionProficient },
    intelligence: { isProficient: dto.isSaveThrowIntelligenceProficient },
    wisdom: { isProficient: dto.isSaveThrowWisdomProficient },
    charisma: { isProficient: dto.isSaveThrowCharismaProficient }
  };
}

export function mapSkillDtoToSkills(dto: SkillDto): Record<SkillType, Skill>
{
  return {
    acrobatics: { isProficient: dto.isAcrobaticsProficient, ability: dto.acrobaticsBondAbility.toLowerCase() as AbilityType },
    animalHandling: { isProficient: dto.isAnimalHandlingProficient, ability: dto.animalHandlingBondAbility.toLowerCase() as AbilityType },
    arcana: { isProficient: dto.isArcanaProficient, ability: dto.arcanaBondAbility.toLowerCase() as AbilityType },
    athletics: { isProficient: dto.isAthleticsProficient, ability: dto.athleticsBondAbility.toLowerCase() as AbilityType },
    deception: { isProficient: dto.isDeceptionProficient, ability: dto.deceptionBondAbility.toLowerCase() as AbilityType },
    history: { isProficient: dto.isHistoryProficient, ability: dto.historyBondAbility.toLowerCase() as AbilityType },
    insight: { isProficient: dto.isInsightProficient, ability: dto.insightBondAbility.toLowerCase() as AbilityType },
    intimidation: { isProficient: dto.isIntimidationProficient, ability: dto.intimidationBondAbility.toLowerCase() as AbilityType },
    investigation: { isProficient: dto.isInvestigationProficient, ability: dto.investigationBondAbility.toLowerCase() as AbilityType },
    medicine: { isProficient: dto.isMedicineProficient, ability: dto.medicineBondAbility.toLowerCase() as AbilityType },
    nature: { isProficient: dto.isNatureProficient, ability: dto.natureBondAbility.toLowerCase() as AbilityType },
    perception: { isProficient: dto.isPerceptionProficient, ability: dto.perceptionBondAbility.toLowerCase() as AbilityType },
    performance: { isProficient: dto.isPerformanceProficient, ability: dto.performanceBondAbility.toLowerCase() as AbilityType },
    persuasion: { isProficient: dto.isPersuasionProficient, ability: dto.persuasionBondAbility.toLowerCase() as AbilityType },
    religion: { isProficient: dto.isReligionProficient, ability: dto.religionBondAbility.toLowerCase() as AbilityType },
    sleightOfHand: { isProficient: dto.isSleightOfHandProficient, ability: dto.sleightOfHandBondAbility.toLowerCase() as AbilityType },
    stealth: { isProficient: dto.isStealthProficient, ability: dto.stealthBondAbility.toLowerCase() as AbilityType },
    survival: { isProficient: dto.isSurvivalProficient, ability: dto.survivalBondAbility.toLowerCase() as AbilityType }
  };
}

export function mapToolDtoToTools(dto: ToolDto[]): Tool[]
{
  return dto.map(tool => ({
    id: tool.id,
    name: tool.name,
    proficiencyType: tool.proficiencyType.toLowerCase() as ProficiencyType,
    bondAbility: tool.bondAbility.toLowerCase() as AbilityType,
    mods: tool.mods
  }));
}

export function mapOtherToolDtoToOtherTools(dto: OtherToolDto[]): OtherTool[]
{
  return dto.map(tool => ({
    id: tool.id,
    name: tool.name,
    type: tool.type.toLowerCase() as OtherToolType
  }));
}

export function mapAttackDtoToAttacks(dto: AttackDto[]): Attack[]
{
  return dto.map(a => ({
    id: a.id,
    name: a.name,
    atk: {
      isIncluded: a.atkIsIncluded,
      bondAbility: a.atkBondAbility as AbilityType | "spell" | "-",
      bonus: a.atkBonus,
      isProficient: a.atkIsProficient,
      range: a.atkRange,
      magicBonus: a.atkMagicBonus,
      critRange: a.atkCritRange
    } as AttackInfo,
    damage1: {
      isIncluded: a.damage1IsIncluded,
      dice: a.damage1Dice,
      bondAbility: a.damage1BondAbility as AbilityType | "spell" | "-",
      bonus: a.damage1Bonus,
      type: a.damage1Type,
      critDice: a.damage1CritDice
    } as AttackDamage,
    damage2: {
      isIncluded: a.damage2IsIncluded,
      dice: a.damage2Dice,
      bondAbility: a.damage2BondAbility as AbilityType | "spell" | "-",
      bonus: a.damage2Bonus,
      type: a.damage2Type,
      critDice: a.damage2CritDice
    } as AttackDamage,
    savingThrow: {
      isIncluded: a.savingThrowIsIncluded,
      bondAbility: a.savingThrowBondAbility as AbilityType,
      dificultyClass: a.savingThrowDifficultyClass as AbilityType | "spell" | "flat"
    } as AttackSavingThrow,
    saveEffect: a.saveEffect,
    description: a.description
  } as Attack ));
}

export function mapGlobalDamageModifierDtoToGlobalDamageModifier(dto: GlobalDamageModifierDto[]): GlobalDamageModifier[]
{
  return dto.map(gdm => ({
    id: gdm.id,
    name: gdm.name,
    damageDice: gdm.damageDice, // 1d6, 1d4, ...
    criticalDamageDice: gdm.criticalDamageDice, // 1d6, 1d4, ...
    type: gdm.type,
    isIncluded: gdm.isIncluded
  } as GlobalDamageModifier))
}

export function mapInventoryGoldDtoToInventoryGold(dto: InventoryGoldDto): InventoryGold
{
  return {
    cp: dto.cp,
    sp: dto.sp,
    ep: dto.ep,
    gp: dto.gp,
    pp: dto.pp
  } as InventoryGold
}

export function mapInventoryItemDtoToInventoryItem(dto: InventoryItemDto[]): InventoryItem[]
{
  return dto.map(i => ({
    id: i.id,
    amount: i.amount,
    name: i.name,
    weight: i.weight,
    isEquipped: i.isEquipped,
    isUsedAsResource: i.isUsedAsResource,
    isHasAnAttack: i.isHasAnAttack,
    prop: i.prop,
    description: i.description
  } as InventoryItem));
}

export function mapInventoryDtosToInventory(igDto: InventoryGoldDto, iiDto: InventoryItemDto[]): Inventory
{
  return {
    gold: mapInventoryGoldDtoToInventoryGold(igDto),
    items: mapInventoryItemDtoToInventoryItem(iiDto)
  } as Inventory
}

export function mapClassResourceDtoToClassResource(cr: ClassResourceDto): ClassResource
{
  return {
    id: cr.id,
    total: cr.total,
    current: cr.current,
    name: cr.name,
    usePb: cr.isUsePb,
    resetOn: cr.resetOn as ResourceResetType
  } as ClassResource
}

export function mapOtherResourceDtosToClassResources(ors: ClassResourceDto[]): ClassResource[]
{
  return ors.map(cr => mapClassResourceDtoToClassResource(cr));
}
