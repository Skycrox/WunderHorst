import { useState, type FormEvent } from "react";
import {
  Alert,
  Button,
  Card,
  Container,
  Form,
  Heading,
  Input,
  Stack,
  Text,
} from "../../ui";
import { useAuth } from "./AuthContext";

/**
 * Login and initial administrator setup view.
 */
export function LoginView() {
  const { needsSetup, login, setupAdmin } = useAuth();
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setErrorMessage(null);

    const trimmedUsername = username.trim();
    if (trimmedUsername.length < 3) {
      setErrorMessage("Der Benutzername muss mindestens 3 Zeichen lang sein.");
      return;
    }

    if (needsSetup && password.length < 8) {
      setErrorMessage("Das Passwort muss mindestens 8 Zeichen lang sein.");
      return;
    }

    if (!needsSetup && password.length === 0) {
      setErrorMessage("Bitte gib ein Passwort ein.");
      return;
    }

    setIsSubmitting(true);
    try {
      if (needsSetup) {
        await setupAdmin({ username: trimmedUsername, password });
      } else {
        await login({ username: trimmedUsername, password });
      }
    } catch (err) {
      setErrorMessage(
        err instanceof Error ? err.message : "Anmeldung fehlgeschlagen.",
      );
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <Container size="sm" centerInViewport>
      <Card padding="lg">
        <Stack gap="lg">
          <Stack gap="xs">
            <Heading level={1}>
              {needsSetup ? "WunderHorst einrichten" : "WunderHorst Anmeldung"}
            </Heading>
            <Text variant="muted">
              {needsSetup
                ? "Lege den initialen Administrator-Account an, um WunderHorst zu starten."
                : "Melde dich mit deinem Konto an, um auf das Dashboard zuzugreifen."}
            </Text>
          </Stack>

          {errorMessage ? (
            <Alert tone="danger" title="Fehler">
              {errorMessage}
            </Alert>
          ) : null}

          <Form onSubmit={handleSubmit}>
            <Stack gap="md">
              <Input
                label="Benutzername"
                name="username"
                autoComplete="username"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                required
              />
              <Input
                label="Passwort"
                name="password"
                type="password"
                autoComplete={needsSetup ? "new-password" : "current-password"}
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                helperText={
                  needsSetup ? "Mindestens 8 Zeichen." : undefined
                }
                required
              />
              <Button type="submit" loading={isSubmitting}>
                {needsSetup ? "Administrator anlegen" : "Anmelden"}
              </Button>
            </Stack>
          </Form>
        </Stack>
      </Card>
    </Container>
  );
}
