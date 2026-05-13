import { FullSizeCentered } from '@/components/styled';
import { Button, CircularProgress, Divider, FormControlLabel, Switch, Typography } from '@mui/material';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Box } from '@mui/material';

import Loading from '@/components/Loading';

type UserInfo = {
  name: string;
  claims: { type: string; value: string }[];
};

type EmailNotificationStatus = {
  isEmailNotificationsEnabled: boolean;
};

const fetchUser = async (): Promise<UserInfo | null> => {
  try {
    const res = await fetch('/bff/user', { credentials: 'include' });
    if (res.status === 401) return null;
    if (!res.ok) throw new Error(`HTTP error! status: ${res.status}`);
    return await res.json();
  } catch (e) {
    console.error('Failed to fetch user:', e);
    return null;
  }
};

const fetchEmailNotificationStatus = async (): Promise<EmailNotificationStatus> => {
  const res = await fetch('/api/finance/User/Notifications/Email/Status', {
    credentials: 'include',
  });
  if (!res.ok) throw new Error(`HTTP error! status: ${res.status}`);
  return await res.json();
};

const setEmailNotifications = async (enable: boolean): Promise<void> => {
  const action = enable ? 'Enable' : 'Disable';
  const res = await fetch(`/api/finance/User/Notifications/Email/${action}`, {
    method: 'POST',
    credentials: 'include',
  });
  if (!res.ok) throw new Error(`HTTP error! status: ${res.status}`);
};

function AccountPage() {
  const queryClient = useQueryClient();

  const { data: user, isLoading, isPending, error } = useQuery({
    queryKey: ['user'],
    queryFn: fetchUser,
    retry: false,
  });

  const { data: emailStatus, isLoading: isEmailStatusLoading } = useQuery({
    queryKey: ['email-notification-status'],
    queryFn: fetchEmailNotificationStatus,
    enabled: !!user,
    retry: false,
  });

  const toggleEmailMutation = useMutation({
    mutationFn: (enable: boolean) => setEmailNotifications(enable),
    onSuccess: (_, enable) => {
      queryClient.setQueryData<EmailNotificationStatus>(['email-notification-status'], {
        isEmailNotificationsEnabled: enable,
      });
    },
    onError: (err) => {
      console.error('Failed to toggle email notifications:', err);
    },
  });

  if (error) {
    console.error('Query error:', error);
  }

  const handleLogin = () => {
    window.location.href = '/bff/login';
  };

  const handleLogout = () => {
    window.location.href = '/bff/logout';
  };

  if (isLoading || isPending) {
    return <Loading />;
  }

  const isAuth = !!user;

  return (
    <>
      <meta name="title" content="Account" />
      {!isAuth && <Button onClick={handleLogin}>Войти</Button>}
      {isAuth && (
        <FullSizeCentered>
          <Typography variant="h3">Аккаунт</Typography>
          <Typography variant="body1">Здравствуйте, {user.name}</Typography>

          <Divider sx={{ width: '100%', my: 3 }} />

          <Box sx={{ width: '100%', maxWidth: 400 }}>
            <Typography variant="h6" gutterBottom>
              Уведомления
            </Typography>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              {isEmailStatusLoading ? (
                <CircularProgress size={20} />
              ) : (
                <FormControlLabel
                  control={
                    <Switch
                      checked={emailStatus?.isEmailNotificationsEnabled ?? false}
                      disabled={toggleEmailMutation.isPending}
                      onChange={(e) => toggleEmailMutation.mutate(e.target.checked)}
                    />
                  }
                  label="Email-уведомления о предстоящих регулярных транзакциях"
                />
              )}
            </Box>
            <Typography variant="caption" color="text.secondary">
              Сводное письмо отправляется за 7 дней до даты регулярных транзакций
            </Typography>
          </Box>

          <Divider sx={{ width: '100%', my: 3 }} />

          <Button onClick={handleLogout}>Выйти</Button>
        </FullSizeCentered>
      )}
    </>
  );
}

export default AccountPage;
