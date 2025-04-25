
import { PayloadAction, createSlice } from '@reduxjs/toolkit';
import { Sheet3State, SpellAbilityType } from '../types/state';

const initialState: Sheet3State = {
  spellBondAbility: "none",
  spells: {
    cantrips: [],
    level1: [],
    level2: [],
    level3: [],
    level4: [],
    level5: [],
    level6: [],
    level7: [],
    level8: [],
    level9: []
  }
}

const sheet3Slice = createSlice({
  name: 'sheet3',
  initialState,
  reducers: {
    updateSpellCastingAbility(state, action: PayloadAction<SpellAbilityType>) {
      state.spellBondAbility = action.payload;
    },
  }
});

export const {
  updateSpellCastingAbility
} = sheet3Slice.actions;
export default sheet3Slice.reducer;
