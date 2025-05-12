import { configureStore } from '@reduxjs/toolkit';

import sheet1Reducer from './sheet1Slice';
import sheet2Reducer from './sheet2Slice';
import sheet3Reducer from './sheet3Slice';
import authReducer from './slices/authSlice';



export const store = configureStore({
  reducer: {
    auth: authReducer,
    sheet1: sheet1Reducer,
    sheet2: sheet2Reducer,
    sheet3: sheet3Reducer
  }
});
