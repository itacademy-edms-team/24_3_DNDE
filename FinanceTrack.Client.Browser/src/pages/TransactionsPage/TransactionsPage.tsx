import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import AddIcon from '@mui/icons-material/Add';
import DeleteIcon from '@mui/icons-material/Delete';
import EditIcon from '@mui/icons-material/Edit';
import RefreshIcon from '@mui/icons-material/Refresh';
import SearchIcon from '@mui/icons-material/Search';
import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControlLabel,
  IconButton,
  InputAdornment,
  Stack,
  Switch,
  Tab,
  Tabs,
  TextField,
  Typography,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  CircularProgress,
} from '@mui/material';
import {
  createExpense,
  createIncome,
  deleteExpense,
  deleteIncome,
  fetchExpensesByIncome,
  fetchIncomes,
  updateExpense,
  updateIncome,
} from './api';
import { ExpenseTransaction, IncomeTransaction } from './types';

type IncomeFormState = {
  name: string;
  amount: string;
  operationDate: string;
  isMonthly: boolean;
};

type ExpenseFormState = IncomeFormState;

type DialogMode = 'create' | 'edit';

type ConfirmDialogState =
  | { type: 'income'; targetId: string; open: true }
  | { type: 'expense'; targetId: string; incomeId?: string; open: true }
  | { open: false; type?: undefined; targetId?: undefined; incomeId?: undefined };

function formatDate(dateStr: string): string {
  if (!dateStr) return '';
  const [year, month, day] = dateStr.split('-');
  if (!year || !month || !day) return dateStr;
  return `${day}.${month}.${year}`;
}

