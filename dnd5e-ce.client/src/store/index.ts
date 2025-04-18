import { configureStore } from '@reduxjs/toolkit';

import sheet1Reducer from './sheet1Slice';
import { Sheet1State } from '../types/state';
import sheet2Reducer from './sheet2Slice';
import { Sheet2State } from '../types/state';

import { RootState } from '../types/state';



export const store = configureStore({
  reducer: {
    sheet1: sheet1Reducer,
    sheet2: sheet2Reducer
  }
});