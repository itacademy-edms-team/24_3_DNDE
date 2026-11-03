/**
 * Тело ошибки в формате RFC 7807 ProblemDetails.
 * Расширения (например, target) попадают в произвольные ключи.
 */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  /** Ошибки валидации: ключ - имя поля, значение - список сообщений. */
  errors?: Record<string, string[]>;
  [extension: string]: unknown;
}

/** Ошибка вызова API: HTTP-статус плюс разобранный ProblemDetails, если он был в ответе. */
export class ApiError extends Error {
  readonly status: number;
  readonly problem: ProblemDetails | null;

  constructor(status: number, problem: ProblemDetails | null) {
    super(problem?.title ?? `HTTP ${status}`);
    this.name = "ApiError";
    this.status = status;
    this.problem = problem;
  }

  /** 401 - сессии нет или она истекла: пользователю нужно войти заново. */
  get isUnauthenticated(): boolean {
    return this.status === 401;
  }
}

/** Читает ProblemDetails из ответа, если тело похоже на JSON; иначе возвращает null. */
export async function readProblemDetails(response: Response): Promise<ProblemDetails | null> {
  const contentType = response.headers.get("Content-Type") ?? "";
  const isJson = contentType.includes("problem+json") || contentType.includes("application/json");
  if (!isJson) return null;

  try {
    return (await response.json()) as ProblemDetails;
  } catch {
    return null;
  }
}
