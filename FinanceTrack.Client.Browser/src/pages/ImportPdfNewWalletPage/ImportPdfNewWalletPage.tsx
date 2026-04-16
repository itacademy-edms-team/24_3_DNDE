import { useState } from 'react';
import { useNavigate } from 'react-router';
import { useMutation } from '@tanstack/react-query';
import {
  Alert,
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  FormControlLabel,
  InputAdornment,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  Switch,
  TextField,
  Typography,
} from '@mui/material';

import BankStatementTypeSelect, {
  BankStatementType,
} from '@/components/BankStatementTypeSelect';
import PdfFileUpload from '@/components/PdfFileUpload';

type WalletType = 'Checking' | 'Savings';

type ImportNewWalletPayload = {
  bankStatementType: BankStatementType;
  pdfFile: File;
  walletName: string;
  walletType: WalletType;
  allowNegativeBalance?: boolean;
  targetAmount?: number;
  targetDate?: string;
};

type ImportNewWalletResponse = {
  walletId: string;
};

const importToNewWallet = async (
  payload: ImportNewWalletPayload,
): Promise<ImportNewWalletResponse> => {
  const formData = new FormData();
  formData.append('BankStatementType', payload.bankStatementType);
  formData.append('PdfFile', payload.pdfFile);
  formData.append('WalletName', payload.walletName);
  formData.append('WalletType', payload.walletType);

  if (payload.walletType === 'Checking') {
    formData.append('AllowNegativeBalance', String(payload.allowNegativeBalance ?? true));
  }

  if (payload.walletType === 'Savings') {
    if (payload.targetAmount !== undefined) {
      formData.append('TargetAmount', String(payload.targetAmount));
    }
    if (payload.targetDate) {
      formData.append('TargetDate', payload.targetDate);
    }
  }

  const res = await fetch('/api/finance/Transactions/Import/Pdf/NewWallet', {
    method: 'POST',
    credentials: 'include',
    body: formData,
  });

  if (!res.ok) {
    const errorText = await res.text();
    throw new Error(errorText || `HTTP error! status: ${res.status}`);
  }

  return await res.json();
};

type FormState = {
  walletName: string;
  walletType: WalletType;
  allowNegativeBalance: boolean;
  targetAmount: string;
  targetDate: string;
};

type FormErrors = {
  bankStatementType?: string;
  pdfFile?: string;
  walletName?: string;
  walletType?: string;
  targetAmount?: string;
};

function ImportPdfNewWalletPage() {
  const navigate = useNavigate();
  const [bankStatementType, setBankStatementType] = useState<BankStatementType | ''>('');
  const [pdfFile, setPdfFile] = useState<File | null>(null);
  const [formState, setFormState] = useState<FormState>({
    walletName: '',
    walletType: 'Checking',
    allowNegativeBalance: true,
    targetAmount: '',
    targetDate: '',
  });
  const [errors, setErrors] = useState<FormErrors>({});
  const [successDialogOpen, setSuccessDialogOpen] = useState(false);
  const [createdWalletId, setCreatedWalletId] = useState<string | null>(null);

  const importMutation = useMutation({
    mutationFn: importToNewWallet,
    onSuccess: (data) => {
      setCreatedWalletId(data.walletId);
      setSuccessDialogOpen(true);
    },
  });

  const isSavings = formState.walletType === 'Savings';

  const validate = (): boolean => {
    const newErrors: FormErrors = {};
    if (!bankStatementType) newErrors.bankStatementType = 'Выберите тип выписки';
    if (!pdfFile) newErrors.pdfFile = 'Выберите PDF-файл';
    if (!formState.walletName.trim()) newErrors.walletName = 'Введите название кошелька';
    if (isSavings) {
      const amount = parseFloat(formState.targetAmount);
      if (!formState.targetAmount || isNaN(amount) || amount <= 0) {
        newErrors.targetAmount = 'Укажите целевую сумму больше 0';
      }
    }
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = () => {
    if (!validate()) return;

    const payload: ImportNewWalletPayload = {
      bankStatementType: bankStatementType as BankStatementType,
      pdfFile: pdfFile!,
      walletName: formState.walletName.trim(),
      walletType: formState.walletType,
    };

    if (isSavings) {
      payload.targetAmount = parseFloat(formState.targetAmount);
      if (formState.targetDate) payload.targetDate = formState.targetDate;
    } else {
      payload.allowNegativeBalance = formState.allowNegativeBalance;
    }

    importMutation.mutate(payload);
  };

  const handleSuccessOk = () => {
    setSuccessDialogOpen(false);
    setBankStatementType('');
    setPdfFile(null);
    setFormState({
      walletName: '',
      walletType: 'Checking',
      allowNegativeBalance: true,
      targetAmount: '',
      targetDate: '',
    });
    setErrors({});
    setCreatedWalletId(null);
    importMutation.reset();
  };

  const handleGoToWallet = () => {
    if (createdWalletId) navigate(`/wallets/${createdWalletId}`);
  };

  return (
    <Box sx={{ p: 3, maxWidth: 600 }}>
      <Typography variant="h4" gutterBottom>
        Импорт в новый кошелёк
      </Typography>

      <Stack spacing={3} sx={{ mt: 2 }}>
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

        <TextField
          label="Название кошелька"
          value={formState.walletName}
          onChange={(e) => {
            setFormState((s) => ({ ...s, walletName: e.target.value }));
            setErrors((prev) => ({ ...prev, walletName: undefined }));
          }}
          fullWidth
          required
          error={!!errors.walletName}
          helperText={errors.walletName}
        />

        <FormControl fullWidth required>
          <InputLabel>Тип кошелька</InputLabel>
          <Select
            value={formState.walletType}
            label="Тип кошелька"
            onChange={(e) => {
              const newType = e.target.value as WalletType;
              setFormState((s) => ({
                ...s,
                walletType: newType,
                allowNegativeBalance: true,
                targetAmount: '',
                targetDate: '',
              }));
              setErrors((prev) => ({ ...prev, targetAmount: undefined }));
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
                  setFormState((s) => ({ ...s, allowNegativeBalance: e.target.checked }))
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
              onChange={(e) => {
                setFormState((s) => ({ ...s, targetAmount: e.target.value }));
                setErrors((prev) => ({ ...prev, targetAmount: undefined }));
              }}
              fullWidth
              required
              error={!!errors.targetAmount}
              helperText={errors.targetAmount}
              slotProps={{
                input: { endAdornment: <InputAdornment position="end">₽</InputAdornment> },
                htmlInput: { min: 0.01, step: 0.01 },
              }}
            />

            <TextField
              label="Целевая дата"
              type="date"
              value={formState.targetDate}
              onChange={(e) => setFormState((s) => ({ ...s, targetDate: e.target.value }))}
              fullWidth
              slotProps={{
                inputLabel: { shrink: true },
                htmlInput: { min: new Date().toISOString().split('T')[0] },
              }}
            />
          </>
        )}

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
          {importMutation.isPending ? 'Импорт...' : 'Создать кошелёк и импортировать'}
        </Button>
      </Stack>

      <Dialog open={successDialogOpen} onClose={handleSuccessOk}>
        <DialogTitle>Импорт выполнен</DialogTitle>
        <DialogContent>
          <Typography>Кошелёк создан и транзакции успешно импортированы.</Typography>
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

export default ImportPdfNewWalletPage;
