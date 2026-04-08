import { useState } from 'react';
import { useNavigate } from 'react-router';
import { useMutation, useQuery } from '@tanstack/react-query';
import {
  Alert,
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  Typography,
} from '@mui/material';

import Loading from '@/components/Loading';
import BankStatementTypeSelect, {
  BankStatementType,
} from '@/components/BankStatementTypeSelect';
import PdfFileUpload from '@/components/PdfFileUpload';

type Wallet = {
  id: string;
  name: string;
  isArchived: boolean;
};

type WalletsResponse = {
  wallets: Wallet[];
};

const fetchWallets = async (): Promise<Wallet[]> => {
  const res = await fetch('/api/finance/Wallets', { credentials: 'include' });
  if (!res.ok) throw new Error(`HTTP error! status: ${res.status}`);
  const data: WalletsResponse = await res.json();
  return data.wallets.filter((w) => !w.isArchived);
};

type ImportPayload = {
  walletId: string;
  bankStatementType: BankStatementType;
  pdfFile: File;
};

const importToExistingWallet = async (payload: ImportPayload): Promise<void> => {
  const formData = new FormData();
  formData.append('WalletId', payload.walletId);
  formData.append('BankStatementType', payload.bankStatementType);
  formData.append('PdfFile', payload.pdfFile);

  const res = await fetch('/api/finance/Transactions/Import/Pdf/ExistingWallet', {
    method: 'POST',
    credentials: 'include',
    body: formData,
  });

  if (!res.ok) {
    const errorText = await res.text();
    throw new Error(errorText || `HTTP error! status: ${res.status}`);
  }
};

type FormErrors = {
  walletId?: string;
  bankStatementType?: string;
  pdfFile?: string;
};

function ImportPdfExistingWalletPage() {
  const navigate = useNavigate();
  const [walletId, setWalletId] = useState('');
  const [bankStatementType, setBankStatementType] = useState<BankStatementType | ''>('');
  const [pdfFile, setPdfFile] = useState<File | null>(null);
  const [errors, setErrors] = useState<FormErrors>({});
  const [successDialogOpen, setSuccessDialogOpen] = useState(false);

  const { data: wallets, isLoading, error: walletsError } = useQuery({
    queryKey: ['wallets'],
    queryFn: fetchWallets,
    retry: false,
  });

  const importMutation = useMutation({
    mutationFn: importToExistingWallet,
    onSuccess: () => {
      setSuccessDialogOpen(true);
    },
  });

  const validate = (): boolean => {
    const newErrors: FormErrors = {};
    if (!walletId) newErrors.walletId = 'Выберите кошелёк';
    if (!bankStatementType) newErrors.bankStatementType = 'Выберите тип выписки';
    if (!pdfFile) newErrors.pdfFile = 'Выберите PDF-файл';
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = () => {
    if (!validate()) return;
    importMutation.mutate({
      walletId,
      bankStatementType: bankStatementType as BankStatementType,
      pdfFile: pdfFile!,
    });
  };

  const handleSuccessOk = () => {
    setSuccessDialogOpen(false);
    setWalletId('');
    setBankStatementType('');
    setPdfFile(null);
    setErrors({});
    importMutation.reset();
  };

  const handleGoToWallet = () => {
    navigate(`/wallets/${walletId}`);
  };

  if (isLoading) return <Loading />;

  if (walletsError) {
    return (
      <Box sx={{ p: 3 }}>
        <Typography color="error">Ошибка загрузки кошельков. Попробуйте обновить страницу.</Typography>
      </Box>
    );
  }

  return (
    <Box sx={{ p: 3, maxWidth: 600 }}>
      <Typography variant="h4" gutterBottom>
        Импорт в существующий кошелёк
      </Typography>

      <Stack spacing={3} sx={{ mt: 2 }}>
        <FormControl fullWidth required error={!!errors.walletId}>
          <InputLabel>Кошелёк</InputLabel>
          <Select
            value={walletId}
            label="Кошелёк"
            onChange={(e) => {
              setWalletId(e.target.value);
              setErrors((prev) => ({ ...prev, walletId: undefined }));
            }}
          >
            {wallets?.map((wallet) => (
              <MenuItem key={wallet.id} value={wallet.id}>
                {wallet.name}
              </MenuItem>
            ))}
          </Select>
          {errors.walletId && (
            <Typography variant="caption" color="error" sx={{ mt: 0.5, ml: 1.75 }}>
              {errors.walletId}
            </Typography>
          )}
        </FormControl>

        <BankStatementTypeSelect
          value={bankStatementType}
          onChange={(v) => {
            setBankStatementType(v);
            setErrors((prev) => ({ ...prev, bankStatementType: undefined }));
          }}
          error={errors.bankStatementType}
        />

        <Alert severity="info">
          В данный момент поддерживается импорт только из текстовых PDF (не сканированных).
        </Alert>

        <PdfFileUpload
          value={pdfFile}
          onChange={(f) => {
            setPdfFile(f);
            setErrors((prev) => ({ ...prev, pdfFile: undefined }));
          }}
          error={errors.pdfFile}
        />

        {importMutation.isError && (
          <Alert severity="error">
            Ошибка импорта транзакций. Проверьте формат выбранной выписки.
          </Alert>
        )}

        <Button
          variant="contained"
          size="large"
          onClick={handleSubmit}
          disabled={importMutation.isPending}
        >
          {importMutation.isPending ? 'Импорт...' : 'Импортировать'}
        </Button>
      </Stack>

      <Dialog open={successDialogOpen} onClose={handleSuccessOk}>
        <DialogTitle>Импорт выполнен</DialogTitle>
        <DialogContent>
          <Typography>Транзакции успешно импортированы в кошелёк.</Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleSuccessOk}>Ок</Button>
          <Button variant="contained" onClick={handleGoToWallet}>
            Перейти к кошельку
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default ImportPdfExistingWalletPage;