function TransactionsPage() {
  const [incomes, setIncomes] = useState<IncomeTransaction[]>([]);
  const [expensesByIncome, setExpensesByIncome] = useState<Record<string, ExpenseTransaction[]>>(
    {}
  );
  const expensesCacheRef = useRef<Record<string, ExpenseTransaction[]>>({});
  const selectedIncomeRef = useRef<string | null>(null);
  const [selectedIncomeId, setSelectedIncomeId] = useState<string | null>(null);
  const [selectedExpenseId, setSelectedExpenseId] = useState<string | null>(null);
  const [loadingIncomes, setLoadingIncomes] = useState(false);
  const [loadingExpenses, setLoadingExpenses] = useState(false);
  const [filterText, setFilterText] = useState('');
  const [errorDialog, setErrorDialog] = useState<{ open: boolean; message: string }>({
    open: false,
    message: '',
  });

  const [incomeDialogMode, setIncomeDialogMode] = useState<DialogMode>('create');
  const [expenseDialogMode, setExpenseDialogMode] = useState<DialogMode>('create');
  const [incomeDialogOpen, setIncomeDialogOpen] = useState(false);
  const [expenseDialogOpen, setExpenseDialogOpen] = useState(false);
  const [confirmDialog, setConfirmDialog] = useState<ConfirmDialogState>({ open: false });

  const [incomeForm, setIncomeForm] = useState<IncomeFormState>({
    name: '',
    amount: '',
    operationDate: '',
    isMonthly: false,
  });
  const [expenseForm, setExpenseForm] = useState<ExpenseFormState>({
    name: '',
    amount: '',
    operationDate: '',
    isMonthly: false,
  });

  const selectedIncome = useMemo(
    () => incomes.find(i => i.id === selectedIncomeId) ?? null,
    [incomes, selectedIncomeId]
  );

  const currentExpenses = useMemo(() => {
    if (!selectedIncomeId) return [];
    return expensesByIncome[selectedIncomeId] ?? [];
  }, [expensesByIncome, selectedIncomeId]);

  const filteredExpenses = useMemo(() => {
    if (!filterText.trim()) return currentExpenses;
    const query = filterText.trim().toLowerCase();
    return currentExpenses.filter(e => e.name.toLowerCase().includes(query));
  }, [currentExpenses, filterText]);

  const validateAmount = (raw: string): number | null => {
    const value = Number(raw.replace(',', '.'));
    if (Number.isNaN(value) || value < 0.01) {
      return null;
    }
    return value;
  };

  const showError = (message: string) => {
    setErrorDialog({ open: true, message });
  };

  const loadExpenses = useCallback(
    async (incomeId: string, options: { force?: boolean } = {}) => {
      if (!incomeId) return;
      if (!options.force && expensesCacheRef.current[incomeId]) {
        setExpensesByIncome(prev => ({ ...prev }));
        return;
      }
      setLoadingExpenses(true);
      try {
        const data = await fetchExpensesByIncome(incomeId);
        setExpensesByIncome(prev => {
          const next = { ...prev, [incomeId]: data };
          expensesCacheRef.current = next;
          return next;
        });
      } catch (e) {
        const err = e as Error;
        showError(err.message ?? 'Ошибка загрузки расходов');
      } finally {
        setLoadingExpenses(false);
      }
    },
    []
  );

  const loadIncomes = useCallback(
    async (preferredId?: string) => {
      setLoadingIncomes(true);
      try {
        const data = await fetchIncomes();
        setIncomes(data);
        const previousSelected = selectedIncomeRef.current;
        const nextSelected =
          preferredId ??
          (previousSelected && data.some(i => i.id === previousSelected)
            ? previousSelected
            : data[0]?.id ?? null);
        setSelectedIncomeId(nextSelected);
        if (nextSelected && !expensesCacheRef.current[nextSelected]) {
          void loadExpenses(nextSelected, { force: false });
        }
      } catch (e) {
        const err = e as Error;
        showError(err.message ?? 'Ошибка загрузки доходов');
      } finally {
        setLoadingIncomes(false);
      }
    },
    [loadExpenses]
  );

  useEffect(() => {
    selectedIncomeRef.current = selectedIncomeId;
  }, [selectedIncomeId]);

  useEffect(() => {
    setSelectedExpenseId(null);
  }, [selectedIncomeId]);

  useEffect(() => {
    void loadIncomes();
  }, [loadIncomes]);

  useEffect(() => {
    if (selectedIncomeId) {
      void loadExpenses(selectedIncomeId);
    }
  }, [selectedIncomeId, loadExpenses]);

  const openIncomeDialog = (mode: DialogMode, income?: IncomeTransaction) => {
    setIncomeDialogMode(mode);
    setIncomeForm({
      name: income?.name ?? '',
      amount: income ? income.amount.toString() : '',
      operationDate: income?.operationDate ?? '',
      isMonthly: income?.isMonthly ?? false,
    });
    setIncomeDialogOpen(true);
  };

  const openExpenseDialog = (mode: DialogMode, expense?: ExpenseTransaction) => {
    setExpenseDialogMode(mode);
    setExpenseForm({
      name: expense?.name ?? '',
      amount: expense ? expense.amount.toString() : '',
      operationDate: expense?.operationDate ?? '',
      isMonthly: expense?.isMonthly ?? false,
    });
    setSelectedExpenseId(expense?.id ?? null);
    setExpenseDialogOpen(true);
  };

  const handleSaveIncome = async () => {
    const amount = validateAmount(incomeForm.amount);
    if (!incomeForm.name.trim() || amount === null || !/^\d{4}-\d{2}-\d{2}$/.test(incomeForm.operationDate)) {
      showError('Проверьте корректность полей дохода');
      return;
    }

    const payload = {
      name: incomeForm.name.trim(),
      amount,
      operationDate: incomeForm.operationDate,
      isMonthly: incomeForm.isMonthly,
    };

    try {
      if (incomeDialogMode === 'create') {
        const id = await createIncome(payload);
        await loadIncomes(id);
        setSelectedIncomeId(id);
      } else if (selectedIncome) {
        await updateIncome(selectedIncome.id, payload);
        await loadIncomes(selectedIncome.id);
      }
      setIncomeDialogOpen(false);
    } catch (e) {
      const err = e as Error;
      showError(err.message ?? 'Не удалось сохранить доход');
    }
  };

  const handleSaveExpense = async () => {
    if (!selectedIncomeId) {
      showError('Сначала выберите доход');
      return;
    }
    const amount = validateAmount(expenseForm.amount);
    if (!expenseForm.name.trim() || amount === null || !/^\d{4}-\d{2}-\d{2}$/.test(expenseForm.operationDate)) {
      showError('Проверьте корректность полей расхода');
      return;
    }

    const payloadBase = {
      name: expenseForm.name.trim(),
      amount,
      operationDate: expenseForm.operationDate,
      isMonthly: expenseForm.isMonthly,
      incomeTransactionId: selectedIncomeId,
    };

    try {
      if (expenseDialogMode === 'create') {
        const id = await createExpense(payloadBase);
        await loadExpenses(selectedIncomeId, { force: true });
        setSelectedExpenseId(id);
      } else if (selectedExpenseId) {
        await updateExpense({ ...payloadBase, transactionId: selectedExpenseId });
        await loadExpenses(selectedIncomeId, { force: true });
      }
      setExpenseDialogOpen(false);
    } catch (e) {
      const err = e as Error;
      showError(err.message ?? 'Не удалось сохранить расход');
    }
  };

  const handleDeleteIncome = async () => {
    if (!confirmDialog.open || confirmDialog.type !== 'income') return;
    try {
      await deleteIncome(confirmDialog.targetId);
      setConfirmDialog({ open: false });
      setExpensesByIncome(prev => {
        const next = { ...prev };
        delete next[confirmDialog.targetId];
        return next;
      });
      await loadIncomes();
    } catch (e) {
      const err = e as Error;
      showError(err.message ?? 'Не удалось удалить доход');
    }
  };

  const handleDeleteExpense = async () => {
    if (!confirmDialog.open || confirmDialog.type !== 'expense' || !confirmDialog.targetId) return;
    const incomeId = confirmDialog.incomeId ?? selectedIncomeId;
    if (!incomeId) return;
    try {
      await deleteExpense(confirmDialog.targetId);
      setConfirmDialog({ open: false });
      await loadExpenses(incomeId, { force: true });
    } catch (e) {
      const err = e as Error;
      showError(err.message ?? 'Не удалось удалить расход');
    }
  };

  const handleRefreshExpenses = () => {
    if (selectedIncomeId) {
      void loadExpenses(selectedIncomeId, { force: true });
    }
  };

  return (
    <Box sx={{ maxWidth: 1100, mx: 'auto', px: { xs: 2, sm: 3 }, py: 3, display: 'flex', flexDirection: 'column', gap: 2 }}>
      <Stack direction="row" justifyContent="space-between" alignItems="center" flexWrap="wrap" gap={1.5}>
        <Typography variant="h4">Transactions</Typography>
        <Stack direction="row" spacing={1}>
          <Button startIcon={<AddIcon />} variant="contained" onClick={() => openIncomeDialog('create')}>
            Add income
          </Button>
          <Button startIcon={<RefreshIcon />} variant="outlined" onClick={() => void loadIncomes()}>
            Refresh incomes
          </Button>
        </Stack>
      </Stack>

      <Paper variant="outlined" sx={{ p: 2 }}>
        {loadingIncomes ? (
          <Stack alignItems="center" justifyContent="center" sx={{ py: 3 }}>
            <CircularProgress size={26} />
          </Stack>
        ) : incomes.length === 0 ? (
          <Stack alignItems="center" spacing={1} sx={{ py: 3 }}>
            <Typography variant="body2" color="text.secondary">
              Нет доходов. Добавьте первый, чтобы видеть связанные расходы.
            </Typography>
            <Button startIcon={<AddIcon />} variant="contained" onClick={() => openIncomeDialog('create')}>
              Add income
            </Button>
          </Stack>
        ) : (
          <Tabs
            value={selectedIncomeId}
            onChange={(_, v) => setSelectedIncomeId(v)}
            variant="scrollable"
            scrollButtons="auto"
            allowScrollButtonsMobile
          >
            {incomes.map(income => (
              <Tab
                key={income.id}
                value={income.id}
                label={
                  <Box sx={{ textTransform: 'none' }}>
                    <Typography variant="body2" fontWeight={600}>
                      {income.name}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {income.amount.toFixed(2)} ₽ • {formatDate(income.operationDate)}
                    </Typography>
                  </Box>
                }
              />
            ))}
          </Tabs>
        )}
      </Paper>

      <Paper variant="outlined" sx={{ p: 2, display: 'flex', flexDirection: 'column', gap: 2 }}>
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1} justifyContent="space-between" alignItems={{ xs: 'stretch', sm: 'center' }}>
          <Stack direction="row" spacing={1} alignItems="center">
            <Button
              startIcon={<EditIcon />}
              variant="outlined"
              disabled={!selectedIncome}
              onClick={() => selectedIncome && openIncomeDialog('edit', selectedIncome)}
            >
              Edit income
            </Button>
            <Button
              startIcon={<DeleteIcon />}
              color="error"
              variant="outlined"
              disabled={!selectedIncome}
              onClick={() =>
                selectedIncome &&
                setConfirmDialog({ open: true, type: 'income', targetId: selectedIncome.id })
              }
            >
              Delete income
            </Button>
          </Stack>
          <Stack direction="row" spacing={1} alignItems="center">
            <Button
              startIcon={<AddIcon />}
              variant="contained"
              disabled={!selectedIncome}
              onClick={() => openExpenseDialog('create')}
            >
              Add expense
            </Button>
            <Button
              startIcon={<RefreshIcon />}
              variant="outlined"
              disabled={!selectedIncome}
              onClick={handleRefreshExpenses}
            >
              Refresh expenses
            </Button>
          </Stack>
        </Stack>

        <TextField
          placeholder="Search expenses by name"
          value={filterText}
          onChange={e => setFilterText(e.target.value)}
          size="small"
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon fontSize="small" />
              </InputAdornment>
            ),
          }}
        />
        <TableContainer component={Paper} variant="outlined">
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Name</TableCell>
                <TableCell width="130">Amount</TableCell>
                <TableCell width="130">Date</TableCell>
                <TableCell width="120">Type</TableCell>
                <TableCell align="right" width="110">
                  Actions
                </TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {loadingExpenses ? (
                <TableRow>
                  <TableCell colSpan={5} align="center">
                    <Stack direction="row" spacing={1} justifyContent="center" alignItems="center">
                      <CircularProgress size={20} />
                      <Typography variant="body2">Loading expenses...</Typography>
                    </Stack>
                  </TableCell>
                </TableRow>
              ) : filteredExpenses.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={5} align="center">
                    <Typography variant="body2" color="text.secondary">
                      {selectedIncome ? 'No expenses for this income yet' : 'Select an income to see expenses'}
                    </Typography>
                  </TableCell>
                </TableRow>
              ) : (
                filteredExpenses.map(expense => (
                  <TableRow key={expense.id} hover selected={expense.id === selectedExpenseId}>
                    <TableCell>{expense.name}</TableCell>
                    <TableCell>{expense.amount.toFixed(2)} ₽</TableCell>
                    <TableCell>{formatDate(expense.operationDate)}</TableCell>
                    <TableCell>{expense.isMonthly ? 'Monthly' : 'Single'}</TableCell>
                    <TableCell align="right">
                      <IconButton
                        size="small"
                        onClick={() => {
                          setSelectedExpenseId(expense.id);
                          openExpenseDialog('edit', expense);
                        }}
                      >
                        <EditIcon fontSize="small" />
                      </IconButton>
                      <IconButton
                        size="small"
                        color="error"
                        onClick={() => {
                          setSelectedExpenseId(expense.id);
                          setConfirmDialog({
                            open: true,
                            type: 'expense',
                            targetId: expense.id,
                            incomeId: selectedIncomeId ?? undefined,
                          });
                        }}
                      >
                        <DeleteIcon fontSize="small" />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </TableContainer>
      </Paper>

      <IncomeDialog
        open={incomeDialogOpen}
        mode={incomeDialogMode}
        form={incomeForm}
        onClose={() => setIncomeDialogOpen(false)}
        onChange={setIncomeForm}
        onSubmit={handleSaveIncome}
      />

      <ExpenseDialog
        open={expenseDialogOpen}
        mode={expenseDialogMode}
        form={expenseForm}
        onClose={() => setExpenseDialogOpen(false)}
        onChange={setExpenseForm}
        onSubmit={handleSaveExpense}
        disabled={!selectedIncomeId}
      />

      <ConfirmDialog
        open={confirmDialog.open}
        title={
          confirmDialog.type === 'income'
            ? 'Delete income'
            : confirmDialog.type === 'expense'
              ? 'Delete expense'
              : ''
        }
        description={
          confirmDialog.type === 'income'
            ? 'Удалить выбранный доход и связанные расходы?'
            : 'Удалить выбранный расход?'
        }
        onCancel={() => setConfirmDialog({ open: false })}
        onConfirm={
          confirmDialog.type === 'income' ? handleDeleteIncome : handleDeleteExpense
        }
      />

      <ErrorDialog
        open={errorDialog.open}
        message={errorDialog.message}
        onClose={() => setErrorDialog({ open: false, message: '' })}
      />
    </Box>
  );
}

type FormDialogProps<TForm> = {
  open: boolean;
  mode: DialogMode;
  form: TForm;
  onChange: (form: TForm) => void;
  onSubmit: () => void;
  onClose: () => void;
  disabled?: boolean;
};

function IncomeDialog({
  open,
  mode,
  form,
  onChange,
  onSubmit,
  onClose,
}: FormDialogProps<IncomeFormState>) {
  return (
    <Dialog open={open} onClose={onClose} maxWidth="xs" fullWidth>
      <DialogTitle>{mode === 'create' ? 'Add income' : 'Edit income'}</DialogTitle>
      <DialogContent dividers>
        <Stack spacing={2} sx={{ mt: 1 }}>
          <TextField
            label="Name"
            value={form.name}
            onChange={e => onChange({ ...form, name: e.target.value })}
            fullWidth
          />
          <TextField
            label="Amount"
            type="number"
            inputProps={{ step: '0.01', min: '0.01' }}
            value={form.amount}
            onChange={e => onChange({ ...form, amount: e.target.value })}
            fullWidth
          />
          <TextField
            label="Operation date"
            type="date"
            InputLabelProps={{ shrink: true }}
            value={form.operationDate}
            onChange={e => onChange({ ...form, operationDate: e.target.value })}
            fullWidth
          />
          <FormControlLabel
            control={
              <Switch
                checked={form.isMonthly}
                onChange={e => onChange({ ...form, isMonthly: e.target.checked })}
              />
            }
            label="Is monthly?"
          />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button onClick={onSubmit} variant="contained">
          {mode === 'create' ? 'Add' : 'Save'}
        </Button>
      </DialogActions>
    </Dialog>
  );
}

function ExpenseDialog({
  open,
  mode,
  form,
  onChange,
  onSubmit,
  onClose,
  disabled,
}: FormDialogProps<ExpenseFormState>) {
  return (
    <Dialog open={open} onClose={onClose} maxWidth="xs" fullWidth>
      <DialogTitle>{mode === 'create' ? 'Add expense' : 'Edit expense'}</DialogTitle>
      <DialogContent dividers>
        <Stack spacing={2} sx={{ mt: 1 }}>
          <TextField
            label="Name"
            value={form.name}
            onChange={e => onChange({ ...form, name: e.target.value })}
            fullWidth
          />
          <TextField
            label="Amount"
            type="number"
            inputProps={{ step: '0.01', min: '0.01' }}
            value={form.amount}
            onChange={e => onChange({ ...form, amount: e.target.value })}
            fullWidth
          />
          <TextField
            label="Operation date"
            type="date"
            InputLabelProps={{ shrink: true }}
            value={form.operationDate}
            onChange={e => onChange({ ...form, operationDate: e.target.value })}
            fullWidth
          />
          <FormControlLabel
            control={
              <Switch
                checked={form.isMonthly}
                onChange={e => onChange({ ...form, isMonthly: e.target.checked })}
              />
            }
            label="Is monthly?"
          />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button onClick={onSubmit} variant="contained" disabled={disabled}>
          {mode === 'create' ? 'Add' : 'Save'}
        </Button>
      </DialogActions>
    </Dialog>
  );
}

type ConfirmDialogProps = {
  open: boolean;
  title: string;
  description: string;
  onConfirm: () => void;
  onCancel: () => void;
};

function ConfirmDialog({ open, title, description, onConfirm, onCancel }: ConfirmDialogProps) {
  return (
    <Dialog open={open} onClose={onCancel} maxWidth="xs" fullWidth>
      <DialogTitle>{title}</DialogTitle>
      <DialogContent dividers>
        <Typography variant="body2">{description}</Typography>
      </DialogContent>
      <DialogActions>
        <Button onClick={onCancel}>Cancel</Button>
        <Button onClick={onConfirm} color="error" variant="contained">
          Delete
        </Button>
      </DialogActions>
    </Dialog>
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
      <DialogTitle>Error</DialogTitle>
      <DialogContent dividers>
        <Typography variant="body2">{message}</Typography>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} autoFocus>
          OK
        </Button>
      </DialogActions>
    </Dialog>
  );
}

export default TransactionsPage;
