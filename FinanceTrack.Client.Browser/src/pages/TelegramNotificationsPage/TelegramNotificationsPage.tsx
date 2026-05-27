import TelegramIcon from "@mui/icons-material/Telegram";
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Step,
  StepContent,
  StepLabel,
  Stepper,
  TextField,
  Typography,
} from "@mui/material";
import { useMutation, useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { useNavigate } from "react-router";

type TelegramStatus = { isTelegramBotNotificationsEnabled: boolean };

const fetchTelegramStatus = async (): Promise<TelegramStatus> => {
  const res = await fetch("/api/finance/User/Notifications/Telegram/Status", {
    method: "GET",
    credentials: "include",
  });
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  return res.json();
};

const generateLink = async (): Promise<string> => {
  const res = await fetch(
    "/api/finance/User/Notifications/Telegram/LinkWithPrimaryCode",
    { method: "POST", credentials: "include" },
  );
  if (!res.ok) {
    const body = await res.json().catch(() => null);
    throw new Error(body?.detail ?? `Ошибка ${res.status}`);
  }
  const data = await res.json();
  return data.telegramBotLinkWithCode as string;
};

const enableNotifications = async (confirmationCode: number): Promise<void> => {
  const res = await fetch("/api/finance/User/Notifications/Telegram/Enable", {
    method: "POST",
    credentials: "include",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ confirmationCode }),
  });
  if (!res.ok) {
    const body = await res.json().catch(() => null);
    throw new Error(body?.detail ?? `Ошибка ${res.status}`);
  }
};

function TelegramNotificationsPage() {
  const navigate = useNavigate();
  const [activeStep, setActiveStep] = useState(0);
  const [botLink, setBotLink] = useState<string | null>(null);
  const [code, setCode] = useState("");
  const [enableError, setEnableError] = useState<string | null>(null);

  const { data: status } = useQuery({
    queryKey: ["telegram-notification-status"],
    queryFn: fetchTelegramStatus,
    retry: false,
  });

  const generateMutation = useMutation({
    mutationFn: generateLink,
    onSuccess: (link) => setBotLink(link),
  });

  const enableMutation = useMutation({
    mutationFn: () => enableNotifications(Number(code)),
    onSuccess: () => navigate("/account"),
    onError: (err: Error) => setEnableError(err.message),
  });

  if (status?.isTelegramBotNotificationsEnabled) {
    return (
      <Box sx={{ p: 3, maxWidth: 560 }}>
        <Alert severity="info" sx={{ mb: 2 }}>
          Telegram-уведомления уже подключены.
        </Alert>
        <Button onClick={() => navigate("/account")}>← Вернуться в аккаунт</Button>
      </Box>
    );
  }

  return (
    <Box sx={{ p: 3, maxWidth: 560 }}>
      <Button onClick={() => navigate("/account")} sx={{ mb: 3 }}>
        ← Назад
      </Button>

      <Typography variant="h5" gutterBottom>
        Подключение Telegram-уведомлений
      </Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 4 }}>
        Следуйте трём шагам для подключения уведомлений через Telegram-бота.
      </Typography>

      <Stepper activeStep={activeStep} orientation="vertical">
        {/* Шаг 1: Генерация ссылки */}
        <Step>
          <StepLabel>Генерация ссылки</StepLabel>
          <StepContent>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              Сгенерируйте персональную ссылку и перейдите по ней в Telegram-бота.
            </Typography>
            {generateMutation.isError && (
              <Alert severity="error" sx={{ mb: 2 }}>
                {generateMutation.error?.message ?? "Ошибка генерации ссылки"}
              </Alert>
            )}
            {!botLink ? (
              <Button
                variant="contained"
                onClick={() => generateMutation.mutate()}
                disabled={generateMutation.isPending}
                startIcon={
                  generateMutation.isPending ? (
                    <CircularProgress size={16} color="inherit" />
                  ) : (
                    <TelegramIcon />
                  )
                }
              >
                Сгенерировать ссылку
              </Button>
            ) : (
              <Box sx={{ display: "flex", flexDirection: "column", gap: 1.5 }}>
                <Alert severity="success">
                  Ссылка создана! Перейдите в бота, чтобы получить код.
                </Alert>
                <Button
                  variant="contained"
                  startIcon={<TelegramIcon />}
                  href={botLink}
                  target="_blank"
                  rel="noopener noreferrer"
                  component="a"
                >
                  Открыть бота
                </Button>
                <Button variant="outlined" onClick={() => setActiveStep(1)}>
                  Далее →
                </Button>
              </Box>
            )}
          </StepContent>
        </Step>

        {/* Шаг 2: Инструкция */}
        <Step>
          <StepLabel>Получите код в боте</StepLabel>
          <StepContent>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              Бот прислал вам 6-значный код подтверждения. Скопируйте его и
              перейдите к следующему шагу.
            </Typography>
            <Box sx={{ display: "flex", gap: 1 }}>
              <Button variant="outlined" onClick={() => setActiveStep(0)}>
                ← Назад
              </Button>
              <Button variant="contained" onClick={() => setActiveStep(2)}>
                У меня есть код →
              </Button>
            </Box>
          </StepContent>
        </Step>

        {/* Шаг 3: Ввод кода */}
        <Step>
          <StepLabel>Ввод кода подтверждения</StepLabel>
          <StepContent>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              Введите 6-значный код из Telegram-бота.
            </Typography>
            {enableError && (
              <Alert severity="error" sx={{ mb: 2 }}>
                {enableError}
              </Alert>
            )}
            <TextField
              label="Код подтверждения"
              value={code}
              onChange={(e) => {
                setEnableError(null);
                setCode(e.target.value.replace(/\D/g, "").slice(0, 6));
              }}
              inputProps={{ inputMode: "numeric", pattern: "[0-9]*" }}
              sx={{ mb: 2 }}
              fullWidth
              autoFocus
            />
            <Box sx={{ display: "flex", gap: 1 }}>
              <Button variant="outlined" onClick={() => setActiveStep(1)}>
                ← Назад
              </Button>
              <Button
                variant="contained"
                onClick={() => enableMutation.mutate()}
                disabled={code.length !== 6 || enableMutation.isPending}
                startIcon={
                  enableMutation.isPending ? (
                    <CircularProgress size={16} color="inherit" />
                  ) : null
                }
              >
                Подключить
              </Button>
            </Box>
          </StepContent>
        </Step>
      </Stepper>
    </Box>
  );
}

export default TelegramNotificationsPage;
