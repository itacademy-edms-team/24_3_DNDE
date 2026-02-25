import { useState } from 'react';
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

const fetchWallet = async (walletId: string): Promise<Wallet> => {
  const res = await fetch(`/api/finance/Wallets/${walletId}`, {
    credentials: 'include',
  });
  if (!res.ok) {
    throw new Error(`HTTP error! status: ${res.status}`);
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
    const errorText = await res.text();
    throw new Error(errorText || `HTTP error! status: ${res.status}`);
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
    const errorText = await res.text();
    throw new Error(errorText || `HTTP error! status: ${res.status}`);
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

const fetchTransactions = async (walletId: string): Promise<Transaction[]> => {
  const res = await fetch(`/api/finance/Wallets/${walletId}/Transactions`, {
    credentials: 'include',
  });
  if (!res.ok) {
    throw new Error(`HTTP error! status: ${res.status}`);
  }
  const data: TransactionsResponse = await res.json();
  return data.transactions;
};

const fetchCategories = async (type: 'Income' | 'Expense'): Promise<Category[]> => {
  const res = await fetch('/api/finance/Categories', {
    credentials: 'include',
  });
  if (!res.ok) {
    throw new Error(`HTTP error! status: ${res.status}`);
  }
  const data: CategoriesResponse = await res.json();
  return data.categories.filter((cat) => cat.type === type);
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
    const errorText = await res.text();
    throw new Error(errorText || `HTTP error! status: ${res.status}`);
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
    const errorText = await res.text();
    throw new Error(errorText || `HTTP error! status: ${res.status}`);
  }

  return await res.json();
};

const deleteTransaction = async (transactionId: string): Promise<void> => {
  const res = await fetch(`/api/finance/Transactions/${transactionId}`, {
    method: 'DELETE',
    credentials: 'include',
  });

  if (!res.ok) {
    const errorText = await res.text();
    throw new Error(errorText || `HTTP error! status: ${res.status}`);
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
    const errorText = await res.text();
    throw new Error(errorText || `HTTP error! status: ${res.status}`);
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

type TransactionFormState = {
  name: string;
  amount: string;
  operationDate: string;
  categoryId: string;
};

function WalletPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { walletId } = useParams<{ walletId: string }>();
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [archiveConfirmOpen, setArchiveConfirmOpen] = useState(false);
  const [transactionDialogOpen, setTransactionDialogOpen] = useState(false);
  const [transactionType, setTransactionType] = useState<'Income' | 'Expense'>('Income');
  const [transactionEditMode, setTransactionEditMode] = useState<'create' | 'edit'>('create');
  const [transactionToEdit, setTransactionToEdit] = useState<Transaction | null>(null);
  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);
  const [transactionToDelete, setTransactionToDelete] = useState<string | null>(null);
  const [formState, setFormState] = useState<WalletFormState>({
    name: '',
    allowNegativeBalance: true,
    targetAmount: '',
    targetDate: '',
  });
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

  const { data: transactions = [] } = useQuery({
    queryKey: ['transactions', walletId],
    queryFn: () => fetchTransactions(walletId!),
    enabled: !!walletId && !wallet?.isArchived,
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

  const updateWalletMutation = useMutation({
    mutationFn: (payload: UpdateWalletPayload) => updateWallet(walletId!, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['wallet', walletId] });
      queryClient.invalidateQueries({ queryKey: ['wallets'] });
      setEditDialogOpen(false);
    },
    onError: (error: Error) => {
      console.error('Failed to update wallet:', error);
      alert(`Ошибка обновления кошелька: ${error.message}`);
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
      alert(`Ошибка архивации кошелька: ${error.message}`);
    },
  });

  const createTransactionMutation = useMutation({
    mutationFn: (payload: CreateIncomePayload | CreateExpensePayload) =>
      transactionType === 'Income' ? createIncome(payload as CreateIncomePayload) : createExpense(payload as CreateExpensePayload),
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
      console.error('Failed to create transaction:', error);
      alert(`Ошибка создания транзакции: ${error.message}`);
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
      alert(`Ошибка обновления транзакции: ${error.message}`);
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
      alert(`Ошибка удаления транзакции: ${error.message}`);
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
      alert('Введите название кошелька');
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
        alert('Для накопительного кошелька необходимо указать целевую сумму больше 0');
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
      alert('Переводы нельзя редактировать');
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
      alert('Введите название транзакции');
      return;
    }

    const amount = parseFloat(transactionForm.amount);
    if (!transactionForm.amount || isNaN(amount) || amount <= 0) {
      alert('Введите корректную сумму (больше 0)');
      return;
    }

    if (!transactionForm.operationDate) {
      alert('Выберите дату операции');
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

  const filteredTransactions = transactions.filter(
    (t) => t.type === 'Income' || t.type === 'Expense'
  );

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
              </Box>
            </Box>

            {filteredTransactions.length === 0 ? (
              <Typography variant="body2" color="text.secondary" sx={{ textAlign: 'center', py: 3 }}>
                Нет транзакций. Добавьте доход или расход.
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
                    {filteredTransactions
                      .sort((a, b) => new Date(b.operationDate).getTime() - new Date(a.operationDate).getTime())
                      .map((transaction) => (
                        <TableRow key={transaction.id}>
                          <TableCell>{formatDateShort(transaction.operationDate)}</TableCell>
                          <TableCell>{transaction.name}</TableCell>
                          <TableCell align="right">
                            <Typography
                              color={transaction.type === 'Income' ? 'success.main' : 'error.main'}
                              fontWeight="bold"
                            >
                              {transaction.type === 'Income' ? '+' : '-'}
                              {formatMoney(transaction.amount)}
                            </Typography>
                          </TableCell>
                          <TableCell align="right">
                            <Box sx={{ display: 'flex', gap: 0.5, justifyContent: 'flex-end' }}>
                              <IconButton
                                size="small"
                                color="primary"
                                onClick={() => handleEditTransaction(transaction)}
                                disabled={transaction.type === 'TransferIn' || transaction.type === 'TransferOut'}
                              >
                                <EditIcon fontSize="small" />
                              </IconButton>
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
                      ))}
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
    </Box>
  );
}

export default WalletPage;