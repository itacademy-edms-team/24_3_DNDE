import { useState, useEffect, useMemo } from 'react';
import { useNavigate, useParams } from 'react-router';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  FormControlLabel,
  IconButton,
  InputAdornment,
  InputLabel,
  LinearProgress,
  MenuItem,
  Paper,
  Select,
  Stack,
  Switch,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import AccountBalanceWalletIcon from '@mui/icons-material/AccountBalanceWallet';
import SavingsIcon from '@mui/icons-material/Savings';
import ArchiveIcon from '@mui/icons-material/Archive';
import DeleteIcon from '@mui/icons-material/Delete';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import TrendingDownIcon from '@mui/icons-material/TrendingDown';
import SwapHorizIcon from '@mui/icons-material/SwapHoriz';

import Loading from '@/components/Loading';

type Wallet = {
  id: string;
  name: string;
  walletType: 'Checking' | 'Savings';
  balance: number;
  allowNegativeBalance: boolean;
  targetAmount: number | null;
  targetDate: string | null;
  isArchived: boolean;
};

type UpdateWalletPayload = {
  name: string;
  allowNegativeBalance?: boolean;
  targetAmount?: number;
  targetDate?: string;
};

const getErrorMessage = (operation: string, status: number): string => {
  return `Ошибка ${operation}: ${status}`;
};

const MONTH_OPTIONS = [
  { value: 1, label: 'Январь' },
  { value: 2, label: 'Февраль' },
  { value: 3, label: 'Март' },
  { value: 4, label: 'Апрель' },
  { value: 5, label: 'Май' },
  { value: 6, label: 'Июнь' },
  { value: 7, label: 'Июль' },
  { value: 8, label: 'Август' },
  { value: 9, label: 'Сентябрь' },
  { value: 10, label: 'Октябрь' },
  { value: 11, label: 'Ноябрь' },
  { value: 12, label: 'Декабрь' },
];

const fetchWallet = async (walletId: string): Promise<Wallet> => {
  const res = await fetch(`/api/finance/Wallets/${walletId}`, {
    credentials: 'include',
  });
  if (!res.ok) {
    throw new Error(getErrorMessage('загрузки кошелька', res.status));
  }
  return await res.json();
};

const updateWallet = async (walletId: string, payload: UpdateWalletPayload): Promise<Wallet> => {
  const res = await fetch(`/api/finance/Wallets/${walletId}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
    },
    credentials: 'include',
    body: JSON.stringify(payload),
  });

  if (!res.ok) {
    throw new Error(getErrorMessage('обновления кошелька', res.status));
  }

  return await res.json();
};

const archiveWallet = async (walletId: string): Promise<void> => {
  const res = await fetch(`/api/finance/Wallets/${walletId}/archive`, {
    credentials: 'include',
    method: 'POST',
    headers: {
        'Content-Type': 'application/json',
    },
    body: '{}', // FastEndpoints default behaviour requirement
  });

  if (!res.ok) {
    throw new Error(getErrorMessage('архивации кошелька', res.status));
  }
};

const formatMoney = (value: number): string => {
  return new Intl.NumberFormat('ru-RU', {
    style: 'currency',
    currency: 'RUB',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value);
};

const formatDate = (dateStr: string | null): string => {
  if (!dateStr) return '';
  const date = new Date(dateStr);
  return date.toLocaleDateString('ru-RU', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });
};

const formatDateShort = (dateStr: string): string => {
  const date = new Date(dateStr);
  return date.toLocaleDateString('ru-RU', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
  });
};

const fetchTransactions = async (
  walletId: string,
  from?: string,
  to?: string
): Promise<Transaction[]> => {
  const params = new URLSearchParams();
  if (from) params.append('from', from);
  if (to) params.append('to', to);
  
  const url = `/api/finance/Wallets/${walletId}/Transactions${params.toString() ? `?${params.toString()}` : ''}`;
  const res = await fetch(url, {
    credentials: 'include',
  });
  if (!res.ok) {
    throw new Error(getErrorMessage('загрузки транзакций', res.status));
  }
  const data: TransactionsResponse = await res.json();
  return data.transactions;
};

const fetchCategories = async (type: 'Income' | 'Expense'): Promise<Category[]> => {
  const res = await fetch('/api/finance/Categories', {
    credentials: 'include',
  });
  if (!res.ok) {
    throw new Error(getErrorMessage('загрузки категорий', res.status));
  }
  const data: CategoriesResponse = await res.json();
  return data.categories.filter((cat) => cat.type === type);
};

const fetchAllWallets = async (): Promise<Wallet[]> => {
  const res = await fetch('/api/finance/Wallets', {
    credentials: 'include',
  });
  if (!res.ok) {
    throw new Error(getErrorMessage('загрузки кошельков', res.status));
  }
  const data: WalletsResponse = await res.json();
  return data.wallets;
};

const createTransfer = async (payload: CreateTransferPayload): Promise<{ id: string }> => {
  const res = await fetch('/api/finance/Transactions/Transfer', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    credentials: 'include',
    body: JSON.stringify(payload),
  });

  if (!res.ok) {
    throw new Error(getErrorMessage('создания перевода', res.status));
  }

  return await res.json();
};

const createIncome = async (payload: CreateIncomePayload): Promise<{ id: string }> => {
  const res = await fetch('/api/finance/Transactions/Income', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    credentials: 'include',
    body: JSON.stringify(payload),
  });

  if (!res.ok) {
    throw new Error(getErrorMessage('создания дохода', res.status));
  }

  return await res.json();
};

const createExpense = async (payload: CreateExpensePayload): Promise<{ id: string }> => {
  const res = await fetch('/api/finance/Transactions/Expense', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    credentials: 'include',
    body: JSON.stringify(payload),
  });

  if (!res.ok) {
    throw new Error(getErrorMessage('создания расхода', res.status));
  }

  return await res.json();
};

const deleteTransaction = async (transactionId: string): Promise<void> => {
  const res = await fetch(`/api/finance/Transactions/${transactionId}`, {
    method: 'DELETE',
    credentials: 'include',
  });

  if (!res.ok) {
    throw new Error(getErrorMessage('удаления транзакции', res.status));
  }
};

type UpdateTransactionPayload = {
  name: string;
  amount: number;
  operationDate: string;
  categoryId?: string | null;
};

const updateTransaction = async (transactionId: string, payload: UpdateTransactionPayload): Promise<void> => {
  const res = await fetch(`/api/finance/Transactions/${transactionId}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
    },
    credentials: 'include',
    body: JSON.stringify(payload),
  });

  if (!res.ok) {
    throw new Error(getErrorMessage('обновления транзакции', res.status));
  }
};

