import { ChangeEvent } from "react";

// dnd5e-ce.client/src/types/state.ts
export type ProficiencyType = "proficient" | "expertise" | "jackOfAllTrades";
export type AbilityType = "strength" | "dexterity" | "constitution" | "intelligence" | "wisdom" | "charisma";
export type SkillType =
  | "acrobatics"
  | "animalHandling"
  | "arcana"
  | "athletics"
  | "deception"
  | "history"
  | "insight"
  | "intimidation"
  | "investigation"
  | "medicine"
  | "nature"
  | "perception"
  | "performance"
  | "persuasion"
  | "religion"
  | "sleightOfHand"
  | "stealth"
  | "survival";
export type OtherToolType = "language" | "weapon" | "armor" | "other";
export type HitDiceType = "d4" | "d6" | "d8" | "d10" | "d12";
export type ResourceResetType = "longRest" | "shortRest" | "-";
export type AttackAbilityType = AbilityType | "spell" | "-";
export type DamageAbilityType = AbilityType | "spell" | "-";
export type DCAbilityType = AbilityType | "spell" | "flat";
export type SpellSchool =
  | 'abjuration'
  | 'conjuration'
  | 'divination'
  | 'enchantment'
  | 'evocation'
  | 'illusion'
  | 'necromancy'
  | 'transmutation';

export interface Characteristic {
  base: number;
}

export interface SaveThrow {
  isProficient: boolean;
}

export interface Skill {
  isProficient: boolean;
  ability: AbilityType;
}

export interface Tool {
  id: string,
  name: string,
  proficiencyType: ProficiencyType,
  bondAbility: AbilityType,
  mods: number
}

export interface OtherTool {
  id: string,
  name: string,
  type: OtherToolType
}

export interface AttackInfo {
  isIncluded: boolean;
  bondAbility: AbilityType | "spell" | "-";
  bonus: number;
  isProficient: boolean;
  range: string;
  magicBonus: number;
  critRange: string;
}

export interface AttackDamage {
  isIncluded: boolean;
  dice: string;
  bondAbility: AbilityType | "spell" | "-";
  bonus: number;
  type: string;
  critDice: string;
}

export interface AttackSavingThrow {
  isIncluded: boolean;
  bondAbility: AbilityType;
  dificultyClass: AbilityType | "spell" | "flat";
}

export interface Attack {
  id: string,
  name: string,
  atk: AttackInfo,
  damage1: AttackDamage,
  damage2: AttackDamage,
  savingThrow: AttackSavingThrow;
  saveEffect: string;
  description: string;
}

export interface GlobalDamageModifier {
  id: string,
  name: string,
  damageDice: string, // 1d6, 1d4, ...
  criticalDamageDice: string, // 1d6, 1d4, ...
  type: string,
  isIncluded: boolean
}

export interface HP {
  max: number,
  current: number,
  temp: number
}

export interface HitDice {
  total: number,
  current: number,
  type: HitDiceType
}
export interface DeathSaveThrow {
  successes: [boolean, boolean, boolean];
  failures: [boolean, boolean, boolean];
}

export interface ClassResource {
  id: number,
  total: number,
  current: number,
  name: string,
  usePb: boolean,
  resetOn: ResourceResetType
}


export interface InventoryGold {
  cp: number,
  sp: number,
  ep: number,
  gp: number,
  pp: number
}

export interface InventoryItem {
  id: string,
  amount: number,
  name: string,
  weight: number,
  isEquipped: boolean,
  isUsedAsResource: boolean,
  isHasAnAttack: boolean,
  prop: string,
  description: string
}

export interface Inventory {
  gold: InventoryGold,
  items: InventoryItem[]
}

export interface Sheet1HeaderState {
  name: string;
  class: string;
  level: number;
  race: string;
  backstory: string;
  worldview: string;
  playerName: string;
  experience: number;
}

export interface AbilityCardCardDataType {
  bonusId: string,
  baseId: string,
  label: string,
}

export interface AbilityCardCardDataMapType extends Record<AbilityType, AbilityCardCardDataType> { }

export interface AbilityCardPropsType {
  skillState: number
  skillHandler: (e: ChangeEvent<HTMLInputElement>) => void,
  cardData: AbilityCardCardDataType
}

export interface AbilityCardCardDataType {
  bonusId: string,
  baseId: string,
  label: string,
}

export interface SaveThrowCardCardDataType {
  label: string
}

export interface SaveThrowsPropsType {
  abilityName: AbilityType,
  cardData: SaveThrowCardCardDataType
}

export interface SkillCardCardData {
  label: string
}

export interface SkillListPropsType {
  skillName: SkillType
  cardData: SkillCardCardData
}

export interface ToolProficienciesAndCustomSkillsCardEditableRowPropsType {
  tool: Tool,
  isEditMode: boolean
  onDelete: any,
}

export interface OtherProficienciesAndCustomSkillsCardEditableRowPropsType {
  tool: OtherTool,
  isEditMode: boolean,
  onDelete: any,
}

export interface AttacksCardAttackEditableRowPropsType {
  attack: Attack,
  isEditMode: boolean,
  onDelete: any
}

export interface AttacksCardGlobalDamageModifierEditableRowPropsType {
  globalDamageModifier: GlobalDamageModifier,
  isEditMode: boolean,
  onDelete: any
}

