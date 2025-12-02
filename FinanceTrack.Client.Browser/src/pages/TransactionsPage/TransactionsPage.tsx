import { useEffect, useState, useCallback } from 'react';
import {
  Box,
  Button,
  CircularProgress,
  Paper,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material';

// Если ты на TypeScript — можно так
// Если на JS — просто убери типы и оставь JSDoc или вообще без него
type IncomeTransactionRecord = {
  id: string;
  amount: number;
  operationDate: string; // с бэка придёт "2025-02-19"
  isMonthly: boolean;
  type: string; // "Income"
};

function TransactionsPage() {
  const [transactions, setTransactions] = useState<IncomeTransactionRecord[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadIncomes = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      const response = await fetch('/api/finance/transactions/income', {
        method: 'GET',
        credentials: 'include'
      });

      if (!response.ok) {
        throw new Error(`Loading error: ${response.status}`);
      }

      const data = await response.json();
      setTransactions(data.transactions ?? []);
    } catch (err: any) {
      setError(err.message ?? 'Unexpected error');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadIncomes();
  }, [loadIncomes]);

  const handleRefresh = () => {
    loadIncomes();
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm('Delete income? All related expenses will be deleted too.')) {
      return;
    }

    try {
      const response = await fetch(`/api/finance/transactions/income/${id}`, {
        method: 'DELETE',
        credentials: 'include',
      });

      if (response.status === 204) {
        // Успешно — просто убираем из списка
        setTransactions(prev => prev.filter(t => t.id !== id));
      } else if (!response.ok) {
        throw new Error(`Deletion error: ${response.status}`);
      }
    } catch (err: any) {
      alert(err.message ?? 'Error when deleting income');
    }
  };

  const handleCreate = async () => {
    const amountStr = window.prompt('Enter income value (example, 1000.50):');
    if (!amountStr) return;

    const amount = Number(amountStr.replace(',', '.'));
    if (Number.isNaN(amount) || amount <= 0) {
      alert('Invalid value');
      return;
    }

    const dateStr = window.prompt('Enter operation date (YYYY-MM-DD), example 2025-02-19:');
    if (!dateStr) return;

    if (!/^\d{4}-\d{2}-\d{2}$/.test(dateStr)) {
      alert('Invalid date format');
      return;
    }

    const isMonthlyAnswer = window.prompt('Is income monthly? (y/n)', 'n');
    const isMonthly = isMonthlyAnswer?.toLowerCase() === 'y';

    try {
      const response = await fetch('/api/finance/transactions/income', {
        method: 'POST',
        credentials: 'include',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          amount,
          operationDate: dateStr, // backend awaits DateOnly — string "YYYY-MM-DD"
          isMonthly,
        }),
      });

      if (!response.ok) {
        throw new Error(`Creating error: ${response.status}`);
      }

      const created = await response.json();
      // Ожидаем минимум { id: "..." }
      // Чтобы не заморачиваться — просто перезагрузим список
      await loadIncomes();
    } catch (err: any) {
      alert(err.message ?? 'Error when creating income');
    }
  };

  return (
    <Box sx={{ p: 3 }}>
      
      <Typography variant="h3" gutterBottom>
        Incomes
      </Typography>

      <Stack direction="row" spacing={2} sx={{ mb: 2 }}>
        <Button variant="contained" onClick={handleCreate}>
          Create income
        </Button>
        <Button variant="outlined" onClick={handleRefresh}>
          Refresh
        </Button>
      </Stack>

      {loading && (
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
          <CircularProgress size={20} />
          <Typography>Loading...</Typography>
        </Box>
      )}

      {error && (
        <Typography color="error" sx={{ mb: 2 }}>
          {error}
        </Typography>
      )}

      <TableContainer component={Paper}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Id</TableCell>
              <TableCell align="right">Amount</TableCell>
              <TableCell>Operation date</TableCell>
              <TableCell>Monthly</TableCell>
              <TableCell>Type</TableCell>
              <TableCell align="right">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {transactions.map(tx => (
              <TableRow key={tx.id}>
                <TableCell>{tx.id}</TableCell>
                <TableCell align="right">{tx.amount.toFixed(2)}</TableCell>
                <TableCell>{tx.operationDate}</TableCell>
                <TableCell>{tx.isMonthly ? 'Yes' : 'No'}</TableCell>
                <TableCell>{tx.type}</TableCell>
                <TableCell align="right">
                  <Button
                    color="error"
                    size="small"
                    onClick={() => handleDelete(tx.id)}
                  >
                    Delete
                  </Button>
                </TableCell>
              </TableRow>
            ))}

            {transactions.length === 0 && !loading && (
              <TableRow>
                <TableCell colSpan={6}>
                  <Typography align="center" sx={{ py: 2 }}>
                    No incomes to display
                  </Typography>
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
}

export default TransactionsPage;
