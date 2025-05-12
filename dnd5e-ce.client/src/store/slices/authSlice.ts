import { createSlice, PayloadAction } from '@reduxjs/toolkit';

import { AuthState } from '../../types/state';
import { IContainsTokens } from '../../types/api';

const initialState: AuthState = {
  accessToken: localStorage.getItem('accessToken') || null,
  refreshToken: localStorage.getItem('refreshToken') || null
};

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    login: (state, action: PayloadAction<IContainsTokens>) => {
      state.accessToken = action.payload.accessToken;
      localStorage.setItem('accessToken', action.payload.accessToken);
      state.refreshToken = action.payload.refreshToken;
      localStorage.setItem('refreshToken', action.payload.refreshToken);
    },
    logout: (state) => {
      state.accessToken = null;
      localStorage.removeItem('accessToken');
      state.refreshToken = null;
      localStorage.removeItem('refreshToken');
    },
  },
});

export const { login, logout } = authSlice.actions;
export default authSlice.reducer;


