const relative = new Intl.RelativeTimeFormat("ru", { numeric: "auto" });

/**
 * «Изменено N назад» для карточки набора. `now` передаётся для тестов.
 * Границы: до минуты  «только что», далее минуты/часы/дни/месяцы/годы.
 */
export function formatUpdatedAt(iso: string, now: number = Date.now()): string {
  const seconds = Math.round((new Date(iso).getTime() - now) / 1000);
  const abs = Math.abs(seconds);

  if (abs < 60) return "только что";
  if (abs < 3600) return relative.format(Math.round(seconds / 60), "minute");
  if (abs < 86400) return relative.format(Math.round(seconds / 3600), "hour");
  if (abs < 30 * 86400)
    return relative.format(Math.round(seconds / 86400), "day");
  if (abs < 365 * 86400)
    return relative.format(Math.round(seconds / (30 * 86400)), "month");
  return relative.format(Math.round(seconds / (365 * 86400)), "year");
}
