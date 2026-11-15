import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AppShell, ThemeProvider } from "../features/app-shell";
import { AuthProvider, LoginView, useAuth } from "../features/auth";
import { Container, Text } from "../ui";

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

  return <AppShell username={user.username} onLogout={logout} />;
}

export function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider>
        <AuthProvider>
          <AuthenticatedGate />
        </AuthProvider>
      </ThemeProvider>
    </QueryClientProvider>
  );
}