type WalletFormState = {
  name: string;
  allowNegativeBalance: boolean;
  targetAmount: string;
  targetDate: string;
};

type Transaction = {
  id: string;
  walletId: string;
  name: string;
  amount: number;
  operationDate: string;
  type: 'Income' | 'Expense' | 'TransferIn' | 'TransferOut';
  categoryId: string | null;
  relatedTransactionId: string | null;
  recurringTransactionId: string | null;
  relatedWalletId: string | null;
  relatedWalletName: string | null;
};

type TransactionsResponse = {
  transactions: Transaction[];
};

type CreateIncomePayload = {
  walletId: string;
  name: string;
  amount: number;
  operationDate: string;
  categoryId?: string | null;
};

type CreateExpensePayload = {
  walletId: string;
  name: string;
  amount: number;
  operationDate: string;
  categoryId?: string | null;
};

type CreateTransferPayload = {
  fromWalletId: string;
  toWalletId: string;
  name: string;
  amount: number;
  operationDate: string;
};

type Category = {
  id: string;
  name: string;
  type: 'Income' | 'Expense';
  icon: string | null;
  color: string | null;
};

type CategoriesResponse = {
  categories: Category[];
};

type WalletsResponse = {
  wallets: Wallet[];
};

type TransactionFormState = {
  name: string;
  amount: string;
  operationDate: string;
  categoryId: string;
};

type TransferFormState = {
  fromWalletId: string;
  toWalletId: string;
  name: string;
  amount: string;
  operationDate: string;
};

function WalletPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { walletId } = useParams<{ walletId: string }>();
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [archiveConfirmOpen, setArchiveConfirmOpen] = useState(false);
  const [transactionDialogOpen, setTransactionDialogOpen] = useState(false);
  const [transferDialogOpen, setTransferDialogOpen] = useState(false);
  const [transactionType, setTransactionType] = useState<'Income' | 'Expense'>('Income');
  const [transactionEditMode, setTransactionEditMode] = useState<'create' | 'edit'>('create');
  const [transactionToEdit, setTransactionToEdit] = useState<Transaction | null>(null);
  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);
  const [transactionToDelete, setTransactionToDelete] = useState<string | null>(null);
  const [errorDialog, setErrorDialog] = useState({ open: false, message: '' });
  const [transferForm, setTransferForm] = useState<TransferFormState>({
    fromWalletId: walletId || '',
    toWalletId: '',
    name: '',
    amount: '',
    operationDate: new Date().toISOString().split('T')[0],
  });
  const [formState, setFormState] = useState<WalletFormState>({
    name: '',
    allowNegativeBalance: true,
    targetAmount: '',
    targetDate: '',
  });

  // Фильтры по датам
  const now = new Date();
  const [filterStartYear, setFilterStartYear] = useState<number>(now.getFullYear());
  const [filterStartMonth, setFilterStartMonth] = useState<number>(now.getMonth() + 1);
  const [filterEndYear, setFilterEndYear] = useState<number>(now.getFullYear());
  const [filterEndMonth, setFilterEndMonth] = useState<number>(now.getMonth() + 1);
  const [filtersInitialized, setFiltersInitialized] = useState(false);
  const [transactionForm, setTransactionForm] = useState<TransactionFormState>({
    name: '',
    amount: '',
    operationDate: new Date().toISOString().split('T')[0],
    categoryId: '',
  });

  const { data: wallet, isLoading, isPending, error } = useQuery({
    queryKey: ['wallet', walletId],
    queryFn: () => fetchWallet(walletId!),
    enabled: !!walletId,
    retry: false,
  });

  // Загружаем все транзакции для определения диапазона дат
  const { data: allTransactions = [] } = useQuery({
    queryKey: ['transactions', walletId, 'all'],
    queryFn: () => fetchTransactions(walletId!),
    enabled: !!walletId && !wallet?.isArchived,
    retry: false,
  });

  // Определяем самую позднюю и самую раннюю даты транзакций
  const latestTransactionDate = useMemo(() => {
    if (allTransactions.length === 0) return null;
    const dates = allTransactions.map((t) => new Date(t.operationDate));
    const latest = new Date(Math.max(...dates.map((d) => d.getTime())));
    return latest;
  }, [allTransactions]);

  const earliestTransactionDate = useMemo(() => {
    if (allTransactions.length === 0) return null;
    const dates = allTransactions.map((t) => new Date(t.operationDate));
    const earliest = new Date(Math.min(...dates.map((d) => d.getTime())));
    return earliest;
  }, [allTransactions]);

  // Определяем диапазон годов для фильтров
  const availableYears = useMemo(() => {
    const currentYear = now.getFullYear();
    let minYear = currentYear - 10;
    let maxYear = currentYear;

    if (earliestTransactionDate) {
      const earliestYear = earliestTransactionDate.getFullYear();
      minYear = Math.min(minYear, earliestYear);
    }
    if (latestTransactionDate) {
      const latestYear = latestTransactionDate.getFullYear();
      maxYear = Math.max(maxYear, latestYear);
    }

    // Добавляем небольшой запас
    minYear = Math.max(2000, minYear - 1); // Не раньше 2000 года
    maxYear = Math.min(2100, maxYear + 1); // Не позже 2100 года

    const years: number[] = [];
    for (let year = maxYear; year >= minYear; year--) {
      years.push(year);
    }
    return years;
  }, [earliestTransactionDate, latestTransactionDate, now]);

  // Сбрасываем фильтры при смене кошелька
  useEffect(() => {
    const currentDate = new Date();
    setFiltersInitialized(false);
    setFilterStartYear(currentDate.getFullYear());
    setFilterStartMonth(currentDate.getMonth() + 1);
    setFilterEndYear(currentDate.getFullYear());
    setFilterEndMonth(currentDate.getMonth() + 1);
  }, [walletId]);

  // Устанавливаем фильтры на основе самой поздней транзакции
  useEffect(() => {
    if (!filtersInitialized && walletId) {
      if (latestTransactionDate) {
        // Если есть транзакции, устанавливаем фильтры на основе самой поздней
        const year = latestTransactionDate.getFullYear();
        const month = latestTransactionDate.getMonth() + 1;
        setFilterEndYear(year);
        setFilterEndMonth(month);
        setFilterStartYear(year);
        setFilterStartMonth(month);
      }
      // Если транзакций нет, фильтры остаются на текущем месяце (уже установлены по умолчанию)
      setFiltersInitialized(true);
    }
  }, [latestTransactionDate, filtersInitialized, walletId]);

  // Формируем даты для фильтрации
  const filterFrom = `${filterStartYear}-${String(filterStartMonth).padStart(2, '0')}-01`;
  const filterTo = `${filterEndYear}-${String(filterEndMonth).padStart(2, '0')}-${new Date(filterEndYear, filterEndMonth, 0).getDate()}`;

  const { data: transactions = [] } = useQuery({
    queryKey: ['transactions', walletId, filterFrom, filterTo],
    queryFn: () => fetchTransactions(walletId!, filterFrom, filterTo),
    enabled: !!walletId && !wallet?.isArchived && filtersInitialized,
    retry: false,
  });

  const { data: incomeCategories = [] } = useQuery({
    queryKey: ['categories', 'Income'],
    queryFn: () => fetchCategories('Income'),
    enabled: transactionDialogOpen && transactionType === 'Income',
    retry: false,
  });

  const { data: expenseCategories = [] } = useQuery({
    queryKey: ['categories', 'Expense'],
    queryFn: () => fetchCategories('Expense'),
    enabled: transactionDialogOpen && transactionType === 'Expense',
    retry: false,
  });

  const { data: allWallets = [] } = useQuery({
    queryKey: ['wallets'],
    queryFn: fetchAllWallets,
    enabled: transferDialogOpen,
    retry: false,
  });

  const updateWalletMutation = useMutation({
    mutationFn: (payload: UpdateWalletPayload) => updateWallet(walletId!, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['wallet', walletId] });
      queryClient.invalidateQueries({ queryKey: ['wallets'] });
      setEditDialogOpen(false);
    },
    onError: (error: Error) => {
      console.error('Failed to update wallet:', error);
      setErrorDialog({ open: true, message: `Ошибка обновления кошелька: ${error.message}` });
    },
  });

  const archiveWalletMutation = useMutation({
    mutationFn: () => archiveWallet(walletId!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['wallet', walletId] });
      queryClient.invalidateQueries({ queryKey: ['wallets'] });
      setArchiveConfirmOpen(false);
      setEditDialogOpen(false);
    },
    onError: (error: Error) => {
      console.error('Failed to archive wallet:', error);
      setErrorDialog({ open: true, message: `Ошибка архивации кошелька: ${error.message}` });
    },
  });

  const createTransactionMutation = useMutation({
    mutationFn: (payload: CreateIncomePayload | CreateExpensePayload) =>
      transactionType === 'Income' ? createIncome(payload as CreateIncomePayload) : createExpense(payload as CreateExpensePayload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transactions', walletId] });
      queryClient.invalidateQueries({ queryKey: ['wallet', walletId] });
      queryClient.invalidateQueries({ queryKey: ['wallets'] });
      // Сбрасываем инициализацию фильтров, чтобы они обновились с учетом новой транзакции
      setFiltersInitialized(false);
      setTransactionDialogOpen(false);
      setTransactionForm({
        name: '',
        amount: '',
        operationDate: new Date().toISOString().split('T')[0],
        categoryId: '',
      });
      setTransactionEditMode('create');
      setTransactionToEdit(null);
    },
    onError: (error: Error) => {
      console.error('Failed to create transaction:', error);
      setErrorDialog({ open: true, message: error.message || 'Ошибка создания транзакции' });
    },
  });

  const updateTransactionMutation = useMutation({
    mutationFn: ({ transactionId, payload }: { transactionId: string; payload: UpdateTransactionPayload }) =>
      updateTransaction(transactionId, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transactions', walletId] });
      queryClient.invalidateQueries({ queryKey: ['wallet', walletId] });
      queryClient.invalidateQueries({ queryKey: ['wallets'] });
      setTransactionDialogOpen(false);
      setTransactionForm({
        name: '',
        amount: '',
        operationDate: new Date().toISOString().split('T')[0],
        categoryId: '',
      });
      setTransactionEditMode('create');
      setTransactionToEdit(null);
    },
    onError: (error: Error) => {
      console.error('Failed to update transaction:', error);
      setErrorDialog({ open: true, message: error.message || 'Ошибка обновления транзакции' });
    },
  });

  const deleteTransactionMutation = useMutation({
    mutationFn: deleteTransaction,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transactions', walletId] });
      queryClient.invalidateQueries({ queryKey: ['wallet', walletId] });
      queryClient.invalidateQueries({ queryKey: ['wallets'] });
      setDeleteConfirmOpen(false);
      setTransactionToDelete(null);
    },
    onError: (error: Error) => {
      console.error('Failed to delete transaction:', error);
      setErrorDialog({ open: true, message: error.message || 'Ошибка удаления транзакции' });
    },
  });

  const createTransferMutation = useMutation({
    mutationFn: createTransfer,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transactions', walletId] });
      queryClient.invalidateQueries({ queryKey: ['wallet', walletId] });
      queryClient.invalidateQueries({ queryKey: ['wallets'] });
      setTransferDialogOpen(false);
      setTransferForm({
        fromWalletId: walletId || '',
        toWalletId: '',
        name: '',
        amount: '',
        operationDate: new Date().toISOString().split('T')[0],
      });
    },
    onError: (error: Error) => {
      console.error('Failed to create transfer:', error);
      setErrorDialog({ open: true, message: error.message || 'Ошибка создания перевода' });
    },
  });

  const handleOpenEditDialog = () => {
    if (wallet) {
      setFormState({
        name: wallet.name,
        allowNegativeBalance: wallet.allowNegativeBalance,
        targetAmount: wallet.targetAmount?.toString() || '',
        targetDate: wallet.targetDate || '',
      });
      setEditDialogOpen(true);
    }
  };

  const handleCloseEditDialog = () => {
    setEditDialogOpen(false);
  };

  const handleSave = () => {
    if (!wallet) return;

    if (!formState.name.trim()) {
      setErrorDialog({ open: true, message: 'Введите название кошелька' });
      return;
    }

    const payload: UpdateWalletPayload = {
      name: formState.name.trim(),
    };

    if (wallet.walletType === 'Checking') {
      payload.allowNegativeBalance = formState.allowNegativeBalance;
    } else {
      const targetAmount = parseFloat(formState.targetAmount);
      if (!formState.targetAmount || isNaN(targetAmount) || targetAmount <= 0) {
        setErrorDialog({ open: true, message: 'Для накопительного кошелька необходимо указать целевую сумму больше 0' });
        return;
      }
      payload.targetAmount = targetAmount;
      if (formState.targetDate) {
        payload.targetDate = formState.targetDate;
      }
    }

    updateWalletMutation.mutate(payload);
  };

  const handleArchiveClick = () => {
    setArchiveConfirmOpen(true);
  };

  const handleArchiveConfirm = () => {
    archiveWalletMutation.mutate();
  };

  const handleBack = () => {
    navigate('/wallets');
  };

  const handleOpenTransactionDialog = (type: 'Income' | 'Expense') => {
    setTransactionType(type);
    setTransactionEditMode('create');
    setTransactionToEdit(null);
    setTransactionForm({
      name: '',
      amount: '',
      operationDate: new Date().toISOString().split('T')[0],
      categoryId: '',
    });
    setTransactionDialogOpen(true);
  };

  const handleEditTransaction = (transaction: Transaction) => {
    if (transaction.type === 'TransferIn' || transaction.type === 'TransferOut') {
      setErrorDialog({ open: true, message: 'Переводы нельзя редактировать' });
      return;
    }
    setTransactionType(transaction.type);
    setTransactionEditMode('edit');
    setTransactionToEdit(transaction);
    setTransactionForm({
      name: transaction.name,
      amount: transaction.amount.toString(),
      operationDate: transaction.operationDate,
      categoryId: transaction.categoryId || '',
    });
    setTransactionDialogOpen(true);
  };

  const handleCloseTransactionDialog = () => {
    setTransactionDialogOpen(false);
    setTransactionEditMode('create');
    setTransactionToEdit(null);
    setTransactionForm({
      name: '',
      amount: '',
      operationDate: new Date().toISOString().split('T')[0],
      categoryId: '',
    });
  };

  const handleSaveTransaction = () => {
    if (!wallet) return;

    if (!transactionForm.name.trim()) {
      setErrorDialog({ open: true, message: 'Введите название транзакции' });
      return;
    }

    const amount = parseFloat(transactionForm.amount);
    if (!transactionForm.amount || isNaN(amount) || amount <= 0) {
      setErrorDialog({ open: true, message: 'Введите корректную сумму (больше 0)' });
      return;
    }

    if (!transactionForm.operationDate) {
      setErrorDialog({ open: true, message: 'Выберите дату операции' });
      return;
    }

    if (transactionEditMode === 'edit' && transactionToEdit) {
      const payload: UpdateTransactionPayload = {
        name: transactionForm.name.trim(),
        amount,
        operationDate: transactionForm.operationDate,
        categoryId: transactionForm.categoryId || null,
      };
      updateTransactionMutation.mutate({ transactionId: transactionToEdit.id, payload });
    } else {
      const payload: CreateIncomePayload | CreateExpensePayload = {
        walletId: wallet.id,
        name: transactionForm.name.trim(),
        amount,
        operationDate: transactionForm.operationDate,
        categoryId: transactionForm.categoryId || null,
      };
      createTransactionMutation.mutate(payload);
    }
  };

  const handleDeleteClick = (transactionId: string) => {
    setTransactionToDelete(transactionId);
    setDeleteConfirmOpen(true);
  };

  const handleDeleteConfirm = () => {
    if (transactionToDelete) {
      deleteTransactionMutation.mutate(transactionToDelete);
    }
  };

  const handleOpenTransferDialog = () => {
    setTransferForm({
      fromWalletId: walletId || '',
      toWalletId: '',
      name: '',
      amount: '',
      operationDate: new Date().toISOString().split('T')[0],
    });
    setTransferDialogOpen(true);
  };

  const handleCloseTransferDialog = () => {
    setTransferDialogOpen(false);
    setTransferForm({
      fromWalletId: walletId || '',
      toWalletId: '',
      name: '',
      amount: '',
      operationDate: new Date().toISOString().split('T')[0],
    });
  };

  const handleSaveTransfer = () => {
    if (!transferForm.name.trim()) {
      setErrorDialog({ open: true, message: 'Введите название перевода' });
      return;
    }

    const amount = parseFloat(transferForm.amount);
    if (!transferForm.amount || isNaN(amount) || amount <= 0) {
      setErrorDialog({ open: true, message: 'Введите корректную сумму (больше 0)' });
      return;
    }

    if (!transferForm.operationDate) {
      setErrorDialog({ open: true, message: 'Выберите дату операции' });
      return;
    }

    if (!transferForm.fromWalletId || !transferForm.toWalletId) {
      setErrorDialog({ open: true, message: 'Выберите кошельки для перевода' });
      return;
    }

    if (transferForm.fromWalletId === transferForm.toWalletId) {
      setErrorDialog({ open: true, message: 'Нельзя перевести деньги на тот же кошелёк' });
      return;
    }

    const payload: CreateTransferPayload = {
      fromWalletId: transferForm.fromWalletId,
      toWalletId: transferForm.toWalletId,
      name: transferForm.name.trim(),
      amount,
      operationDate: transferForm.operationDate,
    };

    createTransferMutation.mutate(payload);
  };

  // Сортируем все транзакции по дате (новые сверху)
  const sortedTransactions = [...transactions].sort(
    (a, b) => new Date(b.operationDate).getTime() - new Date(a.operationDate).getTime()
  );

  const availableWallets = allWallets.filter((w) => !w.isArchived);
  const fromWallets = availableWallets.filter((w) => w.id !== transferForm.toWalletId);
  const toWallets = availableWallets.filter((w) => w.id !== transferForm.fromWalletId);

  if (isLoading || isPending) {
    return <Loading />;
  }

  if (error || !wallet) {
    return (
      <Box sx={{ p: 3 }}>
        <Typography color="error">
          Ошибка загрузки кошелька. Попробуйте обновить страницу.
        </Typography>
        <Button onClick={handleBack} sx={{ mt: 2 }}>
          Вернуться к списку кошельков
        </Button>
      </Box>
    );
  }

  const isSavings = wallet.walletType === 'Savings';
  const progressPercent =
    wallet.targetAmount && wallet.targetAmount > 0
      ? Math.min((wallet.balance / wallet.targetAmount) * 100, 100)
      : 0;

  return (
    <Box sx={{ p: 3 }}>
      <meta name="title" content={wallet.name} />
      <Box sx={{ display: 'flex', alignItems: 'center', mb: 3, gap: 2 }}>
        <Button startIcon={<ArrowBackIcon />} onClick={handleBack}>
          Назад
        </Button>
        <Typography variant="h4" sx={{ flexGrow: 1 }}>
          {wallet.name}
        </Typography>
        {!wallet.isArchived && (
          <Button
            variant="contained"
            startIcon={<EditIcon />}
            onClick={handleOpenEditDialog}
          >
            Редактировать
          </Button>
        )}
        {wallet.isArchived && (
          <Button
            variant="outlined"
            startIcon={<ArchiveIcon />}
            onClick={handleArchiveClick}
          >
            Разархивировать
          </Button>
        )}
      </Box>

      <Card variant="outlined" sx={{ mb: 3 }}>
        <CardContent>
          <Stack spacing={3}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
              {isSavings ? (
                <SavingsIcon sx={{ fontSize: 40, color: 'primary.main' }} />
              ) : (
                <AccountBalanceWalletIcon sx={{ fontSize: 40, color: 'primary.main' }} />
              )}
              <Box>
                <Typography variant="h6">{wallet.name}</Typography>
                <Chip
                  label={isSavings ? 'Накопительный' : 'Расчётный'}
                  size="small"
                  color={isSavings ? 'primary' : 'default'}
                />
                {wallet.isArchived && (
                  <Chip
                    icon={<ArchiveIcon />}
                    label="Архив"
                    size="small"
                    sx={{ ml: 1 }}
                  />
                )}
              </Box>
            </Box>

            <Box>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                Баланс
              </Typography>
              <Typography variant="h3" color={wallet.balance >= 0 ? 'success.main' : 'error.main'}>
                {formatMoney(wallet.balance)}
              </Typography>
            </Box>

            {isSavings && wallet.targetAmount && (
              <Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                  <Typography variant="body2" color="text.secondary">
                    Целевая сумма: {formatMoney(wallet.targetAmount)}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Прогресс: {progressPercent.toFixed(1)}%
                  </Typography>
                </Box>
                <LinearProgress
                  variant="determinate"
                  value={progressPercent}
                  sx={{ height: 10, borderRadius: 1 }}
                />
                {wallet.targetDate && (
                  <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                    Целевая дата: {formatDate(wallet.targetDate)}
                  </Typography>
                )}
              </Box>
            )}

            <Box>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                Настройки
              </Typography>
              <Chip
                label={
                  wallet.allowNegativeBalance
                    ? 'Отрицательный баланс разрешён'
                    : 'Отрицательный баланс запрещён'
                }
                size="small"
                variant="outlined"
              />
            </Box>
          </Stack>
        </CardContent>
      </Card>

      {/* Секция транзакций */}
      {!wallet.isArchived && (
        <Card variant="outlined" sx={{ mb: 3 }}>
          <CardContent>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
              <Typography variant="h6">Транзакции</Typography>
              <Box sx={{ display: 'flex', gap: 1 }}>
                <Button
                  variant="outlined"
                  color="success"
                  startIcon={<TrendingUpIcon />}
                  onClick={() => handleOpenTransactionDialog('Income')}
                  size="small"
                >
                  Доход
                </Button>
                <Button
                  variant="outlined"
                  color="error"
                  startIcon={<TrendingDownIcon />}
                  onClick={() => handleOpenTransactionDialog('Expense')}
                  size="small"
                >
                  Расход
                </Button>
                <Button
                  variant="outlined"
                  color="primary"
                  startIcon={<SwapHorizIcon />}
                  onClick={handleOpenTransferDialog}
                  size="small"
                >
                  Перевод
                </Button>
              </Box>
            </Box>

            {/* Фильтры по датам */}
            <Box sx={{ mb: 3, p: 2, bgcolor: 'background.default', borderRadius: 1 }}>
              <Typography variant="subtitle2" sx={{ mb: 2 }}>
                Фильтр по датам
              </Typography>
              <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap', alignItems: 'center' }}>
                <Typography variant="body2" color="text.secondary">
                  От:
                </Typography>
                <FormControl size="small" sx={{ minWidth: 100 }}>
                  <InputLabel>Год</InputLabel>
                  <Select
                    value={filterStartYear}
                    label="Год"
                    onChange={(e) => setFilterStartYear(Number(e.target.value))}
                  >
                    {availableYears.map((year) => (
                      <MenuItem key={year} value={year}>
                        {year}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
                <FormControl size="small" sx={{ minWidth: 140 }}>
                  <InputLabel>Месяц</InputLabel>
                  <Select
                    value={filterStartMonth}
                    label="Месяц"
                    onChange={(e) => setFilterStartMonth(Number(e.target.value))}
                  >
                    {MONTH_OPTIONS.map((month) => (
                      <MenuItem key={month.value} value={month.value}>
                        {month.label}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>

                <Typography variant="body2" color="text.secondary" sx={{ ml: 2 }}>
                  До:
                </Typography>
                <FormControl size="small" sx={{ minWidth: 100 }}>
                  <InputLabel>Год</InputLabel>
                  <Select
                    value={filterEndYear}
                    label="Год"
                    onChange={(e) => setFilterEndYear(Number(e.target.value))}
                  >
                    {availableYears.map((year) => (
                      <MenuItem key={year} value={year}>
                        {year}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
                <FormControl size="small" sx={{ minWidth: 140 }}>
                  <InputLabel>Месяц</InputLabel>
                  <Select
                    value={filterEndMonth}
                    label="Месяц"
                    onChange={(e) => setFilterEndMonth(Number(e.target.value))}
                  >
                    {MONTH_OPTIONS.map((month) => (
                      <MenuItem key={month.value} value={month.value}>
                        {month.label}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Box>
            </Box>

            {sortedTransactions.length === 0 ? (
              <Typography variant="body2" color="text.secondary" sx={{ textAlign: 'center', py: 3 }}>
                Нет транзакций. Добавьте доход, расход или перевод.
              </Typography>
            ) : (
              <TableContainer component={Paper} variant="outlined">
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Дата</TableCell>
                      <TableCell>Название</TableCell>
                      <TableCell align="right">Сумма</TableCell>
                      <TableCell align="right">Действия</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {sortedTransactions.map((transaction) => {
                      const isTransfer = transaction.type === 'TransferIn' || transaction.type === 'TransferOut';
                      const isIncome = transaction.type === 'Income';
                      const isExpense = transaction.type === 'Expense';
                      
                      let displayName = transaction.name;
                      if (isTransfer) {
                        displayName = transaction.type === 'TransferOut' 
                          ? `↗ ${transaction.name}` 
                          : `↙ ${transaction.name}`;
                      }

                      return (
                        <TableRow key={transaction.id}>
                          <TableCell>{formatDateShort(transaction.operationDate)}</TableCell>
                          <TableCell>
                            <Box>
                              <Box sx={{ display: 'flex', alignItems: 'center', flexWrap: 'wrap', gap: 0.5 }}>
                                {displayName}
                                {isTransfer && (
                                  <Chip
                                    label={transaction.type === 'TransferOut' ? 'Исходящий' : 'Входящий'}
                                    size="small"
                                    variant="outlined"
                                  />
                                )}
                              </Box>
                              {isTransfer && transaction.relatedWalletName && (
                                <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 0.5 }}>
                                  {transaction.type === 'TransferOut' 
                                    ? `→ ${transaction.relatedWalletName}`
                                    : `← ${transaction.relatedWalletName}`}
                                </Typography>
                              )}
                            </Box>
                          </TableCell>
                          <TableCell align="right">
                            <Typography
                              color={
                                isIncome
                                  ? 'success.main'
                                  : isExpense
                                    ? 'error.main'
                                    : transaction.type === 'TransferOut'
                                      ? 'warning.main'
                                      : 'info.main'
                              }
                              fontWeight="bold"
                            >
                              {isIncome ? '+' : isExpense ? '-' : transaction.type === 'TransferOut' ? '↗' : '↙'}
                              {formatMoney(transaction.amount)}
                            </Typography>
                          </TableCell>
                          <TableCell align="right">
                            <Box sx={{ display: 'flex', gap: 0.5, justifyContent: 'flex-end' }}>
                              {!isTransfer && (
                                <IconButton
                                  size="small"
                                  color="primary"
                                  onClick={() => handleEditTransaction(transaction)}
                                >
                                  <EditIcon fontSize="small" />
                                </IconButton>
                              )}
                              <IconButton
                                size="small"
                                color="error"
                                onClick={() => handleDeleteClick(transaction.id)}
                              >
                                <DeleteIcon fontSize="small" />
                              </IconButton>
                            </Box>
                          </TableCell>
                        </TableRow>
                      );
                    })}
                  </TableBody>
                </Table>
              </TableContainer>
            )}
          </CardContent>
        </Card>
      )}

      {/* Dialog редактирования */}
      <Dialog open={editDialogOpen} onClose={handleCloseEditDialog} maxWidth="sm" fullWidth>
        <DialogTitle>Редактировать кошелёк</DialogTitle>
        <DialogContent>
          <Stack spacing={3} sx={{ mt: 1 }}>
            <TextField
              label="Название"
              value={formState.name}
              onChange={(e) => setFormState({ ...formState, name: e.target.value })}
              fullWidth
              required
              autoFocus
            />

            {!isSavings && (
              <FormControlLabel
                control={
                  <Switch
                    checked={formState.allowNegativeBalance}
                    onChange={(e) =>
                      setFormState({ ...formState, allowNegativeBalance: e.target.checked })
                    }
                  />
                }
                label="Разрешить отрицательный баланс"
              />
            )}

            {isSavings && (
              <>
                <TextField
                  label="Целевая сумма"
                  type="number"
                  value={formState.targetAmount}
                  onChange={(e) => setFormState({ ...formState, targetAmount: e.target.value })}
                  fullWidth
                  required
                  inputProps={{ min: 0.01, step: 0.01 }}
                  InputProps={{
                    endAdornment: <InputAdornment position="end">₽</InputAdornment>,
                  }}
                />

                <TextField
                  label="Целевая дата"
                  type="date"
                  value={formState.targetDate}
                  onChange={(e) => setFormState({ ...formState, targetDate: e.target.value })}
                  fullWidth
                  InputLabelProps={{ shrink: true }}
                />
              </>
            )}
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseEditDialog}>Назад</Button>
          <Button
            onClick={handleArchiveClick}
            color={wallet.isArchived ? 'primary' : 'error'}
            startIcon={<ArchiveIcon />}
          >
            {wallet.isArchived ? 'Разархивировать' : 'Архивировать'}
          </Button>
          <Button
            onClick={handleSave}
            variant="contained"
            disabled={updateWalletMutation.isPending}
          >
            {updateWalletMutation.isPending ? 'Сохранение...' : 'Сохранить'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Dialog подтверждения архивации */}
      <Dialog open={archiveConfirmOpen} onClose={() => setArchiveConfirmOpen(false)}>
        <DialogTitle>
          {wallet.isArchived ? 'Подтверждение разархивации' : 'Подтверждение архивации'}
        </DialogTitle>
        <DialogContent>
          <Typography>
            {wallet.isArchived ? (
              <>
                Вы уверены, что хотите разархивировать кошелёк &quot;{wallet.name}&quot;?
                <br />
                После разархивации кошелёк снова станет доступен для операций.
              </>
            ) : (
              <>
                Вы уверены, что хотите архивировать кошелёк &quot;{wallet.name}&quot;?
                <br />
                Архивированный кошелёк нельзя использовать для транзакций, но его можно будет
                разархивировать позже.
              </>
            )}
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setArchiveConfirmOpen(false)}>Отмена</Button>
          <Button
            onClick={handleArchiveConfirm}
            color={wallet.isArchived ? 'primary' : 'error'}
            variant="contained"
            disabled={archiveWalletMutation.isPending}
            startIcon={<ArchiveIcon />}
          >
            {archiveWalletMutation.isPending
              ? wallet.isArchived
                ? 'Разархивация...'
                : 'Архивация...'
              : wallet.isArchived
                ? 'Разархивировать'
                : 'Архивировать'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Dialog создания/редактирования транзакции */}
      <Dialog open={transactionDialogOpen} onClose={handleCloseTransactionDialog} maxWidth="sm" fullWidth>
        <DialogTitle>
          {transactionEditMode === 'edit'
            ? transactionType === 'Income'
              ? 'Редактировать доход'
              : 'Редактировать расход'
            : transactionType === 'Income'
              ? 'Добавить доход'
              : 'Добавить расход'}
        </DialogTitle>
        <DialogContent>
          <Stack spacing={3} sx={{ mt: 1 }}>
            <TextField
              label="Название"
              value={transactionForm.name}
              onChange={(e) => setTransactionForm({ ...transactionForm, name: e.target.value })}
              fullWidth
              required
              autoFocus
            />

            <TextField
              label="Сумма"
              type="number"
              value={transactionForm.amount}
              onChange={(e) => setTransactionForm({ ...transactionForm, amount: e.target.value })}
              fullWidth
              required
              inputProps={{ min: 0.01, step: 0.01 }}
              InputProps={{
                endAdornment: <InputAdornment position="end">₽</InputAdornment>,
              }}
            />

            <TextField
              label="Дата операции"
              type="date"
              value={transactionForm.operationDate}
              onChange={(e) => setTransactionForm({ ...transactionForm, operationDate: e.target.value })}
              fullWidth
              required
              InputLabelProps={{ shrink: true }}
            />

            <FormControl fullWidth>
              <InputLabel>Категория (опционально)</InputLabel>
              <Select
                value={transactionForm.categoryId}
                label="Категория (опционально)"
                onChange={(e) => setTransactionForm({ ...transactionForm, categoryId: e.target.value })}
              >
                <MenuItem value="">Без категории</MenuItem>
                {(transactionType === 'Income' ? incomeCategories : expenseCategories).map((category) => (
                  <MenuItem key={category.id} value={category.id}>
                    {category.icon} {category.name}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseTransactionDialog}>Отмена</Button>
          <Button
            onClick={handleSaveTransaction}
            variant="contained"
            color={transactionType === 'Income' ? 'success' : 'error'}
            disabled={createTransactionMutation.isPending || updateTransactionMutation.isPending}
          >
            {transactionEditMode === 'edit'
              ? updateTransactionMutation.isPending
                ? 'Сохранение...'
                : 'Сохранить'
              : createTransactionMutation.isPending
                ? 'Создание...'
                : 'Создать'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Dialog подтверждения удаления транзакции */}
      <Dialog open={deleteConfirmOpen} onClose={() => setDeleteConfirmOpen(false)}>
        <DialogTitle>Подтверждение удаления</DialogTitle>
        <DialogContent>
          <Typography>
            Вы уверены, что хотите удалить эту транзакцию?
            <br />
            Это действие нельзя отменить.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteConfirmOpen(false)}>Отмена</Button>
          <Button
            onClick={handleDeleteConfirm}
            color="error"
            variant="contained"
            disabled={deleteTransactionMutation.isPending}
            startIcon={<DeleteIcon />}
          >
            {deleteTransactionMutation.isPending ? 'Удаление...' : 'Удалить'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Dialog создания перевода */}
      <Dialog open={transferDialogOpen} onClose={handleCloseTransferDialog} maxWidth="sm" fullWidth>
        <DialogTitle>Создать перевод</DialogTitle>
        <DialogContent>
          <Stack spacing={3} sx={{ mt: 1 }}>
            <FormControl fullWidth required>
              <InputLabel>Откуда</InputLabel>
              <Select
                value={transferForm.fromWalletId}
                label="Откуда"
                onChange={(e) => setTransferForm({ ...transferForm, fromWalletId: e.target.value })}
              >
                {fromWallets.map((w) => (
                  <MenuItem key={w.id} value={w.id}>
                    {w.name} ({formatMoney(w.balance)})
                  </MenuItem>
                ))}
              </Select>
            </FormControl>

            <FormControl fullWidth required>
              <InputLabel>Куда</InputLabel>
              <Select
                value={transferForm.toWalletId}
                label="Куда"
                onChange={(e) => setTransferForm({ ...transferForm, toWalletId: e.target.value })}
              >
                {toWallets.map((w) => (
                  <MenuItem key={w.id} value={w.id}>
                    {w.name} ({formatMoney(w.balance)})
                  </MenuItem>
                ))}
              </Select>
            </FormControl>

            <TextField
              label="Название"
              value={transferForm.name}
              onChange={(e) => setTransferForm({ ...transferForm, name: e.target.value })}
              fullWidth
              required
              autoFocus
            />

            <TextField
              label="Сумма"
              type="number"
              value={transferForm.amount}
              onChange={(e) => setTransferForm({ ...transferForm, amount: e.target.value })}
              fullWidth
              required
              inputProps={{ min: 0.01, step: 0.01 }}
              InputProps={{
                endAdornment: <InputAdornment position="end">₽</InputAdornment>,
              }}
            />

            <TextField
              label="Дата операции"
              type="date"
              value={transferForm.operationDate}
              onChange={(e) => setTransferForm({ ...transferForm, operationDate: e.target.value })}
              fullWidth
              required
              InputLabelProps={{ shrink: true }}
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseTransferDialog}>Отмена</Button>
          <Button
            onClick={handleSaveTransfer}
            variant="contained"
            color="primary"
            disabled={createTransferMutation.isPending}
            startIcon={<SwapHorizIcon />}
          >
            {createTransferMutation.isPending ? 'Создание...' : 'Создать перевод'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Error Dialog */}
      <ErrorDialog
        open={errorDialog.open}
        message={errorDialog.message}
        onClose={() => setErrorDialog({ open: false, message: '' })}
      />
    </Box>
  );
}

type ErrorDialogProps = {
  open: boolean;
  message: string;
  onClose: () => void;
};

function ErrorDialog({ open, message, onClose }: ErrorDialogProps) {
  return (
    <Dialog open={open} onClose={onClose} maxWidth="xs" fullWidth>
      <DialogTitle>Ошибка</DialogTitle>
      <DialogContent dividers>
        <Typography variant="body2" sx={{ whiteSpace: 'pre-line' }}>
          {message}
        </Typography>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} autoFocus>
          Ок
        </Button>
      </DialogActions>
    </Dialog>
  );
}

export default WalletPage;