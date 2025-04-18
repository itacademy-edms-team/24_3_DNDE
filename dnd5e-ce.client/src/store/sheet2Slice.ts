// store/sheet2Slice.js
import { PayloadAction, createSlice } from '@reduxjs/toolkit';
import { Sheet2State } from '../types/state';

const initialState = {
  age: "20 ���",
  height: "175 �",
  weight: "80 ��",
  eyes: "������",
  skin: "�������",
  hair: "�������",
  appearance: "character appearance",
  backstory: "character backstory",
  alliesAndOrganizations: "character alliesAndOrganizations",
  additionalFeaturesAndTraits: "additionalFeaturesAndTraits",
  treasures: "treasures"
};

const sheet2Slice = createSlice({
  name: 'sheet2',
  initialState,
  reducers: {
    updateAge: (state: Sheet2State, action: PayloadAction<string>) => {
      state.age = action.payload;
    },
    // ... ������ ���������
  }
});

export const { updateAge } = sheet2Slice.actions;
export default sheet2Slice.reducer;