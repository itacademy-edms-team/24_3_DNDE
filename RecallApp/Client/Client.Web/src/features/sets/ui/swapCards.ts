import type { EditorCard } from "./ElementCard";

/**
 * Меняет местами две карточки по id (swap). Симметрично в обе стороны.
 * Не мутирует вход; при no-op (одинаковые id или карточки нет) возвращает исходную ссылку.
 */
export function swapCards(cards: EditorCard[], aId: string, bId: string): EditorCard[] {
  if (aId === bId) return cards;

  const a = cards.findIndex((c) => c.id === aId);
  const b = cards.findIndex((c) => c.id === bId);
  if (a < 0 || b < 0) return cards;

  const next = [...cards];
  [next[a], next[b]] = [next[b], next[a]];
  return next;
}
