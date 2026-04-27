import { useState, useEffect, useRef, useMemo } from 'react';
import { useNavigate } from 'react-router';
import { useInfiniteQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  FormControlLabel,
  InputAdornment,
  InputLabel,
  LinearProgress,
  MenuItem,
  Select,
  Stack,
  Switch,
  TextField,
  Typography,
} from '@mui/material';
import Grid2 from '@mui/material/Grid2';
import AddIcon from '@mui/icons-material/Add';
import AccountBalanceWalletIcon from '@mui/icons-material/AccountBalanceWallet';
import SavingsIcon from '@mui/icons-material/Savings';

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

type WalletsPage = {
  wallets: Wallet[];
  nextCursor: string | null;
  hasMore: boolean;
};

type CreateWalletPayload = {
  name: string;
  walletType: 'Checking' | 'Savings';
  allowNegativeBalance?: boolean;
  targetAmount?: number;
  targetDate?: string;
};

type CreateWalletResponse = {
  id: string;
};

const PAGE_SIZE = 30;

const fetchWallets = async ({ pageParam }: { pageParam: string | null }): Promise<WalletsPage> => {
  const params = new URLSearchParams();
  params.append('pageSize', String(PAGE_SIZE));
  if (pageParam) params.append('afterCursor', pageParam);

  const res = await fetch(`/api/finance/Wallets?${params.toString()}`, {
    credentials: 'include',
  });
  if (!res.ok) throw new Error(`HTTP error! status: ${res.status}`);
  return await res.json();
};

const createWallet = async (payload: CreateWalletPayload): Promise<CreateWalletResponse> => {
  const res = await fetch('/api/finance/Wallets', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include',
    body: JSON.stringify(payload),
  });

  if (!res.ok) {
    const errorText = await res.text();
    throw new Error(errorText || `HTTP error! status: ${res.status}`);
  }

  return await res.json();
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

type WalletFormState = {
  name: string;
  walletType: 'Checking' | 'Savings';
  allowNegativeBalance: boolean;
  targetAmount: string;
  targetDate: string;
};

