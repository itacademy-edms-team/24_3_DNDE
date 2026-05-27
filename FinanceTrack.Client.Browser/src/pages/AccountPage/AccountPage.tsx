import { FullSizeCentered } from "@/components/styled";
import TelegramIcon from "@mui/icons-material/Telegram";
import {
  Box,
  Button,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  Divider,
  FormControlLabel,
  Switch,
  Typography,
} from "@mui/material";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { useNavigate } from "react-router";

import Loading from "@/components/Loading";

type UserInfo = {
  name: string;
  claims: { type: string; value: string }[];
};

type EmailNotificationStatus = {
  isEmailNotificationsEnabled: boolean;
};

type TelegramNotificationStatus = {
  isTelegramBotNotificationsEnabled: boolean;
};

const fetchUser = async (): Promise<UserInfo | null> => {
  try {
    const res = await fetch("/bff/user", { credentials: "include" });
    if (res.status === 401) return null;
    if (!res.ok) throw new Error(`HTTP error! status: ${res.status}`);
    return await res.json();
  } catch (e) {
    console.error("Failed to fetch user:", e);
    return null;
  }
};

const fetchEmailNotificationStatus = async (): Promise<EmailNotificationStatus> => {
  const res = await fetch("/api/finance/User/Notifications/Email/Status", {
    credentials: "include",
  });
  if (!res.ok) throw new Error(`HTTP error! status: ${res.status}`);
  return res.json();
};

const setEmailNotifications = async (enable: boolean): Promise<void> => {
  const action = enable ? "Enable" : "Disable";
  const res = await fetch(`/api/finance/User/Notifications/Email/${action}`, {
    method: "POST",
    credentials: "include",
  });
  if (!res.ok) throw new Error(`HTTP error! status: ${res.status}`);
};

const fetchTelegramNotificationStatus =
  async (): Promise<TelegramNotificationStatus> => {
    const res = await fetch(
      "/api/finance/User/Notifications/Telegram/Status",
      { method: "GET", credentials: "include" },
    );
    if (!res.ok) throw new Error(`HTTP error! status: ${res.status}`);
    return res.json();
  };

const disableTelegramNotifications = async (): Promise<void> => {
  const res = await fetch("/api/finance/User/Notifications/Telegram/Disable", {
    method: "POST",
    credentials: "include",
  });
  if (!res.ok) throw new Error(`HTTP error! status: ${res.status}`);
};

function AccountPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [disconnectDialogOpen, setDisconnectDialogOpen] = useState(false);

  const {
    data: user,
    isLoading,
    isPending,
    error,
  } = useQuery({
    queryKey: ["user"],
    queryFn: fetchUser,
    retry: false,
  });

  const { data: emailStatus, isLoading: isEmailStatusLoading } = useQuery({
    queryKey: ["email-notification-status"],
    queryFn: fetchEmailNotificationStatus,
    enabled: !!user,
    retry: false,
  });

  const { data: telegramStatus, isLoading: isTelegramStatusLoading } = useQuery(
    {
      queryKey: ["telegram-notification-status"],
      queryFn: fetchTelegramNotificationStatus,
      enabled: !!user,
      retry: false,
    },
  );

  const toggleEmailMutation = useMutation({
    mutationFn: (enable: boolean) => setEmailNotifications(enable),
    onSuccess: (_, enable) => {
      queryClient.setQueryData<EmailNotificationStatus>(
        ["email-notification-status"],
        { isEmailNotificationsEnabled: enable },
      );
    },
    onError: (err) => {
      console.error("Failed to toggle email notifications:", err);
    },
  });

  const disableTelegramMutation = useMutation({
    mutationFn: disableTelegramNotifications,
    onSuccess: () => {
      queryClient.setQueryData<TelegramNotificationStatus>(
        ["telegram-notification-status"],
        { isTelegramBotNotificationsEnabled: false },
      );
      setDisconnectDialogOpen(false);
    },
    onError: (err) => {
      console.error("Failed to disable telegram notifications:", err);
      setDisconnectDialogOpen(false);
    },
  });

  if (error) {
    console.error("Query error:", error);
  }

  const handleLogin = () => {
    window.location.href = "/bff/login";
  };

  const handleLogout = () => {
    window.location.href = "/bff/logout";
  };

  if (isLoading || isPending) {
    return <Loading />;
  }

  const isAuth = !!user;
  const isTelegramConnected =
    telegramStatus?.isTelegramBotNotificationsEnabled ?? false;

  return (
    <>
      <meta name="title" content="Account" />
      {!isAuth && <Button onClick={handleLogin}>Войти</Button>}
      {isAuth && (
        <FullSizeCentered>
          <Typography variant="h3">Аккаунт</Typography>
          <Typography variant="body1">Здравствуйте, {user.name}</Typography>

          <Divider sx={{ width: "100%", my: 3 }} />

          <Box sx={{ width: "100%", maxWidth: 400 }}>
            <Typography variant="h6" gutterBottom>
              Уведомления
            </Typography>

            {/* Email */}
            <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
              {isEmailStatusLoading ? (
                <CircularProgress size={20} />
              ) : (
                <FormControlLabel
                  control={
                    <Switch
                      checked={
                        emailStatus?.isEmailNotificationsEnabled ?? false
                      }
                      disabled={toggleEmailMutation.isPending}
                      onChange={(e) =>
                        toggleEmailMutation.mutate(e.target.checked)
                      }
                    />
                  }
                  label="Email-уведомления о предстоящих регулярных транзакциях"
                />
              )}
            </Box>
            <Typography variant="caption" color="text.secondary">
              Сводное письмо отправляется за 7 дней до даты регулярных
              транзакций
            </Typography>

            {/* Telegram */}
            <Box sx={{ mt: 3 }}>
              <Typography
                variant="body1"
                gutterBottom
              >
                Telegram-уведомления о предстоящих регулярных транзакциях
              </Typography>
              {isTelegramStatusLoading ? (
                <CircularProgress size={20} />
              ) : isTelegramConnected ? (
                <Box
                  sx={{ display: "flex", alignItems: "center", gap: 2, mt: 1 }}
                >
                  <Button
                    variant="outlined"
                    startIcon={<TelegramIcon />}
                    size="small"
                    color="error"
                    onClick={() => setDisconnectDialogOpen(true)}
                  >
                    Отключить
                  </Button>
                </Box>
              ) : (
                <Button
                  variant="outlined"
                  startIcon={<TelegramIcon />}
                  onClick={() => navigate("/account/telegram-notifications")}
                  sx={{ mt: 1 }}
                >
                  Подключить
                </Button>
              )}
            </Box>
          </Box>

          <Divider sx={{ width: "100%", my: 3 }} />

          <Button onClick={handleLogout}>Выйти</Button>
        </FullSizeCentered>
      )}

      {/* Диалог подтверждения отключения */}
      <Dialog
        open={disconnectDialogOpen}
        onClose={() => setDisconnectDialogOpen(false)}
      >
        <DialogTitle>Отключить Telegram-уведомления?</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Бот перестанет присылать уведомления. Вы сможете подключить их
            снова в любое время.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDisconnectDialogOpen(false)}>Отмена</Button>
          <Button
            color="error"
            onClick={() => disableTelegramMutation.mutate()}
            disabled={disableTelegramMutation.isPending}
            startIcon={
              disableTelegramMutation.isPending ? (
                <CircularProgress size={16} color="inherit" />
              ) : null
            }
          >
            Отключить
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
}

export default AccountPage;
