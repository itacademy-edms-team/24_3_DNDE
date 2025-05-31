import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import {
  Attack,
  GlobalDamageModifier,
  HitDice,
  HitDiceType,
  InventoryGold,
  InventoryItem,
  ItemOtherResourceBond,
  OtherResource,
  OtherTool,
  ResourceResetType,
  Sheet1State,
  Tool
} from '../types/state';
import { AbilitySaveThrowDto, Sheet1Dto } from '../types/api';
import { mapAbilityDtoToAbilities, mapAbilitySaveThrowDtoToSaveThrows, mapAttackDtoToAttacks, mapClassResourceDtoToClassResource, mapGlobalDamageModifierDtoToGlobalDamageModifier, mapInventoryDtosToInventory, mapInventoryItemDtoToInventoryItem, mapOtherResourceDtosToClassResources, mapOtherToolDtoToOtherTools, mapSkillDtoToSkills, mapToolDtoToTools } from '../utils/map';

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
      id: "unga-bunga-3000",
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
  globalDamageModifiers: [
    {
      id: "tung-tung-sakhur-3000",
      name: "Урон от Ярость",
      damageDice: "1d6", // 1d6, 1d4, ...
      criticalDamageDice: "1d6", // 1d6, 1d4, ...
      type: "Ярость",
      isIncluded: false
    }
  ],
  inventory: {
    gold: {
      cp: 0,
      sp: 0,
      ep: 0,
      gp: 0,
      pp: 0
    },
    items: [
      {
        id: "bombardilo-crocodilo-43",
        amount: 1,
        name: "Верёвка",
        weight: 1,
        isEquipped: true,
        isUsedAsResource: false,
        isHasAnAttack: false,
        prop: "",
        description: "Моток верёвки (5м)"
      }
    ]
  },
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
    id: "avada-kedavra-0",
    total: 2,
    current: 2,
    name: "Rage",
    usePb: false,
    resetOn: "longRest"
  },
  otherResources: [
    {
      id: "avada-kedavra-3",
      total: 2,
      current: 2,
      name: "other",
      usePb: false,
      resetOn: "longRest"
    }
  ],
  itemOtherResourceBonds: []
};

