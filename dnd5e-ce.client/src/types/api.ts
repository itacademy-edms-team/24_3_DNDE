import { SpellAbilityType } from "./state";

// Интерфейс для ошибок валидации от сервера export interface IValidationError {   code: string;   description: string; }  export interface IContainsAccessToken {   accessToken: string; }  export interface IContainsRefreshToken {   refreshToken: string; }  export interface IContainsTokens extends IContainsAccessToken, IContainsRefreshToken { }  export type LoginData = {   email: string;   password: string; };  export interface IRegisterFormData {   username: string;   email: string;   password: string;   passwordConfirm: string; }  export interface ILoginFormData {   email: string;   password: string; }  export interface AuthResponse {   success: boolean;   errors: string[]; }  export interface Sheet1Dto
{
  //header fields
  name: string;
  class: string;
  level: number;
  race: string;
  backstory: string,
  worldview: string,
  playerName: string,
  experience: number,
  // body fields  }  export interface Sheet2Dto
{   // header   age: string,
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
  treasures: string }  export interface Sheet3Dto
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
  remainingSpellSlotsLevel9: number; }  export interface CharacterDto
{
  sheet1: Sheet1Dto;
  sheet2: Sheet2Dto;
  sheet3: Sheet3Dto;
}  export interface CharacterListItemDto
{   id: number;
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
