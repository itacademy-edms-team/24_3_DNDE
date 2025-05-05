import { ProficiencyType } from "../types/state";

// Множитель для типа владения
export const calcPbMultiplier = (pType: ProficiencyType) => {
  if (pType === 'proficient') return 1;
  if (pType === 'expertise') return 2;
  if (pType === 'jackOfAllTrades') return 0.5;
  return 0;
};