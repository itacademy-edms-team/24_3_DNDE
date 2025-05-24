import { createSlice, PayloadAction } from '@reduxjs/toolkit';

import { AuthState } from '../../types/state';
import { IContainsTokens } from '../../types/api';

const initialState: AuthState = {
  isAuthenticated: null,
  hasChecked: false,
  redirectUrl: null
};

const authSlice = createSlice({
  name: "auth",
  initialState,
  reducers: {
    loading(state)
    {
      state.isAuthenticated = null;
      state.hasChecked = false;
    },
    login(state)
    {
      state.isAuthenticated = true;
      state.hasChecked = true;
    },
    logout(state)
    {
      state.isAuthenticated = false;
      state.hasChecked = true;
    },
    setRedirectUrl(state, action: { payload: string })
    {
      state.redirectUrl = action.payload;
    },
    clearRedirectUrl(state)
    {
      state.redirectUrl = null;
    }
  }
});

export const { loading, login, logout, setRedirectUrl, clearRedirectUrl } = authSlice.actions;
export default authSlice.reducer;


