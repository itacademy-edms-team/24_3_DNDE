import { createSelector } from "@reduxjs/toolkit";
import { RootState } from "../../types/state";

export const selectIsUserAuthenticated = (state: RootState) => state.auth.isAuthenticated;

