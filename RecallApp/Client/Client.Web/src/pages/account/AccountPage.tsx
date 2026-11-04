import { Center, Loader, Stack, Text } from "@mantine/core";

import { toErrorView } from "@/shared/api/errors/errorView";
import { useMyAccount } from "@/features/account/api/useMyAccount";
import { ProfileCard } from "@/features/account/ui/ProfileCard";
import { RequireSession } from "@/features/session/ui/RequireSession";

/** Страница аккаунта: доступна только с сессией, показывает профиль из домена. */
export function AccountPage() {
  return (
    <RequireSession>
      <AccountContent />
    </RequireSession>
  );
}

function AccountContent() {
  const { data: profile, isPending, isError, error } = useMyAccount();

  if (isPending) {
    return (
      <Center h="50vh">
        <Loader />
      </Center>
    );
  }

  if (isError) {
    const view = toErrorView(error);
    return (
      <Center h="50vh">
        <Stack gap="xs" align="center">
          <Text fw={600}>Не удалось загрузить профиль</Text>
          <Text c="dimmed" size="sm">
            {view.message}
          </Text>
        </Stack>
      </Center>
    );
  }

  return <ProfileCard profile={profile} />;
}