const sheet1Slice = createSlice({
  name: 'sheet1',
  initialState,
  reducers: {
    // For fetch
    loadSheet1(state: Sheet1State, action: PayloadAction<Sheet1Dto>)
    {
      const newSheet = action.payload;

      // Header
      state.name = newSheet.name;
      state.class = newSheet.class;
      state.level = newSheet.level;
      state.race = newSheet.race;
      state.backstory = newSheet.backstory;
      state.worldview = newSheet.worldview;
      state.playerName = newSheet.playerName;
      state.experience = newSheet.experience;

      // Body
      state.abilities = mapAbilityDtoToAbilities(newSheet.ability);
      state.saveThrows = mapAbilitySaveThrowDtoToSaveThrows(newSheet.abilitySaveThrow);
      state.skills = mapSkillDtoToSkills(newSheet.skill);
      state.tools = mapToolDtoToTools(newSheet.tool);
      state.otherTools = mapOtherToolDtoToOtherTools(newSheet.otherTool);

      state.armorClass = newSheet.armorClass;
      state.initiative = newSheet.initiative;
      state.speed = newSheet.speed;
      state.attacks = mapAttackDtoToAttacks(newSheet.attack);
      state.globalDamageModifiers = mapGlobalDamageModifierDtoToGlobalDamageModifier(newSheet.globalDamageModifier);
      state.inventory = mapInventoryDtosToInventory(newSheet.inventoryGold, newSheet.inventoryItem);

      state.personalityTraits = newSheet.personalityTraits;
      state.ideals = newSheet.ideals;
      state.bonds = newSheet.bonds;
      state.flaws = newSheet.flaws;
      state.classResource = mapClassResourceDtoToClassResource(newSheet.classResource);
      state.otherResources = mapOtherResourceDtosToClassResources(newSheet.otherResource);
    },
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
    },
    updateArmorClass(
      state: Sheet1State,
      action: PayloadAction<number>
    ) {
      state.armorClass = action.payload;
    },
    updateInitiative(
      state: Sheet1State,
      action: PayloadAction<number>
    ) {
      state.initiative = action.payload;
    },
    updateSpeed(
      state: Sheet1State,
      action: PayloadAction<number>
    ) {
      state.speed = action.payload;
    },
    updateMaxHP(
      state: Sheet1State,
      action: PayloadAction<number>
    ) {
      state.hp.max = action.payload;
    },
    updateCurrentHP(
      state: Sheet1State,
      action: PayloadAction<number>
    ) {
      state.hp.current = action.payload;
    },
    updateTempHP(
      state: Sheet1State,
      action: PayloadAction<number>
    ) {
      state.hp.temp = action.payload;
    },
    updateHitDiceTotal(
      state: Sheet1State,
      action: PayloadAction<number>
    ) {
      state.hitDice.total = action.payload;
    },
    updateHitDiceCurrent(
      state: Sheet1State,
      action: PayloadAction<number>
    ) {
      state.hitDice.current = action.payload;
    },
    updateHitDiceType(
      state: Sheet1State,
      action: PayloadAction<HitDiceType>
    ) {
      state.hitDice.type = action.payload;
    },
    updateDeathSaveThrowsSuccesses(state, action: PayloadAction<[boolean, boolean, boolean]>) {
      state.deathSaveThrow.successes = action.payload;
    },
    updateDeathSaveThrowsFailures(state, action: PayloadAction<[boolean, boolean, boolean]>) {
      state.deathSaveThrow.failures = action.payload;
    },
    resetDeathSaveThrows(state) {
      state.deathSaveThrow.successes = [false, false, false];
      state.deathSaveThrow.failures = [false, false, false];
    },
    addAttack(state, action: PayloadAction<Attack>) {
      state.attacks.push(action.payload);
    },
    updateAttack(state, action: PayloadAction<Partial<Attack> & {id: string}>) {
      const index = state.attacks.findIndex((attack) => attack.id === action.payload.id);
      if (index !== -1) {
        state.attacks[index] = { ...state.attacks[index], ...action.payload };
      }
    },
    deleteAttack(state, action: PayloadAction<string>) {
      state.attacks = state.attacks.filter((attack) => attack.id !== action.payload);
    },
    addGlobalDamageModifier(state, action: PayloadAction<GlobalDamageModifier>) {
      state.globalDamageModifiers.push(action.payload);
    },
    updateGlobalDamageModifier(state, action: PayloadAction<Partial<GlobalDamageModifier> & { id: string }>) {
      const index = state.globalDamageModifiers.findIndex((gam) => gam.id === action.payload.id);
      if (index !== -1) {
        state.globalDamageModifiers[index] = { ...state.globalDamageModifiers[index], ...action.payload };
      }
    },
    deleteGlobalDamageModifier(state, action: PayloadAction<string>) {
      state.globalDamageModifiers = state.globalDamageModifiers.filter((gam) => gam.id !== action.payload);
    },
    updateInventoryGold(state, action: PayloadAction<Partial<InventoryGold>>) {
      state.inventory.gold = { ...state.inventory.gold, ...action.payload };
    },
    addInventoryItem(state, action: PayloadAction<InventoryItem>) {
      state.inventory.items.push(action.payload);
    },
    deleteInventoryItem(state, action: PayloadAction<string>) {
      state.inventory.items = state.inventory.items.filter((item) => item.id !== action.payload);
    },
    updateInventoryItem(state, action: PayloadAction<Partial<InventoryItem> & { id: string }>) {
      const index = state.inventory.items.findIndex((item) => item.id === action.payload.id);
      if (index !== -1) {
        state.inventory.items[index] = { ...state.inventory.items[index], ...action.payload };
      }
    },
    updateInventoryItemIsUsedAsResource(state, action: PayloadAction<{ id: string, newValue: boolean }>) {
      const index = state.inventory.items.findIndex((item) => item.id === action.payload.id);
      if (index !== -1) {
        // change state
        state.inventory.items[index].isUsedAsResource = action.payload.newValue;
      }
    },
    addInventoryItemOtherResourceBond(state, action: PayloadAction<ItemOtherResourceBond>) {
      state.itemOtherResourceBonds.push(action.payload);
    },
    deleteInventoryItemOtherResourceBond(state, action: PayloadAction<{ itemId: string }>) {
      state.itemOtherResourceBonds = state.itemOtherResourceBonds.filter((bond) => bond.itemId !== action.payload.itemId);
    },
    updateCharacterPersonalityTraits(state, action: PayloadAction<string>) {
      state.personalityTraits = action.payload;
    },
    updateCharacterIdeals(state, action: PayloadAction<string>) {
      state.ideals = action.payload;
    },
    updateCharacterBonds(state, action: PayloadAction<string>) {
      state.bonds = action.payload;
    },
    updateCharacterFlaws(state, action: PayloadAction<string>) {
      state.flaws = action.payload;
    },
    updateCharacterClassResourceTotal(state, action: PayloadAction<number>) {
      state.classResource.total = action.payload;
    },
    updateCharacterClassResourceCurrent(state, action: PayloadAction<number>) {
      state.classResource.current = action.payload;
    },
    updateCharacterClassResourceName(state, action: PayloadAction<string>) {
      state.classResource.name = action.payload;
    },
    updateCharacterClassResourceUsePb(state, action: PayloadAction<boolean>) {
      state.classResource.usePb = action.payload;
    },
    updateCharacterClassResourceResetOn(state, action: PayloadAction<ResourceResetType>) {
      state.classResource.resetOn = action.payload;
    },
    addCharacterOtherResource(state, action: PayloadAction<OtherResource>) {
      state.otherResources.push(action.payload);
    },
    deleteCharacterOtherResource(state, action: PayloadAction<string>) {
      state.otherResources = state.otherResources.filter((resource) => resource.id !== action.payload);
    },
    updateCharacterOtherResourceTotal(state, action: PayloadAction<{ id: string; total: number }>) {
      const index = state.otherResources.findIndex((resource) => resource.id === action.payload.id);
      if (index !== -1) {
        state.otherResources[index].total = action.payload.total;
      }
    },
    updateCharacterOtherResourceCurrent(state, action: PayloadAction<{ id: string; current: number }>) {
      const index = state.otherResources.findIndex((resource) => resource.id === action.payload.id);
      if (index !== -1) {
        state.otherResources[index].current = action.payload.current;
      }
    },
    updateCharacterOtherResourceName(state, action: PayloadAction<{ id: string; name: string }>) {
      const index = state.otherResources.findIndex((resource) => resource.id === action.payload.id);
      if (index !== -1) {
        state.otherResources[index].name = action.payload.name;
      }
    },
    updateCharacterOtherResourceUsePb(state, action: PayloadAction<{ id: string; usePb: boolean }>) {
      const index = state.otherResources.findIndex((resource) => resource.id === action.payload.id);
      if (index !== -1) {
        state.otherResources[index].usePb = action.payload.usePb;
      }
    },
    updateCharacterOtherResourceResetOn(state, action: PayloadAction<{ id: string; resetOn: ResourceResetType }>) {
      const index = state.otherResources.findIndex((resource) => resource.id === action.payload.id);
      if (index !== -1) {
        state.otherResources[index].resetOn = action.payload.resetOn;
      }
    },
  }
});

