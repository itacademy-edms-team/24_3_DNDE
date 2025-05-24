// store/sheet2Slice.js
import { PayloadAction, createSlice } from '@reduxjs/toolkit';
import { Sheet2State } from '../types/state';
import { Sheet2Dto } from '../types/api';

const initialState = {
  age: "20 лет",
  height: "175 м",
  weight: "80 кг",
  eyes: "Зелёные",
  skin: "Светлая",
  hair: "Блондин",
  appearance: "Внешний вид",
  backstory: "Предыстория",
  alliesAndOrganizations: "Союзники и организации",
  additionalFeaturesAndTraits: "Дополнительные особенности и умения",
  treasures: "Сокровища"
};

const sheet2Slice = createSlice({
  name: 'sheet2',
  initialState,
  reducers: {
    loadSheet2(state, action: PayloadAction<Sheet2Dto>)
    {
      const newSheet = action.payload;

      // header
      state.age = newSheet.age;
      state.height = newSheet.height;
      state.weight = newSheet.weight;
      state.eyes = newSheet.eyes;
      state.skin = newSheet.skin;
      state.hair = newSheet.hair;
      // body
      state.appearance = newSheet.appearance;
      state.backstory = newSheet.backstory;
      state.alliesAndOrganizations = newSheet.alliesAndOrganizations;
      state.additionalFeaturesAndTraits = newSheet.additionalFeaturesAndTraits;
      state.treasures = newSheet.treasures;
    },
    // Header
    updateAge(state, action: PayloadAction<string>) {
      state.age = action.payload;
    },
    updateHeight(state, action: PayloadAction<string>) {
      state.height = action.payload;
    },
    updateWeight(state, action: PayloadAction<string>) {
      state.weight = action.payload;
    },
    updateEyes(state, action: PayloadAction<string>) {
      state.eyes = action.payload;
    },
    updateSkin(state, action: PayloadAction<string>) {
      state.skin = action.payload;
    },
    updateHair(state, action: PayloadAction<string>) {
      state.hair = action.payload;
    },
    // Body
    updateAppearance(state, action: PayloadAction<string>) {
      state.appearance = action.payload;
    },
    updateBackstory(state, action: PayloadAction<string>) {
      state.backstory = action.payload;
    },
    updateAlliesAndOrganizations(state, action: PayloadAction<string>) {
      state.alliesAndOrganizations = action.payload;
    },
    updateAdditionalFeaturesAndTraits(state, action: PayloadAction<string>) {
      state.additionalFeaturesAndTraits = action.payload;
    },
    updateTreasures(state, action: PayloadAction<string>) {
      state.treasures = action.payload;
    },
  }
});

export const {
  loadSheet2,
  updateAge, updateHeight, updateWeight, updateEyes, updateSkin, updateHair,
  updateAppearance, updateBackstory, updateAlliesAndOrganizations, updateAdditionalFeaturesAndTraits, updateTreasures
} = sheet2Slice.actions;
export default sheet2Slice.reducer;