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
  attacks: Attack[],
  globalDamageModifiers: GlobalDamageModifier[],
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