import { useState, useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  Box,
  Card,
  CardContent,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  Typography,
  Grid2,
  Paper,
} from '@mui/material';
import {
  BarChart,
  Bar,
  PieChart,
  Pie,
  Cell,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts';

import Loading from '@/components/Loading';

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

type OverviewResponse = {
  totalBalance: number;
  totalIncome: number;
  totalExpense: number;
  netFlow: number;
  accounts: Array<{
    walletId: string;
    walletName: string;
    walletType: string;
    balance: number;
    income: number;
    expense: number;
  }>;
};

type CashFlowPeriod = {
  year: number;
  month: number;
  income: number;
  expense: number;
  net: number;
};

type CashFlowResponse = {
  periods: CashFlowPeriod[];
};

type CategoryAnalytics = {
  categoryId: string | null;
  categoryName: string | null;
  amount: number;
  percentage: number;
};

type CategoriesAnalyticsResponse = {
  incomeByCategory: CategoryAnalytics[];
  expenseByCategory: CategoryAnalytics[];
};

const fetchOverview = async (from: string, to: string): Promise<OverviewResponse> => {
  const res = await fetch(`/api/finance/Analytics/Overview?from=${from}&to=${to}`, {
    credentials: 'include',
  });
  if (!res.ok) {
    throw new Error(getErrorMessage('загрузки обзора', res.status));
  }
  return await res.json();
};

const fetchCashFlow = async (from: string, to: string): Promise<CashFlowResponse> => {
  const res = await fetch(`/api/finance/Analytics/CashFlow?from=${from}&to=${to}`, {
    credentials: 'include',
  });
  if (!res.ok) {
    throw new Error(getErrorMessage('загрузки денежного потока', res.status));
  }
  return await res.json();
};

const fetchCategoriesAnalytics = async (from: string, to: string): Promise<CategoriesAnalyticsResponse> => {
  const res = await fetch(`/api/finance/Analytics/Categories?from=${from}&to=${to}`, {
    credentials: 'include',
  });
  if (!res.ok) {
    throw new Error(getErrorMessage('загрузки аналитики по категориям', res.status));
  }
  return await res.json();
};

const formatMoney = (value: number): string => {
  return new Intl.NumberFormat('ru-RU', {
    style: 'currency',
    currency: 'RUB',
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(value);
};

// Цвета для Pie Chart
const COLORS = [
  '#0088FE',
  '#00C49F',
  '#FFBB28',
  '#FF8042',
  '#8884d8',
  '#82ca9d',
  '#ffc658',
  '#ff7c7c',
  '#8dd1e1',
  '#d084d0',
  '#ffb347',
  '#87ceeb',
];

function GeneralAnalyticsPage() {
  const now = new Date();
  const [filterStartYear, setFilterStartYear] = useState<number>(now.getFullYear());
  const [filterStartMonth, setFilterStartMonth] = useState<number>(now.getMonth() + 1);
  const [filterEndYear, setFilterEndYear] = useState<number>(now.getFullYear());
  const [filterEndMonth, setFilterEndMonth] = useState<number>(now.getMonth() + 1);

  // Формируем даты для фильтрации
  const filterFrom = `${filterStartYear}-${String(filterStartMonth).padStart(2, '0')}-01`;
  const filterTo = `${filterEndYear}-${String(filterEndMonth).padStart(2, '0')}-${new Date(filterEndYear, filterEndMonth, 0).getDate()}`;

  const { data: overview, isLoading: isLoadingOverview } = useQuery({
    queryKey: ['analytics', 'overview', filterFrom, filterTo],
    queryFn: () => fetchOverview(filterFrom, filterTo),
    retry: false,
  });

  const { data: cashFlow, isLoading: isLoadingCashFlow } = useQuery({
    queryKey: ['analytics', 'cashflow', filterFrom, filterTo],
    queryFn: () => fetchCashFlow(filterFrom, filterTo),
    retry: false,
  });

  const { data: categoriesAnalytics, isLoading: isLoadingCategories } = useQuery({
    queryKey: ['analytics', 'categories', filterFrom, filterTo],
    queryFn: () => fetchCategoriesAnalytics(filterFrom, filterTo),
    retry: false,
  });

  // Форматируем данные для Bar Chart (overlapping bars - оба положительные, наложение)
  const cashFlowChartData = useMemo(() => {
    if (!cashFlow?.periods) return [];
    return cashFlow.periods.map((period) => {
      const income = period.income;
      const expense = period.expense;
      // Определяем, какой больше, чтобы правильно наложить
      const max = Math.max(income, expense);
      const min = Math.min(income, expense);
      const isIncomeBigger = income >= expense;
      
      return {
        name: `${MONTH_OPTIONS[period.month - 1]?.label || period.month} ${period.year}`,
        income: income,
        expense: expense,
        // Для наложения: больший сзади, меньший спереди
        bigger: max,
        smaller: min,
        biggerLabel: isIncomeBigger ? 'Доходы' : 'Расходы',
        smallerLabel: isIncomeBigger ? 'Расходы' : 'Доходы',
        biggerColor: isIncomeBigger ? '#4caf50' : '#f44336',
        smallerColor: isIncomeBigger ? '#f44336' : '#4caf50',
      };
    });
  }, [cashFlow]);

  // Форматируем данные для Pie Chart доходов
  const incomePieData = useMemo(() => {
    if (!categoriesAnalytics?.incomeByCategory) return [];
    return categoriesAnalytics.incomeByCategory.map((item) => ({
      name: item.categoryName || 'Без категории',
      value: item.amount,
      percentage: item.percentage,
    }));
  }, [categoriesAnalytics]);

  // Форматируем данные для Pie Chart расходов
  const expensePieData = useMemo(() => {
    if (!categoriesAnalytics?.expenseByCategory) return [];
    return categoriesAnalytics.expenseByCategory.map((item) => ({
      name: item.categoryName || 'Без категории',
      value: item.amount,
      percentage: item.percentage,
    }));
  }, [categoriesAnalytics]);

  // Генерируем список доступных годов
  const availableYears = useMemo(() => {
    const years: number[] = [];
    for (let year = now.getFullYear(); year >= now.getFullYear() - 5; year--) {
      years.push(year);
    }
    return years;
  }, [now]);

  const isLoading = isLoadingOverview || isLoadingCashFlow || isLoadingCategories;

  if (isLoading) {
    return <Loading />;
  }

  // Кастомный Tooltip для Pie Chart
  const CustomPieTooltip = ({ active, payload }: any) => {
    if (active && payload && payload.length) {
      const data = payload[0];
      return (
        <Paper sx={{ p: 1.5, bgcolor: 'background.paper', boxShadow: 2 }}>
          <Typography variant="body2" fontWeight="bold">
            {data.name}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Сумма: {formatMoney(data.value)}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Доля: {data.payload.percentage.toFixed(1)}%
          </Typography>
        </Paper>
      );
    }
    return null;
  };

  // Кастомный Label для Pie Chart
  const renderCustomLabel = (entry: any) => {
    return `${entry.percentage.toFixed(1)}%`;
  };

  return (
    <Box sx={{ p: 3 }}>
      <meta name="title" content="Аналитика" />
      <Typography variant="h4" sx={{ mb: 3 }}>
        Аналитика
      </Typography>

      {/* Фильтры по датам */}
      <Card variant="outlined" sx={{ mb: 3 }}>
        <CardContent>
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
        </CardContent>
      </Card>

      {/* Общий баланс */}
      {overview && (
        <Grid2 container spacing={3} sx={{ mb: 3 }}>
          <Grid2 size={{ xs: 12, sm: 6, md: 3 }}>
            <Card variant="outlined">
              <CardContent>
                <Typography variant="body2" color="text.secondary" gutterBottom>
                  Общий баланс
                </Typography>
                <Typography variant="h4" color={overview.totalBalance >= 0 ? 'success.main' : 'error.main'}>
                  {formatMoney(overview.totalBalance)}
                </Typography>
              </CardContent>
            </Card>
          </Grid2>
          <Grid2 size={{ xs: 12, sm: 6, md: 3 }}>
            <Card variant="outlined">
              <CardContent>
                <Typography variant="body2" color="text.secondary" gutterBottom>
                  Общий доход
                </Typography>
                <Typography variant="h4" color="success.main">
                  {formatMoney(overview.totalIncome)}
                </Typography>
              </CardContent>
            </Card>
          </Grid2>
          <Grid2 size={{ xs: 12, sm: 6, md: 3 }}>
            <Card variant="outlined">
              <CardContent>
                <Typography variant="body2" color="text.secondary" gutterBottom>
                  Общий расход
                </Typography>
                <Typography variant="h4" color="error.main">
                  {formatMoney(overview.totalExpense)}
                </Typography>
              </CardContent>
            </Card>
          </Grid2>
          <Grid2 size={{ xs: 12, sm: 6, md: 3 }}>
            <Card variant="outlined">
              <CardContent>
                <Typography variant="body2" color="text.secondary" gutterBottom>
                  Чистый поток
                </Typography>
                <Typography variant="h4" color={overview.netFlow >= 0 ? 'success.main' : 'error.main'}>
                  {formatMoney(overview.netFlow)}
                </Typography>
              </CardContent>
            </Card>
          </Grid2>
        </Grid2>
      )}

      {/* График доходов/расходов */}
      {cashFlowChartData.length > 0 && (
        <Card variant="outlined" sx={{ mb: 3 }}>
          <CardContent>
            <Typography variant="h6" sx={{ mb: 2 }}>
              Денежный поток по месяцам
            </Typography>
            <ResponsiveContainer width="100%" height={400}>
              <BarChart data={cashFlowChartData} barCategoryGap="30%" barGap={-50}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="name" angle={-45} textAnchor="end" height={100} />
                <YAxis tickFormatter={(value) => formatMoney(value ?? 0)} />
                <Tooltip 
                  content={({ active, payload }) => {
                    if (active && payload && payload.length > 0) {
                      const data = payload[0].payload;
                      return (
                        <Paper sx={{ p: 1.5, bgcolor: 'background.paper', boxShadow: 2 }}>
                          <Typography variant="body2" fontWeight="bold" sx={{ mb: 1 }}>
                            {data.name}
                          </Typography>
                          {payload.map((entry, index) => {
                            const isBigger = entry.dataKey === 'bigger';
                            const label = isBigger ? data.biggerLabel : data.smallerLabel;
                            const color = isBigger ? data.biggerColor : data.smallerColor;
                            return (
                              <Typography key={index} variant="body2" sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                <Box
                                  sx={{
                                    width: 12,
                                    height: 12,
                                    borderRadius: '50%',
                                    bgcolor: color,
                                  }}
                                />
                                {label}: {formatMoney(entry.value as number ?? 0)}
                              </Typography>
                            );
                          })}
                        </Paper>
                      );
                    }
                    return null;
                  }}
                />
                <Legend 
                  formatter={() => null}
                />
                {/* Сначала рисуем больший столбец (сзади) */}
                <Bar 
                  dataKey="bigger" 
                  fill="#888888"
                >
                  {cashFlowChartData.map((entry, index) => (
                    <Cell key={`cell-bigger-${index}`} fill={entry.biggerColor} />
                  ))}
                </Bar>
                {/* Затем меньший столбец (спереди, поверх большего) */}
                <Bar 
                  dataKey="smaller" 
                  fill="#888888"
                >
                  {cashFlowChartData.map((entry, index) => (
                    <Cell key={`cell-smaller-${index}`} fill={entry.smallerColor} />
                  ))}
                </Bar>
              </BarChart>
            </ResponsiveContainer>
            {/* Кастомная легенда */}
            <Box sx={{ display: 'flex', justifyContent: 'center', gap: 3, mt: 2 }}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <Box
                  sx={{
                    width: 16,
                    height: 16,
                    borderRadius: '4px',
                    bgcolor: '#4caf50',
                    opacity: 0.8,
                  }}
                />
                <Typography variant="body2">Доходы</Typography>
              </Box>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <Box
                  sx={{
                    width: 16,
                    height: 16,
                    borderRadius: '4px',
                    bgcolor: '#f44336',
                    opacity: 0.8,
                  }}
                />
                <Typography variant="body2">Расходы</Typography>
              </Box>
            </Box>
          </CardContent>
        </Card>
      )}

      {/* Pie Charts для категорий */}
      <Grid2 container spacing={3}>
        {/* Pie Chart доходов */}
        {incomePieData.length > 0 && (
          <Grid2 size={{ xs: 12, md: 6 }}>
            <Card variant="outlined">
              <CardContent>
                <Typography variant="h6" sx={{ mb: 2 }}>
                  Доходы по категориям
                </Typography>
                <ResponsiveContainer width="100%" height={400}>
                  <PieChart>
                    <Pie
                      data={incomePieData}
                      cx="50%"
                      cy="50%"
                      labelLine={false}
                      label={renderCustomLabel}
                      outerRadius={120}
                      fill="#8884d8"
                      dataKey="value"
                    >
                      {incomePieData.map((_, index) => (
                        <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                      ))}
                    </Pie>
                    <Tooltip content={<CustomPieTooltip />} />
                    <Legend
                      formatter={(value, entry: any) => `${value} (${formatMoney(entry.payload.value)})`}
                    />
                  </PieChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>
          </Grid2>
        )}

        {/* Pie Chart расходов */}
        {expensePieData.length > 0 && (
          <Grid2 size={{ xs: 12, md: 6 }}>
            <Card variant="outlined">
              <CardContent>
                <Typography variant="h6" sx={{ mb: 2 }}>
                  Расходы по категориям
                </Typography>
                <ResponsiveContainer width="100%" height={400}>
                  <PieChart>
                    <Pie
                      data={expensePieData}
                      cx="50%"
                      cy="50%"
                      labelLine={false}
                      label={renderCustomLabel}
                      outerRadius={120}
                      fill="#8884d8"
                      dataKey="value"
                    >
                      {expensePieData.map((_, index) => (
                        <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                      ))}
                    </Pie>
                    <Tooltip content={<CustomPieTooltip />} />
                    <Legend
                      formatter={(value, entry: any) => `${value} (${formatMoney(entry.payload.value)})`}
                    />
                  </PieChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>
          </Grid2>
        )}

        {/* Сообщения, если нет данных */}
        {incomePieData.length === 0 && expensePieData.length === 0 && (
          <Grid2 size={12}>
            <Card variant="outlined">
              <CardContent>
                <Typography variant="body2" color="text.secondary" sx={{ textAlign: 'center', py: 3 }}>
                  Нет данных по категориям за выбранный период
                </Typography>
              </CardContent>
            </Card>
          </Grid2>
        )}
      </Grid2>
    </Box>
  );
}

export default GeneralAnalyticsPage;
