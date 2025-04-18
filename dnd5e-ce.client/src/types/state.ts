// dnd5e-ce.client/src/types/state.ts
export type ProficiencyType = "proficient" | "expertise" | "jackOfAllTrades";
export type AbilityType = "strength" | "dexterity" | "constitution" | "intelligence" | "wisdom" | "charisma";
export type SkillType =
  | "acrobatics"
  | "investigation"
  | "perception"
  | "survival"
  | "performance"
  | "intimidation"
  | "history"
  | "sleightOfHand"
  | "arcana"
  | "medicine"
  | "deception"
  | "nature"
  | "insight"
  | "religion"
  | "stealth"
  | "persuasion"
  | "animalHandling";
export type OtherToolType = "language" | "weapon" | "armor" | "other";
export type HitDiceType = "d4" | "d6" | "d8" | "d10" | "d12";
export type ResourceResetType = "longRest" | "shortRest" | "-";

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
  id: number,
  name: string,
  proficiencyType: ProficiencyType,
  bondAbility: AbilityType,
  mods: number
}

export interface OtherTool {
  id: number,
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
  id: number,
  name: string,
  atk: AttackInfo,
  damage1: AttackDamage,
  damage2: AttackDamage,
  savingThrow: AttackSavingThrow;
  saveEffect: string;
  description: string;
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
interface DeathSaveThrow {
  successes: [boolean, boolean, boolean];
  failures: [boolean, boolean, boolean];
}

interface ClassResource {
  id: number,
  total: number,
  current: number,
  name: string,
  usePb: boolean,
  resetOn: ResourceResetType
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
  characteristics: Record<AbilityType, Characteristic>,
  saveThrows: Record<AbilityType, SaveThrow>,
  skills: Record<SkillType, Skill>,
  tools: Tool[],
  otherTools: OtherTool[],
  attacks: Attack[],
  armorClass: number,
  initiative: number,
  speed: number,
  hp: HP,
  hitDice: HitDice,
  deathSaveThrow: DeathSaveThrow,
  personalityTraits: string,
  ideals: string,
  bonds: string,
  flaws: string,
  classResource: ClassResource,
  otherResources: ClassResource[]
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

export interface RootState {
  sheet1: Sheet1State;
  sheet2: Sheet2State;
}