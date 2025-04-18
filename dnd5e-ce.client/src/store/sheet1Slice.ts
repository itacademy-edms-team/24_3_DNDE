import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import {
  Sheet1State
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
  characteristics: {
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
    investigation: { isProficient: true, ability: "intelligence" },
    perception: { isProficient: false, ability: "wisdom" },
    survival: { isProficient: false, ability: "wisdom" },
    performance: { isProficient: false, ability: "charisma" },
    intimidation: { isProficient: false, ability: "charisma" },
    history: { isProficient: false, ability: "intelligence" },
    sleightOfHand: { isProficient: false, ability: "dexterity" },
    arcana: { isProficient: false, ability: "intelligence" },
    medicine: { isProficient: false, ability: "wisdom" },
    deception: { isProficient: false, ability: "charisma" },
    nature: { isProficient: false, ability: "intelligence" },
    insight: { isProficient: false, ability: "wisdom" },
    religion: { isProficient: false, ability: "intelligence" },
    stealth: { isProficient: false, ability: "dexterity" },
    persuasion: { isProficient: false, ability: "charisma" },
    animalHandling: { isProficient: false, ability: "wisdom" }
  },
  tools: [
    {
      id: 0,
      name: "ׂÿז¸כûו לוקט",
      proficiencyType: "proficient", // proficient | expertise | jackOfAllTrades
      bondAbility: "strength",
      mods: 0
    },
    {
      id: 1,
      name: "tool2",
      proficiencyType: "proficient", // proficient | expertise | jackOfAllTrades
      bondAbility: "intelligence",
      mods: 0
    },
  ],
  otherTools: [
    {
      id: 0,
      name: "lang1",
      type: "language", // language, weapon, armor, other
    },
    {
      id: 1,
      name: "weapon1",
      type: "weapon",
    },
    {
      id: 2,
      name: "armor1",
      type: "armor",
    },
    {
      id: 3,
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
    updateName(state: Sheet1State, action: PayloadAction<string>) {
      state.name = action.payload;
    }
  }
});

export const { updateName } = sheet1Slice.actions;
export default sheet1Slice.reducer;