export interface InventoryItemEditableRowPropsType {
  inventoryItem: InventoryItem,
  isEditMode: boolean,
  onDelete: any
}

export type SpellLevel = 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9;

export interface SpellComponents {
  v: {
    isIncluded: boolean
  },
  s: {
    isIncluded: boolean
  },
  m: {
    isIncluded: boolean
  }
}

export type SpellOutputType = "spellcard" | "attack";
export type SpellAttackType = "ranged" | "mele";
export type SpellDescriptionInAttackIncludeVariety = "off" | "partial" | "on";

export interface SpellDamage {
  dice: string,
  type: string
}

export interface SpellAttack {
  attackType: SpellAttackType,
  damage1: SpellDamage,
  damage2: SpellDamage
}

export type SpellAbilityType = "none" | AbilityType;

export interface SpellSavingThrow {
  ability: SpellAbilityType,
  effect: string
}

export type SpellHighLevelCastDiceType = "none" | HitDiceType | "d20";

export interface SpellHigherLevelCast {
  diceAmount: number,
  diceType: SpellHighLevelCastDiceType,
  bonus: number
}

export interface Spell {
  id: string,
  name: string,
  school: SpellSchool,
  isRitual: boolean,
  castingTime: string,
  range: string,
  target: string,
  components: SpellComponents,
  componentsDescription: string,
  isConcentration: boolean,
  duration: string,
  spellCastingAbility: SpellAbilityType,
  innate: string,
  output: SpellOutputType,
  attack: SpellAttack,
  healingDice: string,
  isAbilityModIncluded: boolean, // Add ability mod to attack or no
  savingThrow: SpellSavingThrow,
  higherLevelCast: SpellHigherLevelCast,
  includeSpellDescriptionInAttack: SpellDescriptionInAttackIncludeVariety,
  description: string,
  atHigherLevels: string,
  class: string,
  type: string,
  cantripProgression: CantripProgressionType,
  isPrepared: boolean
}

// cantripBeam - cantripBeam depends on level and adds additional beams to attack
// cantripDice - adds additional damageDices to attack depending on level
export type CantripProgressionType = "none" | "cantripBeam" | "cantripDice";

export interface SpellItemToggleType {
  eventKey: {
    info: string,
    edit: string
  };
  isEditMode: boolean;
  cantripName: string;
  onDelete?: () => void;
}

export interface SpellCardPropsType {
  spell: Spell,
  isEditMode: boolean,
  onDelete: any,
  spellLevel: SpellLevel
}

export type CharacterClass =
  | 'wizard'
  | 'cleric'
  | 'druid'
  | 'sorcerer'
  | 'bard'
  | 'paladin'
  | 'ranger'
  | 'eldritch knight'
  | 'arcane trickster'
  | 'warlock';

export interface ClassInfo {
  class: CharacterClass | string;
  level: number;
}

export interface SpellSlots {
  [key: number]: number; // e.g., { 1: 4, 2: 3, 3: 2 } for 4 slots of level 1, 3 slots of level 2, etc.
}

export interface PactMagic {
  slots: number; // Number of pact magic slots
  level: number; // Level of pact magic slots
}


export interface Sheet1State {
  //header fields
  name: string;
  class: string;
  level: number;
  race: string;
  backstory: string,
  worldview: string,
  playerName: string,
  experience: number,
  // body fields
  isInspired: boolean,
  abilities: Record<AbilityType, Characteristic>,
  saveThrows: Record<AbilityType, SaveThrow>,
  skills: Record<SkillType, Skill>,
  tools: Tool[],
  otherTools: OtherTool[],
  armorClass: number,
  initiative: number,
  speed: number,
  hp: HP,
  attacks: Attack[],
  hitDice: HitDice,
  deathSaveThrow: DeathSaveThrow,
  globalDamageModifiers: GlobalDamageModifier[],
  inventory: Inventory,
  personalityTraits: string,
  ideals: string,
  bonds: string,
  flaws: string,
  classResource: ClassResource,
  otherResources: ClassResource[],
}

export interface Sheet2State {
  age: string,
  height: string,
  weight: string,
  eyes: string,
  skin: string,
  hair: string,
  appearance: string,
  backstory: string,
  alliesAndOrganizations: string,
  additionalFeaturesAndTraits: string,
  treasures: string
}

export type SpellCategoryKey = 'cantrips' | 'level0' | 'level1' | 'level2' | 'level3' | 'level4' | 'level5' | 'level6' | 'level7' | 'level8' | 'level9';
export type RemainingSpellSlots = 'level1' | 'level2' | 'level3' | 'level4' | 'level5' | 'level6' | 'level7' | 'level8' | 'level9';

export interface Sheet3State {
  spellBondAbility: SpellAbilityType,
  spells: {
    cantrips: Spell[],
    level1: Spell[],
    level2: Spell[],
    level3: Spell[],
    level4: Spell[],
    level5: Spell[],
    level6: Spell[],
    level7: Spell[],
    level8: Spell[],
    level9: Spell[],
  },
  remainingSpellSlots: {
    level1: number,
    level2: number,
    level3: number,
    level4: number,
    level5: number,
    level6: number,
    level7: number,
    level8: number,
    level9: number,
  }
}

export interface RootState {
  sheet1: Sheet1State;
  sheet2: Sheet2State;
  sheet3: Sheet3State;
}