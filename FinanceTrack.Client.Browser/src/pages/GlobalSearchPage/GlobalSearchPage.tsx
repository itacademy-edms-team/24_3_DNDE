import { useState, useMemo, useEffect } from 'react';
import { useNavigate } from 'react-router';
import { useQuery } from '@tanstack/react-query';
import {
  Box,
  Card,
  CardContent,
  TextField,
  Typography,
  Tabs,
  Tab,
  Chip,
  Stack,
  List,
  ListItemButton,
  ListItemText,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
} from '@mui/material';

import Loading from '@/components/Loading';

const getErrorMessage = (operation: string, status: number): string => {
  return `Ошибка ${operation}: ${status}`;
};

const formatMoney = (value: number): string => {
  return new Intl.NumberFormat('ru-RU', {
    style: 'currency',
    currency: 'RUB',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value);
};

type GlobalSearchWallet = {
  id: string;
  name: string;
  walletType: string;
  balance: number;
  allowNegativeBalance: boolean;
  targetAmount: number | null;
  targetDate: string | null;
  isArchived: boolean;
};

type GlobalSearchTransaction = {
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

type GlobalSearchRecurringTransaction = {
  id: string;
  walletId: string;
  categoryId: string | null;
  name: string;
  type: 'Income' | 'Expense';
  amount: number;
  dayOfMonth: number;
  startDate: string;
  endDate: string | null;
  isActive: boolean;
  lastProcessedDate: string | null;
};

type GlobalSearchResponse = {
  wallets: GlobalSearchWallet[];
  incomes: GlobalSearchTransaction[];
  expenses: GlobalSearchTransaction[];
  transfers: GlobalSearchTransaction[];
  recurringTransactions: GlobalSearchRecurringTransaction[];
};

type ResultFilter = 'all' | 'income' | 'expense' | 'transfer' | 'recurring';

const fetchGlobalSearch = async (
  query: string,
  limitPerType: number
): Promise<GlobalSearchResponse> => {
  const params = new URLSearchParams();
  params.append('Query', query);
  params.append('LimitPerType', String(limitPerType));

  const res = await fetch(`/api/finance/GlobalSearch?${params.toString()}`, {
    credentials: 'include',
  });

  if (!res.ok) {
    throw new Error(getErrorMessage('глобального поиска', res.status));
  }

  return await res.json();
};

function GlobalSearchPage() {
  const navigate = useNavigate();

  const [search, setSearch] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [filter, setFilter] = useState<ResultFilter>('all');
  const [limitPerType, setLimitPerType] = useState<number>(10);

  // Дебаунс ввода, чтобы не слать запрос на каждый символ
  useEffect(() => {
    const handle = setTimeout(() => {
      setDebouncedSearch(search.trim());
    }, 400);

    return () => clearTimeout(handle);
  }, [search]);

  const isSearchEnabled = debouncedSearch.length >= 2;

  const { data, isFetching, error } = useQuery({
    queryKey: ['global-search', debouncedSearch, limitPerType],
    queryFn: () => fetchGlobalSearch(debouncedSearch, limitPerType),
    enabled: isSearchEnabled,
    retry: false,
  });

  const hasAnyResults = useMemo(() => {
    if (!data) return false;
    return (
      data.wallets.length > 0 ||
      data.incomes.length > 0 ||
      data.expenses.length > 0 ||
      data.transfers.length > 0 ||
      data.recurringTransactions.length > 0
    );
  }, [data]);

  const handleOpenWallet = (walletId: string) => {
    navigate(`/wallets/${walletId}`);
  };

  const handleFilterChange = (_: React.SyntheticEvent, value: ResultFilter) => {
    if (value) {
      setFilter(value);
    }
  };

  const renderWalletsSection = () => {
    if (!data || data.wallets.length === 0) return null;

    return (
      <Card variant="outlined" sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" sx={{ mb: 1.5 }}>
            Кошельки
          </Typography>
          <List>
            {data.wallets.map((wallet) => (
              <ListItemButton
                key={wallet.id}
                onClick={() => handleOpenWallet(wallet.id)}
              >
                <ListItemText
                  primary={
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <Typography variant="subtitle1">{wallet.name}</Typography>
                      {wallet.isArchived && (
                        <Chip label="Архив" size="small" color="default" />
                      )}
                    </Box>
                  }
                  secondary={
                    <Stack direction="row" spacing={2} sx={{ mt: 0.5 }}>
                      <Typography variant="body2" color="text.secondary">
                        Тип: {wallet.walletType}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        Баланс: {formatMoney(wallet.balance)}
                      </Typography>
                    </Stack>
                  }
                />
              </ListItemButton>
            ))}
          </List>
        </CardContent>
      </Card>
    );
  };

  const renderIncomesSection = () => {
    if (!data || data.incomes.length === 0) return null;
    if (!(filter === 'all' || filter === 'income')) return null;

    return (
      <Card variant="outlined" sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" sx={{ mb: 1.5 }}>
            Доходы
          </Typography>
          <List>
            {data.incomes.map((item) => (
              <ListItemButton
                key={item.id}
                onClick={() => handleOpenWallet(item.walletId)}
              >
                <ListItemText
                  primary={
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <Typography variant="subtitle1">{item.name}</Typography>
                      <Chip label="Доход" size="small" color="success" />
                    </Box>
                  }
                  secondary={
                    <Stack direction="row" spacing={2} sx={{ mt: 0.5 }}>
                      <Typography variant="body2" color="text.secondary">
                        Сумма: {formatMoney(item.amount)}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        Дата: {new Date(item.operationDate).toLocaleDateString('ru-RU')}
                      </Typography>
                    </Stack>
                  }
                  secondaryTypographyProps={{ component: 'div' }}
                />
              </ListItemButton>
            ))}
          </List>
        </CardContent>
      </Card>
    );
  };

  const renderExpensesSection = () => {
    if (!data || data.expenses.length === 0) return null;
    if (!(filter === 'all' || filter === 'expense')) return null;

    return (
      <Card variant="outlined" sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" sx={{ mb: 1.5 }}>
            Расходы
          </Typography>
          <List>
            {data.expenses.map((item) => (
              <ListItemButton
                key={item.id}
                onClick={() => handleOpenWallet(item.walletId)}
              >
                <ListItemText
                  primary={
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <Typography variant="subtitle1">{item.name}</Typography>
                      <Chip label="Расход" size="small" color="error" />
                    </Box>
                  }
                  secondary={
                    <Stack direction="row" spacing={2} sx={{ mt: 0.5 }}>
                      <Typography variant="body2" color="text.secondary">
                        Сумма: {formatMoney(item.amount)}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        Дата: {new Date(item.operationDate).toLocaleDateString('ru-RU')}
                      </Typography>
                    </Stack>
                  }
                  secondaryTypographyProps={{ component: 'div' }}
                />
              </ListItemButton>
            ))}
          </List>
        </CardContent>
      </Card>
    );
  };

  const renderTransfersSection = () => {
    if (!data || data.transfers.length === 0) return null;
    if (!(filter === 'all' || filter === 'transfer')) return null;

    return (
      <Card variant="outlined" sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" sx={{ mb: 1.5 }}>
            Переводы
          </Typography>
          <List>
            {data.transfers.map((item) => (
              <ListItemButton
                key={item.id}
                onClick={() => handleOpenWallet(item.walletId)}
              >
                <ListItemText
                  primary={
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <Typography variant="subtitle1">{item.name}</Typography>
                      <Chip label="Перевод" size="small" color="info" />
                    </Box>
                  }
                  secondary={
                    <Stack direction="row" spacing={2} sx={{ mt: 0.5, flexWrap: 'wrap' }}>
                      <Typography variant="body2" color="text.secondary">
                        Сумма: {formatMoney(item.amount)}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        Дата: {new Date(item.operationDate).toLocaleDateString('ru-RU')}
                      </Typography>
                      {item.relatedWalletName && (
                        <Typography variant="body2" color="text.secondary">
                          Связанный кошелёк: {item.relatedWalletName}
                        </Typography>
                      )}
                    </Stack>
                  }
                  secondaryTypographyProps={{ component: 'div' }}
                />
              </ListItemButton>
            ))}
          </List>
        </CardContent>
      </Card>
    );
  };

  const renderRecurringSection = () => {
    if (!data || data.recurringTransactions.length === 0) return null;
    if (!(filter === 'all' || filter === 'recurring')) return null;

    return (
      <Card variant="outlined" sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" sx={{ mb: 1.5 }}>
            Повторяющиеся транзакции
          </Typography>
          <List>
            {data.recurringTransactions.map((item) => (
              <ListItemButton
                key={item.id}
                onClick={() => handleOpenWallet(item.walletId)}
              >
                <ListItemText
                  primary={
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <Typography variant="subtitle1">{item.name}</Typography>
                      <Chip
                        label={item.type === 'Income' ? 'Доход' : 'Расход'}
                        size="small"
                        color={item.type === 'Income' ? 'success' : 'error'}
                      />
                      {!item.isActive && (
                        <Chip label="Неактивна" size="small" color="default" />
                      )}
                    </Box>
                  }
                  secondary={
                    <Stack direction="row" spacing={2} sx={{ mt: 0.5, flexWrap: 'wrap' }}>
                      <Typography variant="body2" color="text.secondary">
                        Сумма: {formatMoney(item.amount)}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        День месяца: {item.dayOfMonth}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        Начало: {new Date(item.startDate).toLocaleDateString('ru-RU')}
                      </Typography>
                      {item.endDate && (
                        <Typography variant="body2" color="text.secondary">
                          Конец: {new Date(item.endDate).toLocaleDateString('ru-RU')}
                        </Typography>
                      )}
                    </Stack>
                  }
                  secondaryTypographyProps={{ component: 'div' }}
                />
              </ListItemButton>
            ))}
          </List>
        </CardContent>
      </Card>
    );
  };

  return (
    <Box sx={{ p: 3 }}>
      <meta name="title" content="Глобальный поиск" />
      <Typography variant="h4" sx={{ mb: 3 }}>
        Глобальный поиск
      </Typography>

      <Card variant="outlined" sx={{ mb: 3 }}>
        <CardContent>
          <Stack spacing={2}>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <TextField
                label="Поиск"
                placeholder="Например: еда, зарплата, перевод..."
                fullWidth
                value={search}
                onChange={(e) => setSearch(e.target.value)}
              />
              <FormControl size="small" sx={{ minWidth: 160 }}>
                <InputLabel>Показывать первые</InputLabel>
                <Select
                  value={limitPerType}
                  label="Показывать первые"
                  onChange={(e) => setLimitPerType(Number(e.target.value))}
                >
                  <MenuItem value={10}>10</MenuItem>
                  <MenuItem value={30}>30</MenuItem>
                  <MenuItem value={50}>50</MenuItem>
                  <MenuItem value={100}>100</MenuItem>
                </Select>
              </FormControl>
            </Stack>

            <Tabs
              value={filter}
              onChange={handleFilterChange}
              variant="scrollable"
              scrollButtons="auto"
            >
              <Tab label="Все" value="all" />
              <Tab label="Доходы" value="income" />
              <Tab label="Расходы" value="expense" />
              <Tab label="Переводы" value="transfer" />
              <Tab label="Повторяющиеся" value="recurring" />
            </Tabs>
          </Stack>
        </CardContent>
      </Card>

      {error && (
        <Typography color="error" sx={{ mb: 2 }}>
          {(error as Error).message}
        </Typography>
      )}

      {!isSearchEnabled && (
        <Typography variant="body2" color="text.secondary">
          Введите минимум 2 символа для начала поиска.
        </Typography>
      )}

      {isSearchEnabled && isFetching && <Loading />}

      {isSearchEnabled && !isFetching && hasAnyResults && (
        <Box>
          {renderWalletsSection()}
          {renderIncomesSection()}
          {renderExpensesSection()}
          {renderTransfersSection()}
          {renderRecurringSection()}
        </Box>
      )}

      {isSearchEnabled && !isFetching && !hasAnyResults && (
        <Typography variant="body2" color="text.secondary">
          По вашему запросу ничего не найдено.
        </Typography>
      )}
    </Box>
  );
}

export default GlobalSearchPage;