export const { loadSheet1, updateName, updateClass, updateLevel, updateRace, updateBackstory,
  updateWorldview, updatePlayerName, updateExperience, updateStrength, updateDexterity,
  updateConstitution, updateIntelligence, updateWisdom, updateCharisma, updateInspiration,
  updateSaveThrowProficiency, updateSkillProficiency, addTool, updateTool, deleteTool, addOtherTool,
  updateOtherTool, deleteOtherTool, updateArmorClass, updateInitiative, updateSpeed,
  updateMaxHP, updateCurrentHP, updateTempHP, updateHitDiceTotal, updateHitDiceCurrent, updateHitDiceType,
  updateDeathSaveThrowsSuccesses, updateDeathSaveThrowsFailures, resetDeathSaveThrows,
  addAttack, updateAttack, deleteAttack, addGlobalDamageModifier, updateGlobalDamageModifier, deleteGlobalDamageModifier,
  updateInventoryGold, addInventoryItem, updateInventoryItem, deleteInventoryItem,
  addInventoryItemOtherResourceBond, deleteInventoryItemOtherResourceBond,
  updateCharacterPersonalityTraits, updateCharacterIdeals, updateCharacterBonds, updateCharacterFlaws,
  updateCharacterClassResourceTotal, updateCharacterClassResourceCurrent, updateCharacterClassResourceName,
  updateCharacterClassResourceUsePb, updateCharacterClassResourceResetOn,
  addCharacterOtherResource, deleteCharacterOtherResource,
  updateCharacterOtherResourceCurrent, updateCharacterOtherResourceTotal, updateCharacterOtherResourceName,
  updateCharacterOtherResourceResetOn, updateCharacterOtherResourceUsePb
} = sheet1Slice.actions;
export default sheet1Slice.reducer;