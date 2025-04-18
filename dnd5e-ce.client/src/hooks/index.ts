// dnd5e-ce.client/src/hooks/index.ts
import { TypedUseSelectorHook, useDispatch, useSelector } from 'react-redux';
import { store } from '../store';
import { RootState } from '../types/state';

// ��� ��� useDispatch
export type AppDispatch = typeof store.dispatch;

// �������������� ����
export const useAppDispatch = () => useDispatch<AppDispatch>();
export const useAppSelector: TypedUseSelectorHook<RootState> = useSelector;