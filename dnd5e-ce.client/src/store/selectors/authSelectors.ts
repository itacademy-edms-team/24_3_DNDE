import { createSelector } from "@reduxjs/toolkit";
import { RootState } from "../../types/state";

export const selectIsUserAuthenticated = (state: RootState) => state.auth.isAuthenticated;
export const selectHasChecked = (state: RootState) => state.auth.hasChecked;
export const selectRedirectUrl = (state: RootState) => state.auth.redirectUrl;

