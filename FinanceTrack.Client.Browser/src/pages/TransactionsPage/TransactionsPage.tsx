import { useCallback, useEffect, useMemo, useState } from 'react';
import {
  Box,
  Button,
  Card,
  CardActionArea,
  CardContent,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControlLabel,
  Stack,
  Switch,
  TextField,
  Typography,
  Paper,
  Divider,
} from '@mui/material';

type IncomeTransactionRecord = {
  id: string;
  amount: number;
  operationDate: string; // "YYYY-MM-DD"
  isMonthly: boolean;
  type: string; // "Income"
};

type ListIncomeTransactionsByUserIdResponse = {
  transactions: IncomeTransactionRecord[];
};

type AddIncomeFormState = {
  amount: string;
  operationDate: string;
  isMonthly: boolean;
};

type EditIncomeFormState = {
  operationDate: string;
  isMonthly: boolean;
};

function TransactionsPage() {
  const [transactions, setTransactions] = useState<IncomeTransactionRecord[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [isAddDialogOpen, setIsAddDialogOpen] = useState(false);
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false);

  const [addForm, setAddForm] = useState<AddIncomeFormState>({
    amount: '',
    operationDate: '',
    isMonthly: false,
  });

  const [editForm, setEditForm] = useState<EditIncomeFormState>({
    operationDate: '',
    isMonthly: false,
  });

  const [selectedId, setSelectedId] = useState<string | null>(null);

  const selectedTransaction = useMemo(
    () => transactions.find(t => t.id === selectedId) ?? null,
    [transactions, selectedId]
  );

  const loadIncomes = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      const response = await fetch('/api/finance/transactions/income', {
        method: 'GET',
        credentials: 'include',
      });

      if (!response.ok) {
        throw new Error(`Ошибка загрузки: ${response.status}`);
      }

      const data = (await response.json()) as ListIncomeTransactionsByUserIdResponse;

      if (!data || !Array.isArray(data.transactions)) {
        throw new Error('Некорректный формат ответа сервера');
      }

      setTransactions(data.transactions);
    } catch (e) {
      const err = e as Error;
      setError(err.message ?? 'Неизвестная ошибка');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadIncomes();
  }, [loadIncomes]);

  const handleOpenAddDialog = () => {
    setAddForm({
      amount: '',
      operationDate: '',
      isMonthly: false,
    });
    setIsAddDialogOpen(true);
  };

  const handleCloseAddDialog = () => {
    setIsAddDialogOpen(false);
  };

  const handleOpenEditDialog = () => {
    if (!selectedTransaction) return;

    setEditForm({
      operationDate: selectedTransaction.operationDate,
      isMonthly: selectedTransaction.isMonthly,
    });

    setIsEditDialogOpen(true);
  };

  const handleCloseEditDialog = () => {
    setIsEditDialogOpen(false);
  };

  const handleAddIncome = async () => {
    const amount = Number(addForm.amount.replace(',', '.'));

    if (Number.isNaN(amount) || amount <= 0) {
      alert('Неверная сумма');
      return;
    }

    if (!/^\d{4}-\d{2}-\d{2}$/.test(addForm.operationDate)) {
      alert('Дата должна быть в формате ГГГГ-ММ-ДД');
      return;
    }

    try {
      const response = await fetch('/api/finance/transactions/income', {
        method: 'POST',
        credentials: 'include',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          amount,
          operationDate: addForm.operationDate,
          isMonthly: addForm.isMonthly,
        }),
      });

      if (!response.ok) {
        throw new Error(`Ошибка создания: ${response.status}`);
      }

      await loadIncomes();
      setIsAddDialogOpen(false);
    } catch (e) {
      const err = e as Error;
      alert(err.message ?? 'Ошибка при создании дохода');
    }
  };

  const handleSaveEdit = async () => {
    if (!selectedTransaction) return;

    if (!/^\d{4}-\d{2}-\d{2}$/.test(editForm.operationDate)) {
      alert('Дата должна быть в формате ГГГГ-ММ-ДД');
      return;
    }

    try {
      const response = await fetch(
        `/api/finance/transactions/income/${encodeURIComponent(selectedTransaction.id)}`,
        {
          method: 'PUT',
          credentials: 'include',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({
            amount: selectedTransaction.amount, // пока не редактируем сумму
            operationDate: editForm.operationDate,
            isMonthly: editForm.isMonthly,
          }),
        }
      );

      if (!response.ok) {
        throw new Error(`Ошибка обновления: ${response.status}`);
      }

      await loadIncomes();
      setIsEditDialogOpen(false);
    } catch (e) {
      const err = e as Error;
      alert(err.message ?? 'Ошибка при обновлении дохода');
    }
  };

  const handleSelectTransaction = (id: string) => {
    setSelectedId(prev => (prev === id ? null : id));
  };

  const formatOperationDate = (dateStr: string): string => {
    if (!dateStr) return '';
    // ожидаем "YYYY-MM-DD"
    const [year, month, day] = dateStr.split('-');
    if (!year || !month || !day) return dateStr;
    return `${day}.${month}.${year}`;
  };

  return (
    <Box sx={{ p: 3, display: 'flex', flexDirection: 'column', gap: 2 }}>
      <Typography variant="h3" gutterBottom>
        Доходы
      </Typography>

      <Stack direction="row" spacing={2} alignItems="center">
        <Button variant="contained" onClick={handleOpenAddDialog}>
          Добавить доход
        </Button>
        <Button variant="outlined" onClick={() => void loadIncomes()}>
          Обновить список
        </Button>
        {loading && (
          <Typography variant="body2" color="text.secondary">
            Загрузка...
          </Typography>
        )}
      </Stack>

      {error && (
        <Typography color="error" variant="body2">
          {error}
        </Typography>
      )}

      {/* Лента доходов */}
      <Paper
        elevation={2}
        sx={{
          p: 2,
          maxHeight: 360,
          overflowY: 'auto',
          display: 'flex',
          flexDirection: 'column',
          gap: 1.5,
        }}
      >
        {transactions.length === 0 && !loading && (
          <Typography variant="body2" color="text.secondary">
            Доходов пока нет. Нажми &quot;Добавить доход&quot;, чтобы создать первый.
          </Typography>
        )}

        {transactions.map(tx => {
          const isSelected = tx.id === selectedId;
          return (
            <Card
              key={tx.id}
              variant={isSelected ? 'elevation' : 'outlined'}
              sx={{
                transition: 'all 0.15s ease',
                borderColor: isSelected ? 'primary.main' : 'divider',
                bgcolor: isSelected ? 'action.hover' : 'background.paper',
              }}
            >
              <CardActionArea onClick={() => handleSelectTransaction(tx.id)}>
                <CardContent sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Box>
                    <Typography variant="subtitle1">
                      Доход {tx.id.slice(0, 8)}…
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Дата: {formatOperationDate(tx.operationDate)}
                    </Typography>
                  </Box>
                  <Box sx={{ textAlign: 'right' }}>
                    <Typography variant="subtitle1">
                      {tx.amount.toFixed(2)} ₽
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {tx.isMonthly ? 'Ежемесячный' : 'Разовый'}
                    </Typography>
                  </Box>
                </CardContent>
              </CardActionArea>
            </Card>
          );
        })}
      </Paper>

      {/* Карточка под лентой с деталями выбранного дохода */}
      {selectedTransaction && (
        <Box>
          <Typography variant="h6" gutterBottom>
            Детали дохода
          </Typography>
          <Card
            variant="outlined"
            sx={{
              maxWidth: 420,
              cursor: 'pointer',
              '&:hover': { boxShadow: 4 },
            }}
            onClick={handleOpenEditDialog}
          >
            <CardContent>
              <Typography variant="subtitle1" gutterBottom>
                Доход {selectedTransaction.id.slice(0, 8)}…
              </Typography>
              <Typography variant="body2">
                Сумма: {selectedTransaction.amount.toFixed(2)} ₽
              </Typography>
              <Typography variant="body2">
                Дата поступления: {formatOperationDate(selectedTransaction.operationDate)}
              </Typography>
              <Typography variant="body2">
                Тип: {selectedTransaction.isMonthly ? 'Ежемесячный' : 'Разовый'}
              </Typography>
              <Typography variant="caption" color="text.secondary">
                Нажми, чтобы изменить дату и ежемесячность
              </Typography>
            </CardContent>
          </Card>
        </Box>
      )}

      {/* Диалог добавления дохода */}
      <Dialog open={isAddDialogOpen} onClose={handleCloseAddDialog} maxWidth="xs" fullWidth>
        <DialogTitle>Добавить доход</DialogTitle>
        <DialogContent dividers>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField
              label="Сумма"
              type="number"
              fullWidth
              value={addForm.amount}
              onChange={e => setAddForm(prev => ({ ...prev, amount: e.target.value }))}
            />
            <TextField
              label="Дата операции"
              type="date"
              fullWidth
              InputLabelProps={{ shrink: true }}
              value={addForm.operationDate}
              onChange={e => setAddForm(prev => ({ ...prev, operationDate: e.target.value }))}
            />
            <FormControlLabel
              control={
                <Switch
                  checked={addForm.isMonthly}
                  onChange={e =>
                    setAddForm(prev => ({ ...prev, isMonthly: e.target.checked }))
                  }
                />
              }
              label="Ежемесячный доход"
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseAddDialog}>Назад</Button>
          <Button onClick={handleAddIncome} variant="contained">
            Добавить
          </Button>
        </DialogActions>
      </Dialog>

      {/* Диалог редактирования дохода */}
      <Dialog open={isEditDialogOpen} onClose={handleCloseEditDialog} maxWidth="xs" fullWidth>
        <DialogTitle>Изменить доход</DialogTitle>
        <DialogContent dividers>
          {selectedTransaction ? (
            <Stack spacing={2} sx={{ mt: 1 }}>
              <Box>
                <Typography variant="subtitle2" color="text.secondary">
                  Доход
                </Typography>
                <Typography variant="body2">
                  {selectedTransaction.id}
                </Typography>
                <Typography variant="body2">
                  Сумма: {selectedTransaction.amount.toFixed(2)} ₽
                </Typography>
              </Box>
              <Divider />
              <TextField
                label="Дата операции"
                type="date"
                fullWidth
                InputLabelProps={{ shrink: true }}
                value={editForm.operationDate}
                onChange={e =>
                  setEditForm(prev => ({ ...prev, operationDate: e.target.value }))
                }
              />
              <FormControlLabel
                control={
                  <Switch
                    checked={editForm.isMonthly}
                    onChange={e =>
                      setEditForm(prev => ({ ...prev, isMonthly: e.target.checked }))
                    }
                  />
                }
                label="Ежемесячный доход"
              />
            </Stack>
          ) : (
            <Typography>Доход не выбран</Typography>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseEditDialog}>Назад</Button>
          <Button
            onClick={handleSaveEdit}
            variant="contained"
            disabled={!selectedTransaction}
          >
            Сохранить
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default TransactionsPage;
