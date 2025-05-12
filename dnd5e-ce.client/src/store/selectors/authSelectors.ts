import { createSelector } from "@reduxjs/toolkit";
import { RootState } from "../../types/state";

export const selectAccessToken = (state: RootState) => state.auth.accessToken;

