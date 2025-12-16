import {
  CreateExpensePayload,
  CreateIncomePayload,
  ExpenseTransaction,
  IncomeTransaction,
  ListTransactionsResponse,
  UpdateExpensePayload,
  UpdateIncomePayload,
} from './types';

const BASE = '/api/finance/transactions';

async function requestJson<T>(input: RequestInfo, init?: RequestInit): Promise<T> {
  const hasBody = Boolean(init?.body);
  const headers = {
    ...(hasBody ? { 'Content-Type': 'application/json' } : {}),
    ...(init?.headers ?? {}),
  };

  const response = await fetch(input, {
    credentials: 'include',
    ...init,
    headers,
  });

  if (!response.ok) {
    throw new Error(`Request failed: ${response.status}`);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

export async function fetchIncomes(): Promise<IncomeTransaction[]> {
  const data = await requestJson<ListTransactionsResponse<IncomeTransaction>>(
    `${BASE}/income`,
    { method: 'GET' }
  );
  return data.transactions ?? [];
}

export async function createIncome(payload: CreateIncomePayload): Promise<string> {
  const data = await requestJson<{ id: string }>(`${BASE}/income`, {
    method: 'POST',
    body: JSON.stringify(payload),
  });
  return data.id;
}

export async function updateIncome(id: string, payload: UpdateIncomePayload): Promise<void> {
  await requestJson<void>(`${BASE}/income/${encodeURIComponent(id)}`, {
    method: 'PUT',
    body: JSON.stringify(payload),
  });
}

export async function deleteIncome(id: string): Promise<void> {
  await requestJson<void>(`${BASE}/income/${encodeURIComponent(id)}`, {
    method: 'DELETE',
  });
}

export async function fetchExpensesByIncome(
  incomeId: string
): Promise<ExpenseTransaction[]> {
  const data = await requestJson<ListTransactionsResponse<ExpenseTransaction>>(
    `${BASE}/income/${encodeURIComponent(incomeId)}/expenses`,
    { method: 'GET' }
  );
  return (data.transactions ?? []).map(tx => ({ ...tx, incomeTransactionId: incomeId }));
}

export async function createExpense(payload: CreateExpensePayload): Promise<string> {
  const data = await requestJson<{ id: string }>(`${BASE}/expense`, {
    method: 'POST',
    body: JSON.stringify(payload),
  });
  return data.id;
}

export async function updateExpense(payload: UpdateExpensePayload): Promise<void> {
  await requestJson<void>(`${BASE}/expense/${encodeURIComponent(payload.transactionId)}`, {
    method: 'PUT',
    body: JSON.stringify(payload),
  });
}

export async function deleteExpense(transactionId: string): Promise<void> {
  await requestJson<void>(`${BASE}/expense/${encodeURIComponent(transactionId)}`, {
    method: 'DELETE',
  });
}