import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import {
    OtherTool,
  Sheet1State,
  Tool
} from '../types/state';

const initialState: Sheet1State = {
  name: "character name",
  class: "none",
  level: 1,
  race: "character race",
  backstory: "character backstory",
  worldview: "character worldview",
  playerName: "player name",
  experience: 0,
  abilities: {
    strength: {
      base: 15
    },
    dexterity: {
      base: 12
    },
    constitution: {
      base: 12
    },
    intelligence: {
      base: 14
    },
    wisdom: {
      base: 13
    },
    charisma: {
      base: 11
    }
  },
  isInspired: false,
  saveThrows: {
    strength: {
      isProficient: true
    },
    dexterity: {
      isProficient: true
    },
    constitution: {
      isProficient: false
    },
    intelligence: {
      isProficient: false
    },
    wisdom: {
      isProficient: false
    },
    charisma: {
      isProficient: false
    }
  },
  skills: {
    acrobatics: { isProficient: true, ability: "dexterity" },
    animalHandling: { isProficient: false, ability: "wisdom" },
    arcana: { isProficient: false, ability: "intelligence" },
    athletics: { isProficient: true, ability: "strength" },
    deception: { isProficient: false, ability: "charisma" },
    history: { isProficient: false, ability: "intelligence" },
    insight: { isProficient: false, ability: "wisdom" },
    intimidation: { isProficient: false, ability: "charisma" },
    investigation: { isProficient: true, ability: "intelligence" },
    medicine: { isProficient: false, ability: "wisdom" },
    nature: { isProficient: false, ability: "intelligence" },
    perception: { isProficient: false, ability: "wisdom" },
    performance: { isProficient: false, ability: "charisma" },
    persuasion: { isProficient: false, ability: "charisma" },
    religion: { isProficient: false, ability: "intelligence" },
    sleightOfHand: { isProficient: false, ability: "dexterity" },
    stealth: { isProficient: false, ability: "dexterity" },
    survival: { isProficient: false, ability: "wisdom" }
  },
  tools: [
    {
      id: "123",
      name: "Тяжёлые мечи",
      proficiencyType: "proficient", // proficient | expertise | jackOfAllTrades
      bondAbility: "strength",
      mods: 0
    },
    {
      id: "321",
      name: "tool2",
      proficiencyType: "proficient", // proficient | expertise | jackOfAllTrades
      bondAbility: "intelligence",
      mods: 0
    },
  ],
  otherTools: [
    {
      id: "asdasd-123",
      name: "lang1",
      type: "language", // language, weapon, armor, other
    },
    {
      id: "gfasd-123-54dasd",
      name: "weapon1",
      type: "weapon",
    },
    {
      id: "gfasd-123-54das2d",
      name: "armor1",
      type: "armor",
    },
    {
      id: "gfasd-123-54dasd33",
      name: "other1",
      type: "other",
    },
  ],
  attacks: [
    {
      id: 0,
      name: "Attack0",
      atk: {
        isIncluded: false, // enable or disable participant in calculation
        bondAbility: "strength", // strength, dexterity, constitution, intelligence, wisdom, charisma, spell, "-"
        bonus: 0,
        isProficient: true,
        range: "10/30",
        magicBonus: 0,
        critRange: ""
      },
      damage1: {
        isIncluded: false,
        dice: "1d6",
        bondAbility: "strength", // strength, dexterity, constitution, intelligence, wisdom, charisma, spell, "-"
        bonus: 0,
        type: "",
        critDice: "1d6"
      },
      damage2: {
        isIncluded: false,
        dice: "1d6",
        bondAbility: "strength", // strength, dexterity, constitution, intelligence, wisdom, charisma, spell, "-"
        bonus: 0,
        type: "",
        critDice: "1d6"
      },
      savingThrow: {
        isIncluded: false,
        bondAbility: "strength", // strength, dexterity, constitution, intelligence, wisdom, charisma
        dificultyClass: "strength" // strength, dexterity, constitution, intelligence, wisdom, charisma, spell, flat
      },
      saveEffect: "",
      description: ""
    }
  ],
  armorClass: 0,
  initiative: 0,
  speed: 0,
  hp: {
    max: 10,
    current: 10,
    temp: 5
  },
  hitDice: {
    total: 3,
    current: 3,
    type: "d8"
  },
  deathSaveThrow: {
    successes: [false, false, false],
    failures: [false, false, false]
  },
  personalityTraits: "character personality traits",
  ideals: "character ideals",
  bonds: "character bonds",
  flaws: "character flaws",
  classResource: {
    id: 0,
    total: 2,
    current: 2,
    name: "Rage",
    usePb: false,
    resetOn: "longRest"
  },
  otherResources: [
    {
      id: 1,
      total: 2,
      current: 2,
      name: "other",
      usePb: false,
      resetOn: "longRest"
    }
  ]
};

