import { ResourceResetType, SpellAbilityType } from "./state";

// Интерфейс для ошибок валидации от сервера
export interface IValidationError {
  code: string;
  description: string;
}

export interface IContainsAccessToken {
  accessToken: string;
}

export interface IContainsRefreshToken {
  refreshToken: string;
}

export interface IContainsTokens extends IContainsAccessToken, IContainsRefreshToken { }

export type LoginData = {
  email: string;
  password: string;
};

export interface IRegisterFormData {
  username: string;
  email: string;
  password: string;
  passwordConfirm: string;
}

export interface ILoginFormData {
  email: string;
  password: string;
}

export interface AuthResponse {
  success: boolean;
  errors: string[];
}

export interface Sheet1Dto {
  name: string;
  class: string;
  level: number;
  race: string;
  backstory: string;
  worldview: string;
  playerName: string;
  experience: number;
  ability: AbilityDto;
  abilitySaveThrow: AbilitySaveThrowDto;
  skill: SkillDto;
  tool: ToolDto[];
  otherTool: OtherToolDto[];
  armorClass: number;
  initiative: number;
  speed: number;
  hitPoint: HitPointDto;
  hitDice: HitDiceDto;
  deathSaveThrow: DeathSaveThrowDto;
  attack: AttackDto[];
  globalDamageModifier: GlobalDamageModifierDto[];
  inventoryGold: InventoryGoldDto;
  inventoryItem: InventoryItemDto[];
  personalityTraits: string;
  ideals: string;
  bonds: string;
  flaws: string;
  classResource: ClassResourceDto;
  otherResource: OtherResourceDto[];
}

export interface AbilityDto {
  strengthBase: number;
  dexterityBase: number;
  constitutionBase: number;
  intelligenceBase: number;
  wisdomBase: number;
  charismaBase: number;
}

export interface AbilitySaveThrowDto {
  isSaveThrowStrengthProficient: boolean;
  isSaveThrowDexterityProficient: boolean;
  isSaveThrowConstitutionProficient: boolean;
  isSaveThrowIntelligenceProficient: boolean;
  isSaveThrowWisdomProficient: boolean;
  isSaveThrowCharismaProficient: boolean;
}

export interface SkillDto {
  isAcrobaticsProficient: boolean;
  acrobaticsBondAbility: string; // e.g., "Dexterity"
  isAnimalHandlingProficient: boolean;
  animalHandlingBondAbility: string; // e.g., "Wisdom"
  isArcanaProficient: boolean;
  arcanaBondAbility: string; // e.g., "Intelligence"
  isAthleticsProficient: boolean;
  athleticsBondAbility: string; // e.g., "Strength"
  isDeceptionProficient: boolean;
  deceptionBondAbility: string; // e.g., "Charisma"
  isHistoryProficient: boolean;
  historyBondAbility: string; // e.g., "Intelligence"
  isInsightProficient: boolean;
  insightBondAbility: string; // e.g., "Wisdom"
  isIntimidationProficient: boolean;
  intimidationBondAbility: string; // e.g., "Charisma"
  isInvestigationProficient: boolean;
  investigationBondAbility: string; // e.g., "Intelligence"
  isMedicineProficient: boolean;
  medicineBondAbility: string; // e.g., "Wisdom"
  isNatureProficient: boolean;
  natureBondAbility: string; // e.g., "Intelligence"
  isPerceptionProficient: boolean;
  perceptionBondAbility: string; // e.g., "Wisdom"
  isPerformanceProficient: boolean;
  performanceBondAbility: string; // e.g., "Charisma"
  isPersuasionProficient: boolean;
  persuasionBondAbility: string; // e.g., "Charisma"
  isReligionProficient: boolean;
  religionBondAbility: string; // e.g., "Intelligence"
  isSleightOfHandProficient: boolean;
  sleightOfHandBondAbility: string; // e.g., "Dexterity"
  isStealthProficient: boolean;
  stealthBondAbility: string; // e.g., "Dexterity"
  isSurvivalProficient: boolean;
  survivalBondAbility: string; // e.g., "Wisdom"
}

export interface ToolDto
{
  id: string;
  name: string;
  proficiencyType: "Proficient" | "Expertise" | "JackOfAllTrades";
  bondAbility: string; // e.g., "Wisdom", "Intelligence"
  mods: number;
}

export interface OtherToolDto
{
  id: string;
  name: string;
  type: string; // e.g., "Musical Instrument", "Adventuring Gear"
}

export interface HitPointDto {
  max: number;
  current: number;
  temp: number;
}

export interface HitDiceDto {
  total: number;
  current: number;
  diceType: "D4" | "D6" | "D8" | "D10" | "D12";
}

export interface DeathSaveThrowDto {
  successTotal: number;
  failuresTotal: number;
}

