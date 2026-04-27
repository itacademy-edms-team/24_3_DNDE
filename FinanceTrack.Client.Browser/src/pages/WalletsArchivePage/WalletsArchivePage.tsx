import { useEffect, useRef, useMemo } from 'react';
import { useState } from 'react';
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
  LinearProgress,
  Typography,
} from '@mui/material';
import Grid2 from '@mui/material/Grid2';
import AccountBalanceWalletIcon from '@mui/icons-material/AccountBalanceWallet';
import SavingsIcon from '@mui/icons-material/Savings';
import ArchiveIcon from '@mui/icons-material/Archive';
import UnarchiveIcon from '@mui/icons-material/Unarchive';

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

const getErrorMessage = (operation: string, status: number): string => {
  return `Ошибка ${operation}: ${status}`;
};

const PAGE_SIZE = 30;

const fetchArchivedWallets = async ({
  pageParam,
}: {
  pageParam: string | null;
}): Promise<WalletsPage> => {
  const params = new URLSearchParams();
  params.append('pageSize', String(PAGE_SIZE));
  if (pageParam) params.append('afterCursor', pageParam);

  const res = await fetch(`/api/finance/Wallets/archive?${params.toString()}`, {
    credentials: 'include',
  });
  if (!res.ok) throw new Error(getErrorMessage('загрузки архивных кошельков', res.status));
  return await res.json();
};

const unarchiveWallet = async (walletId: string): Promise<void> => {
  const res = await fetch(`/api/finance/Wallets/${walletId}/unarchive`, {
    credentials: 'include',
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: '{}',
  });

  if (!res.ok) throw new Error(getErrorMessage('разархивации кошелька', res.status));
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

function WalletsArchivePage() {
  const queryClient = useQueryClient();
  const sentinelRef = useRef<HTMLDivElement>(null);
  const [unarchiveConfirmOpen, setUnarchiveConfirmOpen] = useState(false);
  const [walletToUnarchive, setWalletToUnarchive] = useState<Wallet | null>(null);
  const [errorDialog, setErrorDialog] = useState({ open: false, message: '' });

  const {
    data: walletsData,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
    isLoading,
    error,
  } = useInfiniteQuery({
    queryKey: ['wallets-archive'],
    queryFn: fetchArchivedWallets,
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

  const unarchiveWalletMutation = useMutation({
    mutationFn: (walletId: string) => unarchiveWallet(walletId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['wallets-archive'] });
      queryClient.invalidateQueries({ queryKey: ['wallets'] });
      setUnarchiveConfirmOpen(false);
      setWalletToUnarchive(null);
    },
    onError: (error: Error) => {
      console.error('Failed to unarchive wallet:', error);
      setErrorDialog({ open: true, message: error.message || 'Ошибка разархивации кошелька' });
    },
  });

  const handleUnarchiveClick = (wallet: Wallet) => {
    setWalletToUnarchive(wallet);
    setUnarchiveConfirmOpen(true);
  };

  const handleUnarchiveConfirm = () => {
    if (walletToUnarchive) {
      unarchiveWalletMutation.mutate(walletToUnarchive.id);
    }
  };

  if (isLoading) {
    return <Loading />;
  }

  if (error) {
    return (
      <Box sx={{ p: 3 }}>
        <Typography color="error">
          Ошибка загрузки архивных кошельков. Попробуйте обновить страницу.
        </Typography>
      </Box>
    );
  }

  return (
    <Box sx={{ p: 3 }}>
      <meta name="title" content="Архивные кошельки" />
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4">Архивные кошельки</Typography>
      </Box>

      {wallets.length === 0 && (
        <Box sx={{ textAlign: 'center', py: 4 }}>
          <Typography variant="h5" gutterBottom>
            Нет архивных кошельков
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Архивированные кошельки будут отображаться здесь
          </Typography>
        </Box>
      )}

      {wallets.length > 0 && (
        <Grid2 container spacing={2}>
          {wallets.map((wallet) => (
            <Grid2 size={{ xs: 12, sm: 6, md: 4 }} key={wallet.id}>
              <WalletCard wallet={wallet} onUnarchive={handleUnarchiveClick} />
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

      <Dialog open={unarchiveConfirmOpen} onClose={() => setUnarchiveConfirmOpen(false)}>
        <DialogTitle>Разархивировать кошелёк</DialogTitle>
        <DialogContent>
          <Typography>
            Вы уверены, что хотите разархивировать кошелёк &quot;{walletToUnarchive?.name}&quot;?
            <br />
            После разархивации кошелёк снова станет доступен для операций.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setUnarchiveConfirmOpen(false)}>Отмена</Button>
          <Button
            onClick={handleUnarchiveConfirm}
            color="primary"
            variant="contained"
            disabled={unarchiveWalletMutation.isPending}
            startIcon={<UnarchiveIcon />}
          >
            {unarchiveWalletMutation.isPending ? 'Разархивация...' : 'Разархивировать'}
          </Button>
        </DialogActions>
      </Dialog>

      <ErrorDialog
        open={errorDialog.open}
        message={errorDialog.message}
        onClose={() => setErrorDialog({ open: false, message: '' })}
      />
    </Box>
  );
}

type WalletCardProps = {
  wallet: Wallet;
  onUnarchive: (wallet: Wallet) => void;
};

function WalletCard({ wallet, onUnarchive }: WalletCardProps) {
  const isSavings = wallet.walletType === 'Savings';
  const progressPercent =
    wallet.targetAmount && wallet.targetAmount > 0
      ? Math.min((wallet.balance / wallet.targetAmount) * 100, 100)
      : 0;

  return (
    <Card
      variant="outlined"
      sx={{
        height: '100%',
        display: 'flex',
        flexDirection: 'column',
        opacity: 0.7,
        position: 'relative',
      }}
    >
      <Chip
        icon={<ArchiveIcon />}
        label="Архив"
        size="small"
        sx={{ position: 'absolute', top: 8, right: 8 }}
      />
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
            {wallet.targetDate && (
              <Typography
                variant="caption"
                color="text.secondary"
                sx={{ mt: 0.5, display: 'block' }}
              >
                До: {formatDate(wallet.targetDate)}
              </Typography>
            )}
            <LinearProgress
              variant="determinate"
              value={progressPercent}
              sx={{ height: 8, borderRadius: 1, mt: 1 }}
            />
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

        <Button
          variant="contained"
          color="primary"
          startIcon={<UnarchiveIcon />}
          onClick={() => onUnarchive(wallet)}
          fullWidth
          sx={{ mt: 2 }}
        >
          Разархивировать
        </Button>
      </CardContent>
    </Card>
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

export default WalletsArchivePage;