const sheet1Slice = createSlice({
  name: 'sheet1',
  initialState,
  reducers: {
    // Header
    updateName(state: Sheet1State, action: PayloadAction<string>) {
      state.name = action.payload;
    },
    updateClass(state: Sheet1State, action: PayloadAction<string>) {
      state.class = action.payload;
    },
    updateLevel(state: Sheet1State, action: PayloadAction<number>) {
      state.level = action.payload;
    },
    updateRace(state: Sheet1State, action: PayloadAction<string>) {
      state.race = action.payload;
    },
    updateBackstory(state: Sheet1State, action: PayloadAction<string>) {
      state.backstory = action.payload;
    },
    updateWorldview(state: Sheet1State, action: PayloadAction<string>) {
      state.worldview = action.payload;
    },
    updatePlayerName(state: Sheet1State, action: PayloadAction<string>) {
      state.playerName = action.payload;
    },
    updateExperience(state: Sheet1State, action: PayloadAction<number>) {
      state.experience = action.payload;
    },
    // Body
    // SkillCard
    updateStrength(state: Sheet1State, action: PayloadAction<number>) {
      state.abilities.strength.base = action.payload;
    },
    updateDexterity(state: Sheet1State, action: PayloadAction<number>) {
      state.abilities.dexterity.base = action.payload;
    },
    updateConstitution(state: Sheet1State, action: PayloadAction<number>) {
      state.abilities.constitution.base = action.payload;
    },
    updateIntelligence(state: Sheet1State, action: PayloadAction<number>) {
      state.abilities.intelligence.base = action.payload;
    },
    updateWisdom(state: Sheet1State, action: PayloadAction<number>) {
      state.abilities.wisdom.base = action.payload;
    },
    updateCharisma(state: Sheet1State, action: PayloadAction<number>) {
      state.abilities.charisma.base = action.payload;
    },
    // InspirationCard
    updateInspiration(state: Sheet1State, action: PayloadAction<boolean>) {
      state.isInspired = action.payload;
    },
    // SaveThrows
    updateSaveThrowProficiency(
      state: Sheet1State,
      action: PayloadAction<{ ability: keyof Sheet1State['saveThrows']; isProficient: boolean }>
    ) {
      const { ability, isProficient } = action.payload;
      state.saveThrows[ability].isProficient = isProficient;
    },
    // SkillList
    updateSkillProficiency(
      state: Sheet1State,
      action: PayloadAction<{ skill: keyof Sheet1State['skills']; isProficient: boolean }>
    ) {
      const { skill, isProficient } = action.payload;
      state.skills[skill].isProficient = isProficient;
    },
    // ToolProficienciesAndCustomSkillsCard
    addTool(
      state: Sheet1State,
      action: PayloadAction<Tool>
    ) {
      state.tools.push(action.payload);
    },
    updateTool(
      state: Sheet1State,
      action: PayloadAction<Tool>
    ) {
      const index = state.tools.findIndex((tool) => tool.id === action.payload.id);
      if (index !== -1) {
        state.tools[index] = action.payload;
      }
    },
    deleteTool(
      state: Sheet1State,
      action: PayloadAction<string>
    ) {
      state.tools = state.tools.filter((tool) => tool.id !== action.payload);
    },
    addOtherTool(
      state: Sheet1State,
      action: PayloadAction<OtherTool>
    ) {
      state.otherTools.push(action.payload);
    },
    updateOtherTool(
      state: Sheet1State,
      action: PayloadAction<OtherTool>
    ) {
      const index = state.otherTools.findIndex((tool) => tool.id === action.payload.id);
      if (index !== -1) {
        state.otherTools[index] = action.payload;
      }
    },
    deleteOtherTool(
      state: Sheet1State,
      action: PayloadAction<string>
    ) {
      state.otherTools = state.otherTools.filter((tool) => tool.id !== action.payload);
    }
  }
});

export const { updateName, updateClass, updateLevel, updateRace, updateBackstory, updateWorldview, updatePlayerName, updateExperience, updateStrength, updateDexterity, updateConstitution, updateIntelligence, updateWisdom, updateCharisma, updateInspiration, updateSaveThrowProficiency, updateSkillProficiency, addTool, updateTool, deleteTool, addOtherTool, updateOtherTool, deleteOtherTool } = sheet1Slice.actions;
export default sheet1Slice.reducer;