function WalletsPage() {
  const queryClient = useQueryClient();
  const sentinelRef = useRef<HTMLDivElement>(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [formState, setFormState] = useState<WalletFormState>({
    name: '',
    walletType: 'Checking',
    allowNegativeBalance: true,
    targetAmount: '',
    targetDate: '',
  });

  const {
    data: walletsData,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
    isLoading,
    error,
  } = useInfiniteQuery({
    queryKey: ['wallets'],
    queryFn: fetchWallets,
    initialPageParam: null as string | null,
    getNextPageParam: (lastPage) => (lastPage.hasMore ? lastPage.nextCursor : undefined),
    retry: false,
  });

  const wallets = useMemo(() => {
    const seen = new Set<string>();
    return (walletsData?.pages ?? [])
      .flatMap((p) => p.wallets)
      .filter((w) => {
        if (seen.has(w.id)) return false;
        seen.add(w.id);
        return true;
      });
  }, [walletsData]);

  useEffect(() => {
    const sentinel = sentinelRef.current;
    if (!sentinel) return;

    const observer = new IntersectionObserver(
      (entries) => {
        if (entries[0].isIntersecting && hasNextPage && !isFetchingNextPage) {
          fetchNextPage();
        }
      },
      { threshold: 0.1 },
    );

    observer.observe(sentinel);
    return () => observer.disconnect();
  }, [hasNextPage, isFetchingNextPage, fetchNextPage]);

  const createWalletMutation = useMutation({
    mutationFn: createWallet,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['wallets'] });
      setDialogOpen(false);
      resetForm();
    },
    onError: (error: Error) => {
      console.error('Failed to create wallet:', error);
      alert(`Ошибка создания кошелька: ${error.message}`);
    },
  });

  const resetForm = () => {
    setFormState({
      name: '',
      walletType: 'Checking',
      allowNegativeBalance: true,
      targetAmount: '',
      targetDate: '',
    });
  };

  const handleOpenDialog = () => {
    resetForm();
    setDialogOpen(true);
  };

  const handleCloseDialog = () => {
    setDialogOpen(false);
    resetForm();
  };

  const handleSubmit = () => {
    if (!formState.name.trim()) {
      alert('Введите название кошелька');
      return;
    }

    if (formState.walletType === 'Savings') {
      const targetAmount = parseFloat(formState.targetAmount);
      if (!formState.targetAmount || isNaN(targetAmount) || targetAmount <= 0) {
        alert('Для накопительного кошелька необходимо указать целевую сумму больше 0');
        return;
      }
    }

    const payload: CreateWalletPayload = {
      name: formState.name.trim(),
      walletType: formState.walletType,
    };

    if (formState.walletType === 'Checking') {
      payload.allowNegativeBalance = formState.allowNegativeBalance;
    } else {
      payload.targetAmount = parseFloat(formState.targetAmount);
      if (formState.targetDate) {
        payload.targetDate = formState.targetDate;
      }
    }

    createWalletMutation.mutate(payload);
  };

  const isSavings = formState.walletType === 'Savings';

  if (isLoading) {
    return <Loading />;
  }

  if (error) {
    return (
      <Box sx={{ p: 3 }}>
        <Typography color="error">
          Ошибка загрузки кошельков. Попробуйте обновить страницу.
        </Typography>
      </Box>
    );
  }

  return (
    <Box sx={{ p: 3 }}>
      <meta name="title" content="Кошельки" />
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4">Кошельки</Typography>
        <Button variant="contained" startIcon={<AddIcon />} onClick={handleOpenDialog}>
          Создать кошелёк
        </Button>
      </Box>

      {wallets.length === 0 && (
        <Box sx={{ textAlign: 'center', py: 4 }}>
          <Typography variant="h5" gutterBottom>
            У вас пока нет кошельков
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            Создайте первый кошелёк, чтобы начать отслеживать финансы
          </Typography>
        </Box>
      )}

      {wallets.length > 0 && (
        <Grid2 container spacing={2}>
          {wallets.map((wallet) => (
            <Grid2 size={{ xs: 12, sm: 6, md: 4 }} key={wallet.id}>
              <WalletCard wallet={wallet} />
            </Grid2>
          ))}
        </Grid2>
      )}

      <div ref={sentinelRef} />

      {isFetchingNextPage && (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 2 }}>
          <CircularProgress size={24} />
        </Box>
      )}

      <Dialog open={dialogOpen} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle>Создать новый кошелёк</DialogTitle>
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

            <FormControl fullWidth required>
              <InputLabel>Тип кошелька</InputLabel>
              <Select
                value={formState.walletType}
                label="Тип кошелька"
                onChange={(e) => {
                  const newType = e.target.value as 'Checking' | 'Savings';
                  setFormState({
                    ...formState,
                    walletType: newType,
                    allowNegativeBalance: newType === 'Checking',
                    targetAmount: newType === 'Savings' ? formState.targetAmount : '',
                    targetDate: newType === 'Savings' ? formState.targetDate : '',
                  });
                }}
              >
                <MenuItem value="Checking">Расчётный</MenuItem>
                <MenuItem value="Savings">Накопительный</MenuItem>
              </Select>
            </FormControl>

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
                  slotProps={{
                    htmlInput: { min: 0.01, step: 0.01 },
                    input: { endAdornment: <InputAdornment position="end">₽</InputAdornment> },
                  }}
                />

                <TextField
                  label="Целевая дата"
                  type="date"
                  value={formState.targetDate}
                  onChange={(e) => setFormState({ ...formState, targetDate: e.target.value })}
                  fullWidth
                  slotProps={{
                    inputLabel: { shrink: true },
                    htmlInput: { min: new Date().toISOString().split('T')[0] },
                  }}
                />
              </>
            )}
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Отмена</Button>
          <Button
            onClick={handleSubmit}
            variant="contained"
            disabled={createWalletMutation.isPending}
          >
            {createWalletMutation.isPending ? 'Создание...' : 'Создать'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

type WalletCardProps = {
  wallet: Wallet;
};

function WalletCard({ wallet }: WalletCardProps) {
  const navigate = useNavigate();
  const isSavings = wallet.walletType === 'Savings';
  const progressPercent =
    wallet.targetAmount && wallet.targetAmount > 0
      ? Math.min((wallet.balance / wallet.targetAmount) * 100, 100)
      : 0;

  return (
    <Card
      variant="outlined"
      onClick={() => navigate(`/wallets/${wallet.id}`)}
      sx={{
        height: '100%',
        display: 'flex',
        flexDirection: 'column',
        cursor: 'pointer',
        '&:hover': { boxShadow: 3 },
        transition: 'box-shadow 0.2s',
      }}
    >
      <CardContent sx={{ flexGrow: 1 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
          {isSavings ? (
            <SavingsIcon sx={{ mr: 1, color: 'primary.main' }} />
          ) : (
            <AccountBalanceWalletIcon sx={{ mr: 1, color: 'primary.main' }} />
          )}
          <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
            {wallet.name}
          </Typography>
        </Box>

        <Chip
          label={isSavings ? 'Накопительный' : 'Расчётный'}
          size="small"
          color={isSavings ? 'primary' : 'default'}
          sx={{ mb: 2 }}
        />

        <Typography
          variant="h4"
          gutterBottom
          color={wallet.balance >= 0 ? 'success.main' : 'error.main'}
        >
          {formatMoney(wallet.balance)}
        </Typography>

        {isSavings && wallet.targetAmount && (
          <Box sx={{ mt: 2 }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
              <Typography variant="body2" color="text.secondary">
                Цель: {formatMoney(wallet.targetAmount)}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {progressPercent.toFixed(1)}%
              </Typography>
            </Box>
            <LinearProgress
              variant="determinate"
              value={progressPercent}
              sx={{ height: 8, borderRadius: 1 }}
            />
            {wallet.targetDate && (
              <Typography variant="caption" color="text.secondary" sx={{ mt: 0.5, display: 'block' }}>
                До: {formatDate(wallet.targetDate)}
              </Typography>
            )}
          </Box>
        )}

        {!wallet.allowNegativeBalance && (
          <Chip
            label="Отрицательный баланс запрещён"
            size="small"
            variant="outlined"
            sx={{ mt: 1 }}
          />
        )}
      </CardContent>
    </Card>
  );
}

export default WalletsPage;
