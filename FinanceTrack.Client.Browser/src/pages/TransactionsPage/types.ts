export type TransactionType = 'Income' | 'Expense';

export type TransactionBase = {
  id: string;
  name: string;
  amount: number;
  operationDate: string; // ISO date string "YYYY-MM-DD"
  isMonthly: boolean;
  type: TransactionType;
};

export type IncomeTransaction = TransactionBase & {
  type: 'Income';
};

export type ExpenseTransaction = TransactionBase & {
  type: 'Expense';
  incomeTransactionId?: string; // not returned by API but useful on client
};

export type ListTransactionsResponse<T extends TransactionBase> = {
  transactions: T[];
};

export type CreateIncomePayload = {
  name: string;
  amount: number;
  operationDate: string;
  isMonthly: boolean;
};

export type UpdateIncomePayload = CreateIncomePayload;

export type CreateExpensePayload = {
  name: string;
  amount: number;
  operationDate: string;
  isMonthly: boolean;
  incomeTransactionId: string;
};

export type UpdateExpensePayload = CreateExpensePayload & {
  transactionId: string;
};