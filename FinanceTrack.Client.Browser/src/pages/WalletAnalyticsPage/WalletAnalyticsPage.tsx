import { useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router';
import { useQuery } from '@tanstack/react-query';
import {
  Box,
  Button,
  Card,
  CardContent,
  FormControl,
  Grid2,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  Typography,
  useTheme,
} from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import {
  ComposedChart,
  Bar,
  Line,
  Cell,
  Pie,
  PieChart,
  Sector,
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

type WalletOverviewResponse = {
  walletId: string;
  walletName: string;
  balance: number;
  income: number;
  expense: number;
  netFlow: number;
};

type WalletCashFlowPeriod = {
  year: number;
  month: number;
  income: number;
  expense: number;
  net: number;
};

type WalletCashFlowResponse = {
  periods: WalletCashFlowPeriod[];
};

type WalletCategoryAnalytics = {
  categoryId: string | null;
  categoryName: string | null;
  amount: number;
  percentage: number;
};

type WalletCategoriesAnalyticsResponse = {
  incomeByCategory: WalletCategoryAnalytics[];
  expenseByCategory: WalletCategoryAnalytics[];
};

type DateMinMaxResponse = {
  minDate: string | null;
  maxDate: string | null;
};

const fetchWalletOverview = async (
  walletId: string,
  from: string,
  to: string
): Promise<WalletOverviewResponse> => {
  const res = await fetch(`/api/finance/Analytics/Wallets/${walletId}/Overview?From=${from}&To=${to}`, {
    credentials: 'include',
  });
  if (!res.ok) {
    throw new Error(getErrorMessage('загрузки обзора аналитики кошелька', res.status));
  }
  return await res.json();
};

const fetchWalletCashFlow = async (
  walletId: string,
  from: string,
  to: string
): Promise<WalletCashFlowResponse> => {
  const res = await fetch(`/api/finance/Analytics/Wallets/${walletId}/CashFlow?From=${from}&To=${to}`, {
    credentials: 'include',
  });
  if (!res.ok) {
    throw new Error(getErrorMessage('загрузки денежного потока кошелька', res.status));
  }
  return await res.json();
};

const fetchWalletCategoriesAnalytics = async (
  walletId: string,
  from: string,
  to: string
): Promise<WalletCategoriesAnalyticsResponse> => {
  const res = await fetch(`/api/finance/Analytics/Wallets/${walletId}/Categories?From=${from}&To=${to}`, {
    credentials: 'include',
  });
  if (!res.ok) {
    throw new Error(getErrorMessage('загрузки аналитики кошелька по категориям', res.status));
  }
  return await res.json();
};

const fetchWalletDateMinMax = async (walletId: string): Promise<DateMinMaxResponse> => {
  const res = await fetch(`/api/finance/Analytics/Wallets/${walletId}/Meta/DateMinMax`, {
    credentials: 'include',
  });
  if (!res.ok) {
    throw new Error(getErrorMessage('загрузки границ дат аналитики кошелька', res.status));
  }
  return await res.json();
};

const getYearFromDateString = (dateString: string | null): number | null => {
  if (!dateString) return null;
  const parsed = new Date(dateString);
  if (Number.isNaN(parsed.getTime())) return null;
  return parsed.getFullYear();
};

const formatMoney = (value: number): string => {
  return new Intl.NumberFormat('ru-RU', {
    style: 'currency',
    currency: 'RUB',
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(value);
};

function WalletAnalyticsPage() {
  const navigate = useNavigate();
  const { walletId } = useParams<{ walletId: string }>();
  const theme = useTheme();
  const now = new Date();
  const [filterStartYear, setFilterStartYear] = useState<number>(now.getFullYear());
  const [filterStartMonth, setFilterStartMonth] = useState<number>(now.getMonth() + 1);
  const [filterEndYear, setFilterEndYear] = useState<number>(now.getFullYear());
  const [filterEndMonth, setFilterEndMonth] = useState<number>(now.getMonth() + 1);

  const filterFrom = `${filterStartYear}-${String(filterStartMonth).padStart(2, '0')}-01`;
  const filterTo = `${filterEndYear}-${String(filterEndMonth).padStart(2, '0')}-${new Date(filterEndYear, filterEndMonth, 0).getDate()}`;

  const {
    data: overview,
    isLoading: isLoadingOverview,
    error: overviewError,
  } = useQuery({
    queryKey: ['wallet-analytics', walletId, 'overview', filterFrom, filterTo],
    queryFn: () => fetchWalletOverview(walletId!, filterFrom, filterTo),
    enabled: !!walletId,
    retry: false,
  });

  const {
    data: cashFlow,
    isLoading: isLoadingCashFlow,
    error: cashFlowError,
  } = useQuery({
    queryKey: ['wallet-analytics', walletId, 'cashflow', filterFrom, filterTo],
    queryFn: () => fetchWalletCashFlow(walletId!, filterFrom, filterTo),
    enabled: !!walletId,
    retry: false,
  });

  const {
    data: categoriesAnalytics,
    isLoading: isLoadingCategories,
    error: categoriesError,
  } = useQuery({
    queryKey: ['wallet-analytics', walletId, 'categories', filterFrom, filterTo],
    queryFn: () => fetchWalletCategoriesAnalytics(walletId!, filterFrom, filterTo),
    enabled: !!walletId,
    retry: false,
  });

  const { data: dateMinMax } = useQuery({
    queryKey: ['wallet-analytics', walletId, 'meta', 'date-min-max'],
    queryFn: () => fetchWalletDateMinMax(walletId!),
    enabled: !!walletId,
    retry: false,
  });

  const availableYears = useMemo(() => {
    const years: number[] = [];
    const minMetaYear = getYearFromDateString(dateMinMax?.minDate ?? null);
    const maxMetaYear = getYearFromDateString(dateMinMax?.maxDate ?? null);
    const minDataYear = minMetaYear ?? now.getFullYear() - 5;
    const maxDataYear = maxMetaYear ?? now.getFullYear();
    const minYear = Math.min(minDataYear, filterStartYear, filterEndYear);
    const maxYear = Math.max(maxDataYear, filterStartYear, filterEndYear);

    for (let year = maxYear; year >= minYear; year--) {
      years.push(year);
    }
    return years;
  }, [now, dateMinMax, filterStartYear, filterEndYear]);

  const cashFlowChartData = useMemo(() => {
    if (!cashFlow?.periods) return [];
    return cashFlow.periods.map((period) => {
      const income = period.income;
      const expense = period.expense;
      const max = Math.max(income, expense);
      const min = Math.min(income, expense);
      const isIncomeBigger = income >= expense;

      return {
        name: `${MONTH_OPTIONS[period.month - 1]?.label || period.month} ${period.year}`,
        net: period.net,
        bigger: max,
        smaller: min,
        biggerLabel: isIncomeBigger ? 'Доходы' : 'Расходы',
        smallerLabel: isIncomeBigger ? 'Расходы' : 'Доходы',
        biggerColor: isIncomeBigger ? '#4caf50' : '#f44336',
        smallerColor: isIncomeBigger ? '#f44336' : '#4caf50',
      };
    });
  }, [cashFlow]);

  const incomePieData = useMemo(() => {
    if (!categoriesAnalytics?.incomeByCategory) return [];
    const mapped = categoriesAnalytics.incomeByCategory.map((item) => ({
      name: item.categoryName || 'Без категории',
      value: item.amount,
      percentage: item.percentage,
    }));
    const major = mapped.filter((d) => d.percentage >= 3);
    const minor = mapped.filter((d) => d.percentage < 3);
    if (minor.length >= 2) {
      major.push({
        name: 'Прочее',
        value: minor.reduce((s, d) => s + d.value, 0),
        percentage: minor.reduce((s, d) => s + d.percentage, 0),
      });
      return major;
    }
    return mapped;
  }, [categoriesAnalytics]);

  const expensePieData = useMemo(() => {
    if (!categoriesAnalytics?.expenseByCategory) return [];
    const mapped = categoriesAnalytics.expenseByCategory.map((item) => ({
      name: item.categoryName || 'Без категории',
      value: item.amount,
      percentage: item.percentage,
    }));
    const major = mapped.filter((d) => d.percentage >= 3);
    const minor = mapped.filter((d) => d.percentage < 3);
    if (minor.length >= 2) {
      major.push({
        name: 'Прочее',
        value: minor.reduce((s, d) => s + d.value, 0),
        percentage: minor.reduce((s, d) => s + d.percentage, 0),
      });
      return major;
    }
    return mapped;
  }, [categoriesAnalytics]);

  const [activeIncomeIndex, setActiveIncomeIndex] = useState<number | undefined>(undefined);
  const [activeExpenseIndex, setActiveExpenseIndex] = useState<number | undefined>(undefined);

  const isLoading = isLoadingOverview || isLoadingCashFlow || isLoadingCategories;
  const error = overviewError || cashFlowError || categoriesError;

  if (!walletId) {
    return (
      <Box sx={{ p: 3 }}>
        <Typography color="error">Не удалось определить кошелёк для аналитики.</Typography>
      </Box>
    );
  }

  if (isLoading) {
    return <Loading />;
  }

  const CustomPieTooltip = ({ active, payload }: any) => {
    if (active && payload && payload.length) {
      const data = payload[0];
      const color = data.payload.fill || COLORS[0];
      return (
        <Paper sx={{ p: 1.5, bgcolor: 'background.paper', boxShadow: 2 }}>
          <Typography
            component="div"
            variant="body2"
            fontWeight="bold"
            sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}
          >
            <Box sx={{ width: 12, height: 12, borderRadius: '50%', bgcolor: color }} />
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

  const renderActiveShape = (props: any) => {
    const { cx, cy, innerRadius, outerRadius, startAngle, endAngle, fill } = props;
    return (
      <g>
        <Sector
          cx={cx}
          cy={cy}
          innerRadius={innerRadius - 4}
          outerRadius={outerRadius + 10}
          startAngle={startAngle}
          endAngle={endAngle}
          fill={fill}
        />
      </g>
    );
  };

  const renderCustomLabel = (entry: any) => {
    return `${entry.percentage.toFixed(1)}%`;
  };

  return (
    <Box sx={{ p: 3 }}>
      <meta name="title" content={overview?.walletName || 'Аналитика кошелька'} />

      <Box sx={{ display: 'flex', alignItems: 'center', mb: 3, gap: 2 }}>
        <Button startIcon={<ArrowBackIcon />} onClick={() => navigate(`/wallets/${walletId}`)}>
          Назад
        </Button>
        <Typography variant="h4" sx={{ flexGrow: 1 }}>
          {overview?.walletName ? `Аналитика: ${overview.walletName}` : 'Аналитика кошелька'}
        </Typography>
      </Box>

      {error && (
        <Typography color="error" sx={{ mb: 2 }}>
          {(error as Error).message}
        </Typography>
      )}

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

      {overview && (
        <Grid2 container spacing={3} sx={{ mb: 3 }}>
          <Grid2 size={{ xs: 12, sm: 6, md: 3 }}>
            <Card variant="outlined">
              <CardContent>
                <Typography variant="body2" color="text.secondary" gutterBottom>
                  Баланс
                </Typography>
                <Typography variant="h4" color={overview.balance >= 0 ? 'success.main' : 'error.main'}>
                  {formatMoney(overview.balance)}
                </Typography>
              </CardContent>
            </Card>
          </Grid2>
          <Grid2 size={{ xs: 12, sm: 6, md: 3 }}>
            <Card variant="outlined">
              <CardContent>
                <Typography variant="body2" color="text.secondary" gutterBottom>
                  Доход
                </Typography>
                <Typography variant="h4" color="success.main">
                  {formatMoney(overview.income)}
                </Typography>
              </CardContent>
            </Card>
          </Grid2>
          <Grid2 size={{ xs: 12, sm: 6, md: 3 }}>
            <Card variant="outlined">
              <CardContent>
                <Typography variant="body2" color="text.secondary" gutterBottom>
                  Расход
                </Typography>
                <Typography variant="h4" color="error.main">
                  {formatMoney(overview.expense)}
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

      {cashFlowChartData.length > 0 && (
        <Card variant="outlined" sx={{ mb: 3 }}>
          <CardContent>
            <Typography variant="h6" sx={{ mb: 2 }}>
              Денежный поток по месяцам
            </Typography>
            <ResponsiveContainer width="100%" height={400}>
              <ComposedChart data={cashFlowChartData} barCategoryGap="30%" barGap={-60}>
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
                          {payload
                            .filter((entry) => entry.dataKey !== 'net')
                            .map((entry, index) => {
                              const isBigger = entry.dataKey === 'bigger';
                              const label = isBigger ? data.biggerLabel : data.smallerLabel;
                              const color = isBigger ? data.biggerColor : data.smallerColor;
                              return (
                                <Typography
                                  key={index}
                                  component="div"
                                  variant="body2"
                                  sx={{ display: 'flex', alignItems: 'center', gap: 1 }}
                                >
                                  <Box
                                    sx={{
                                      width: 12,
                                      height: 12,
                                      borderRadius: '50%',
                                      bgcolor: color,
                                    }}
                                  />
                                  {label}: {formatMoney((entry.value as number) ?? 0)}
                                </Typography>
                              );
                            })}
                          <Typography
                            component="div"
                            variant="body2"
                            sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 0.5 }}
                          >
                            <Box
                              sx={{
                                width: 12,
                                height: 12,
                                borderRadius: '50%',
                                bgcolor: '#2196f3',
                              }}
                            />
                            Чистый поток: {formatMoney(data.net ?? 0)}
                          </Typography>
                        </Paper>
                      );
                    }
                    return null;
                  }}
                />
                <Bar dataKey="bigger" fill="#888888" radius={[5, 5, 0, 0]}>
                  {cashFlowChartData.map((entry, index) => (
                    <Cell key={`wallet-cell-bigger-${index}`} fill={entry.biggerColor} />
                  ))}
                </Bar>
                <Bar dataKey="smaller" fill="#888888" radius={[5, 5, 0, 0]}>
                  {cashFlowChartData.map((entry, index) => (
                    <Cell key={`wallet-cell-smaller-${index}`} fill={entry.smallerColor} />
                  ))}
                </Bar>
                <Line
                  type="monotone"
                  dataKey="net"
                  stroke="#2196f3"
                  strokeWidth={2}
                  dot={{ r: 4, fill: '#2196f3' }}
                  activeDot={{ r: 6 }}
                />
              </ComposedChart>
            </ResponsiveContainer>
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
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <Box
                  sx={{
                    width: 16,
                    height: 3,
                    bgcolor: '#2196f3',
                  }}
                />
                <Typography variant="body2">Чистый поток</Typography>
              </Box>
            </Box>
          </CardContent>
        </Card>
      )}

      <Grid2 container spacing={3}>
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
                      stroke="none"
                      cx="50%"
                      cy="50%"
                      labelLine={false}
                      label={renderCustomLabel}
                      innerRadius={70}
                      outerRadius={120}
                      fill="#8884d8"
                      dataKey="value"
                      onMouseEnter={(_, index) => setActiveIncomeIndex(index)}
                      onMouseLeave={() => setActiveIncomeIndex(undefined)}
                      {...{ activeIndex: activeIncomeIndex, activeShape: renderActiveShape }}
                    >
                      {incomePieData.map((_, index) => (
                        <Cell key={`wallet-income-cell-${index}`} fill={COLORS[index % COLORS.length]} />
                      ))}
                    </Pie>
                    <text x="50%" y="50%" textAnchor="middle" dominantBaseline="central" fontSize={16} fontWeight="bold" fill={theme.palette.text.primary}>
                      {formatMoney(incomePieData.reduce((s, d) => s + d.value, 0))}
                    </text>
                    <Tooltip content={<CustomPieTooltip />} />
                    <Legend formatter={(value, entry: any) => `${value} (${formatMoney(entry.payload.value)})`} />
                  </PieChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>
          </Grid2>
        )}

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
                      stroke="none"
                      cx="50%"
                      cy="50%"
                      labelLine={false}
                      label={renderCustomLabel}
                      innerRadius={70}
                      outerRadius={120}
                      fill="#8884d8"
                      dataKey="value"
                      onMouseEnter={(_, index) => setActiveExpenseIndex(index)}
                      onMouseLeave={() => setActiveExpenseIndex(undefined)}
                      {...{ activeIndex: activeExpenseIndex, activeShape: renderActiveShape }}
                    >
                      {expensePieData.map((_, index) => (
                        <Cell key={`wallet-expense-cell-${index}`} fill={COLORS[index % COLORS.length]} />
                      ))}
                    </Pie>
                    <text x="50%" y="50%" textAnchor="middle" dominantBaseline="central" fontSize={16} fontWeight="bold" fill={theme.palette.text.primary}>
                      {formatMoney(expensePieData.reduce((s, d) => s + d.value, 0))}
                    </text>
                    <Tooltip content={<CustomPieTooltip />} />
                    <Legend formatter={(value, entry: any) => `${value} (${formatMoney(entry.payload.value)})`} />
                  </PieChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>
          </Grid2>
        )}

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

export default WalletAnalyticsPage;