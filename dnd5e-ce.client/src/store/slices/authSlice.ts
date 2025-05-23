import { createSlice, PayloadAction } from '@reduxjs/toolkit';

import { AuthState } from '../../types/state';
import { IContainsTokens } from '../../types/api';

const initialState: AuthState = {
  isAuthenticated: false
};

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    loading: (state) =>
    {
      state.isAuthenticated = null;
    },
    login: (state) => 
    {
      state.isAuthenticated = true;
    },
    logout: (state) => 
    {
      state.isAuthenticated = false;
    },
  },
});

export const { loading, login, logout } = authSlice.actions;
export default authSlice.reducer;


