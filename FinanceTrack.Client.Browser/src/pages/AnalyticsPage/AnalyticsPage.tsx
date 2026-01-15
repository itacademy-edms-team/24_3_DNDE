import { useCallback, useEffect, useMemo, useState } from 'react';
import ArrowBackIosNewIcon from '@mui/icons-material/ArrowBackIosNew';
import ArrowForwardIosIcon from '@mui/icons-material/ArrowForwardIos';
import {
  Box,
  Button,
  CircularProgress,
  MenuItem,
  Paper,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import { PieChart, Pie, Cell, ResponsiveContainer, Legend, Tooltip } from 'recharts';
import { fetchIncomes, fetchExpensesByIncome } from './api';
import type { ExpenseTransaction, IncomeTransaction } from './api';

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

const getYearMonth = (dateStr: string): { year: number; month: number } => {
  const [yearRaw, monthRaw] = dateStr.split('-');
  return {
    year: Number(yearRaw) || 0,
    month: Number(monthRaw) || 0,
  };
};

const isInPeriod = (
  transactionDate: string,
  isMonthly: boolean,
  selectedYear: number,
  selectedMonth: number
): boolean => {
  const { year, month } = getYearMonth(transactionDate);
  if (!year || !month) return false;
  if (isMonthly) {
    return year < selectedYear || (year === selectedYear && month <= selectedMonth);
  }
  return year === selectedYear && month === selectedMonth;
};

const formatMoney = (value: number): string => `${value.toFixed(2)} ₽`;

// Цвета для секторов графика
const COLORS = [
  '#0088FE',
  '#00C49F',
  '#FFBB28',
  '#FF8042',
  '#8884D8',
  '#82CA9D',
  '#FFC658',
  '#FF7C7C',
  '#8DD1E1',
  '#D084D0',
  '#FFB347',
  '#87CEEB',
];

type ExpenseGroup = {
  name: string;
  value: number;
  count: number;
};

function AnalyticsPage() {
  const now = useMemo(() => new Date(), []);
  const [selectedYear, setSelectedYear] = useState<number>(now.getFullYear());
  const [selectedMonth, setSelectedMonth] = useState<number>(now.getMonth() + 1);
  const [incomes, setIncomes] = useState<IncomeTransaction[]>([]);
  const [expensesByIncome, setExpensesByIncome] = useState<Record<string, ExpenseTransaction[]>>({});
  const [loadingIncomes, setLoadingIncomes] = useState(false);
  const [loadingExpenses, setLoadingExpenses] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadExpenses = useCallback(async (incomeId: string) => {
    try {
      const data = await fetchExpensesByIncome(incomeId);
      setExpensesByIncome(prev => {
        if (prev[incomeId]) {
          return prev; // Уже загружены
        }
        return {
          ...prev,
          [incomeId]: data,
        };
      });
    } catch (e) {
      const err = e as Error;
      setError(err.message ?? 'Ошибка загрузки расходов');
    }
  }, []);

  const loadIncomes = useCallback(async () => {
    setLoadingIncomes(true);
    setError(null);
    try {
      const data = await fetchIncomes();
      setIncomes(data);
      
      // Загружаем расходы для всех доходов
      setLoadingExpenses(true);
      try {
        await Promise.all(data.map(income => loadExpenses(income.id)));
      } catch (e) {
        const err = e as Error;
        setError(err.message ?? 'Ошибка загрузки расходов');
      } finally {
        setLoadingExpenses(false);
      }
    } catch (e) {
      const err = e as Error;
      setError(err.message ?? 'Ошибка загрузки доходов');
    } finally {
      setLoadingIncomes(false);
    }
  }, [loadExpenses]);

  useEffect(() => {
    void loadIncomes();
  }, [loadIncomes]);

  // Собираем все расходы из всех доходов
  const allExpenses = useMemo(() => {
    return Object.values(expensesByIncome).flat();
  }, [expensesByIncome]);

  // Фильтруем расходы по выбранному периоду
  const filteredExpenses = useMemo(() => {
    return allExpenses.filter(e => isInPeriod(e.operationDate, e.isMonthly, selectedYear, selectedMonth));
  }, [allExpenses, selectedYear, selectedMonth]);

  // Группируем расходы по названиям
  const expenseGroups = useMemo<ExpenseGroup[]>(() => {
    const groups = new Map<string, number>();

    filteredExpenses.forEach(expense => {
      const current = groups.get(expense.name) || 0;
      groups.set(expense.name, current + expense.amount);
    });

    return Array.from(groups.entries())
      .map(([name, value]) => ({
        name,
        value: Number(value.toFixed(2)),
        count: filteredExpenses.filter(e => e.name === name).length,
      }))
      .sort((a, b) => b.value - a.value);
  }, [filteredExpenses]);

  const totalAmount = useMemo(
    () => expenseGroups.reduce((sum, group) => sum + group.value, 0),
    [expenseGroups]
  );

  const availableYears = useMemo(() => {
    const set = new Set<number>([now.getFullYear(), selectedYear]);
    incomes.forEach(i => {
      const { year } = getYearMonth(i.operationDate);
      if (year) set.add(year);
    });
    allExpenses.forEach(e => {
      const { year } = getYearMonth(e.operationDate);
      if (year) set.add(year);
    });
    return Array.from(set).sort((a, b) => b - a);
  }, [incomes, allExpenses, now, selectedYear]);

  const shiftMonth = (delta: number) => {
    setSelectedMonth(prev => {
      let nextMonth = prev + delta;
      let nextYear = selectedYear;
      if (nextMonth > 12) {
        nextMonth = 1;
        nextYear += 1;
      } else if (nextMonth < 1) {
        nextMonth = 12;
        nextYear -= 1;
      }
      setSelectedYear(nextYear);
      return nextMonth;
    });
  };

  const CustomTooltip = ({ active, payload }: any) => {
    if (active && payload && payload.length) {
      const data = payload[0];
      return (
        <Paper sx={{ p: 1.5, boxShadow: 3 }}>
          <Typography variant="body2" fontWeight={600}>
            {data.name}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Сумма: {formatMoney(data.value)}
          </Typography>
          <Typography variant="caption" color="text.secondary">
            Количество: {data.payload.count}
          </Typography>
          <Typography variant="caption" color="text.secondary">
            Доля: {((data.value / totalAmount) * 100).toFixed(1)}%
          </Typography>
        </Paper>
      );
    }
    return null;
  };

  const CustomLabel = ({ cx, cy, midAngle, innerRadius, outerRadius, percent, name }: any) => {
    const RADIAN = Math.PI / 180;
    const radius = innerRadius + (outerRadius - innerRadius) * 0.5;
    const x = cx + radius * Math.cos(-midAngle * RADIAN);
    const y = cy + radius * Math.sin(-midAngle * RADIAN);

    // Показываем метку только если доля больше 5%
    if (percent < 0.05) return null;

    return (
      <text
        x={x}
        y={y}
        fill="white"
        textAnchor={x > cx ? 'start' : 'end'}
        dominantBaseline="central"
        fontSize={12}
        fontWeight={600}
      >
        {name.length > 15 ? `${name.substring(0, 15)}...` : name}
      </text>
    );
  };

  return (
    <>
      <meta name="title" content="Аналитика" />
      <Box sx={{ maxWidth: 1200, mx: 'auto', px: { xs: 2, sm: 3 }, py: 3, display: 'flex', flexDirection: 'column', gap: 3 }}>
        <Stack direction="row" justifyContent="center" alignItems="center">
          <Typography variant="h4">Аналитика расходов</Typography>
        </Stack>

        <Stack direction="row" spacing={1} alignItems="center" justifyContent="center" flexWrap="wrap" rowGap={1.5}>
          <Button
            variant="outlined"
            size="small"
            onClick={() => shiftMonth(-1)}
            aria-label="Предыдущий месяц"
          >
            <ArrowBackIosNewIcon fontSize="small" />
          </Button>
          <TextField
            select
            size="small"
            label="Месяц"
            value={selectedMonth}
            onChange={e => setSelectedMonth(Number(e.target.value))}
            sx={{ minWidth: { xs: 140, sm: 180 } }}
          >
            {MONTH_OPTIONS.map(m => (
              <MenuItem key={m.value} value={m.value}>
                {m.label}
              </MenuItem>
            ))}
          </TextField>
          <TextField
            select
            size="small"
            label="Год"
            value={selectedYear}
            onChange={e => setSelectedYear(Number(e.target.value))}
            sx={{ minWidth: { xs: 110, sm: 140 } }}
          >
            {availableYears.map(y => (
              <MenuItem key={y} value={y}>
                {y}
              </MenuItem>
            ))}
          </TextField>
          <Button
            variant="outlined"
            size="small"
            onClick={() => shiftMonth(1)}
            aria-label="Следующий месяц"
          >
            <ArrowForwardIosIcon fontSize="small" />
          </Button>
        </Stack>

        {(loadingIncomes || loadingExpenses) ? (
          <Stack alignItems="center" justifyContent="center" sx={{ py: 5 }}>
            <CircularProgress size={40} />
            <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
              {loadingIncomes ? 'Загрузка доходов...' : 'Загрузка расходов...'}
            </Typography>
          </Stack>
        ) : error ? (
          <Paper variant="outlined" sx={{ p: 3 }}>
            <Typography variant="body1" color="error" align="center">
              {error}
            </Typography>
          </Paper>
        ) : expenseGroups.length === 0 ? (
          <Paper variant="outlined" sx={{ p: 3 }}>
            <Typography variant="body1" color="text.secondary" align="center">
              Нет расходов за выбранный период
            </Typography>
          </Paper>
        ) : (
          <Paper variant="outlined" sx={{ p: 3 }}>
            <Stack spacing={3}>
              <Stack direction="row" justifyContent="space-between" alignItems="center">
                <Typography variant="h6">Распределение расходов</Typography>
                <Typography variant="body1" fontWeight={600}>
                  Всего: {formatMoney(totalAmount)}
                </Typography>
              </Stack>

              <Box sx={{ width: '100%', height: 500 }}>
                <ResponsiveContainer width="100%" height="100%">
                  <PieChart>
                    <Pie
                      data={expenseGroups}
                      cx="50%"
                      cy="50%"
                      labelLine={false}
                      label={CustomLabel}
                      outerRadius={180}
                      fill="#8884d8"
                      dataKey="value"
                    >
                      {expenseGroups.map((_, index) => (
                        <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                      ))}
                    </Pie>
                    <Tooltip content={<CustomTooltip />} />
                    <Legend
                      formatter={(value) => {
                        const group = expenseGroups.find(g => g.name === value);
                        if (!group) return value;
                        const percent = ((group.value / totalAmount) * 100).toFixed(1);
                        return `${value} (${percent}%)`;
                      }}
                    />
                  </PieChart>
                </ResponsiveContainer>
              </Box>

              <Box>
                <Typography variant="subtitle2" gutterBottom>
                  Детализация по расходам:
                </Typography>
                <Stack spacing={1} sx={{ mt: 1 }}>
                  {expenseGroups.map((group, index) => {
                    const percent = ((group.value / totalAmount) * 100).toFixed(1);
                    return (
                      <Stack
                        key={group.name}
                        direction="row"
                        justifyContent="space-between"
                        alignItems="center"
                        sx={{
                          p: 1.5,
                          borderRadius: 1,
                          bgcolor: 'action.hover',
                        }}
                      >
                        <Stack direction="row" spacing={1} alignItems="center">
                          <Box
                            sx={{
                              width: 16,
                              height: 16,
                              borderRadius: '50%',
                              bgcolor: COLORS[index % COLORS.length],
                            }}
                          />
                          <Typography variant="body2">{group.name}</Typography>
                        </Stack>
                        <Stack direction="row" spacing={2} alignItems="center">
                          <Typography variant="body2" color="text.secondary">
                            {group.count} {group.count === 1 ? 'транзакция' : group.count < 5 ? 'транзакции' : 'транзакций'}
                          </Typography>
                          <Typography variant="body2" fontWeight={600}>
                            {formatMoney(group.value)}
                          </Typography>
                          <Typography variant="body2" color="text.secondary">
                            ({percent}%)
                          </Typography>
                        </Stack>
                      </Stack>
                    );
                  })}
                </Stack>
              </Box>
            </Stack>
          </Paper>
        )}
      </Box>
    </>
  );
}

export default AnalyticsPage;