export interface AttackDto {
  id: string;
  name: string;
  atkIsIncluded: boolean;
  atkBondAbility: string; // Possible values: "strength", "dexterity", "constitution", "intelligence", "wisdom", "charisma", "spell", "-"
  atkBonus: number;
  atkIsProficient: boolean;
  atkRange: string;
  atkMagicBonus: number;
  atkCritRange: string;
  damage1IsIncluded: boolean;
  damage1Dice: string;
  damage1BondAbility: string; // Possible values: "strength", "dexterity", "constitution", "intelligence", "wisdom", "charisma", "spell", "-"
  damage1Bonus: number;
  damage1Type: string;
  damage1CritDice: string;
  damage2IsIncluded: boolean;
  damage2Dice: string;
  damage2BondAbility: string; // Possible values: "strength", "dexterity", "constitution", "intelligence", "wisdom", "charisma", "spell", "-"
  damage2Bonus: number;
  damage2Type: string;
  damage2CritDice: string;
  savingThrowIsIncluded: boolean;
  savingThrowBondAbility: string; // Possible values: "strength", "dexterity", "constitution", "intelligence", "wisdom", "charisma"
  savingThrowDifficultyClass: string; // Possible values: "strength", "dexterity", "constitution", "intelligence", "wisdom", "charisma", "spell", "flat"
  saveEffect: string;
  description: string;
}

export interface GlobalDamageModifierDto {
  id: string;
  name: string;
  damageDice: string; // Examples: "1d6", "1d4", "2d8", ...
  criticalDamageDice: string; // Examples: "1d6", "1d4", "2d8", ...
  type: string; // Examples: "fire", "slashing", "piercing", ...
  isIncluded: boolean;
}

export interface InventoryGoldDto {
  id: string;
  cp: number; // Copper pieces
  sp: number; // Silver pieces
  ep: number; // Electrum pieces
  gp: number; // Gold pieces
  pp: number; // Platinum pieces
}

export interface InventoryItemDto {
  id: string;
  amount: number;
  name: string;
  weight: number;
  isEquipped: boolean;
  isUsedAsResource: boolean;
  isHasAnAttack: boolean;
  prop: string; // Examples: "weapon", "armor", "consumable", "tool", ...
  description: string;
}

export interface ClassResourceDto
{
  id: string;
  total: number;
  current: number;
  name: string;
  isUsePb: boolean;
  resetOn: string;
}

export interface OtherResourceDto
{
  id: string;
  total: number;
  current: number;
  name: string;
  isUsePb: boolean;
  resetOn: string;
}

export interface Sheet2Dto
{
  // header
  age: string,
  height: string,
  weight: string,
  eyes: string,
  skin: string,
  hair: string,
  // body
  appearance: string,
  backstory: string,
  alliesAndOrganizations: string,
  additionalFeaturesAndTraits: string,
  treasures: string
}

export interface Sheet3Dto
{
  // header
  spellBondAbility: SpellAbilityType;
  // body
  remainingSpellSlotsLevel1: number;
  remainingSpellSlotsLevel2: number;
  remainingSpellSlotsLevel3: number;
  remainingSpellSlotsLevel4: number;
  remainingSpellSlotsLevel5: number;
  remainingSpellSlotsLevel6: number;
  remainingSpellSlotsLevel7: number;
  remainingSpellSlotsLevel8: number;
  remainingSpellSlotsLevel9: number;
}

export interface CharacterDto
{
  id: string;
  sheet1: Sheet1Dto;
  sheet2: Sheet2Dto;
  sheet3: Sheet3Dto;
}

export interface CharacterListItemDto
{
  id: string;
  name: string;
  level: number;
  class: string;
}

export interface CharacterCreateDto
{
  sheet1: Sheet1CreateDto;
  sheet2?: Sheet2CreateDto;
  sheet3?: Sheet3CreateDto;
}

export interface Sheet1CreateDto
{
  name: string;
  level: number;
  class: string;
}

export interface Sheet2CreateDto
{
  age?: string;
  height?: string;
  weight?: string;
  eyes?: string;
  skin?: string;
  hair?: string;
  appearance?: string;
  backstory?: string;
  alliesAndOrganizations?: string;
  additionalFeaturesAndTraits?: string;
  treasures?: string;
}

export interface Sheet3CreateDto
{
  spellBondAbility?: string;
  remainingSpellSlotsLevel1: number;
  remainingSpellSlotsLevel2: number;
  remainingSpellSlotsLevel3: number;
  remainingSpellSlotsLevel4: number;
  remainingSpellSlotsLevel5: number;
  remainingSpellSlotsLevel6: number;
  remainingSpellSlotsLevel7: number;
  remainingSpellSlotsLevel8: number;
  remainingSpellSlotsLevel9: number;
}
