import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AuthProvider, LoginView, useAuth } from "../features/auth";
import { Button, Card, Container, Heading, Stack, Text } from "../ui";

const queryClient = new QueryClient();

function AuthenticatedGate() {
  const { user, isLoading, logout } = useAuth();

  if (isLoading) {
    return (
      <Container size="sm" centerInViewport>
        <Text variant="muted">Lade Sitzung…</Text>
      </Container>
    );
  }

  if (!user) {
    return <LoginView />;
  }

  return (
    <Container size="lg">
      <Card>
        <Stack direction="horizontal" justify="between" align="center">
          <Stack gap="xs">
            <Heading level={1}>WunderHorst</Heading>
            <Text variant="muted">Angemeldet als {user.username}</Text>
          </Stack>
          <Button variant="secondary" onClick={() => void logout()}>
            Abmelden
          </Button>
        </Stack>
      </Card>
    </Container>
  );
}

export function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <AuthenticatedGate />
      </AuthProvider>
    </QueryClientProvider>
  );
}
