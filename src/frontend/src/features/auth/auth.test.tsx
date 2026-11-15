import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { apiClient } from "../../api";
import { AuthProvider, LoginView, useAuth } from "./index";
import { Button, Text } from "../../ui";

function TestHarness() {
  const { user, isLoading, logout } = useAuth();
  if (isLoading) {
    return <Text>Loading</Text>;
  }
  if (!user) {
    return <LoginView />;
  }
  return (
    <Button onClick={() => void logout()}>
      Signed in: {user.username}
    </Button>
  );
}

function renderWithQuery(ui: React.ReactNode) {
  const client = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return render(
    <QueryClientProvider client={client}>
      <AuthProvider>{ui}</AuthProvider>
    </QueryClientProvider>,
  );
}

describe("Auth Feature", () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("renders initial admin setup when needsSetup is true and signs in after creation", async () => {
    const user = userEvent.setup();
    let signedIn = false;

    vi.spyOn(apiClient, "GET").mockImplementation(async (path) => {
      if (path === "/api/auth/setup-status") {
        return {
          data: { needsSetup: !signedIn },
          response: new Response(null, { status: 200 }),
        } as never;
      }
      if (signedIn) {
        return {
          data: { id: "1", username: "admin", role: "Admin" },
          response: new Response(null, { status: 200 }),
        } as never;
      }
      return {
        data: undefined,
        response: new Response(null, { status: 401 }),
      } as never;
    });

    const postSpy = vi.spyOn(apiClient, "POST").mockImplementation(async (path) => {
      if (path === "/api/auth/setup") {
        return {
          data: { id: "1", username: "admin", role: "Admin", createdAt: "now" },
          response: new Response(null, { status: 201 }),
        } as never;
      }
      if (path === "/api/auth/login") {
        signedIn = true;
        return {
          data: { id: "1", username: "admin", role: "Admin" },
          response: new Response(null, { status: 200 }),
        } as never;
      }
      return {
        data: undefined,
        response: new Response(null, { status: 204 }),
      } as never;
    });

    renderWithQuery(<TestHarness />);

    expect(
      await screen.findByRole("heading", { name: "WunderHorst einrichten" }),
    ).toBeInTheDocument();

    await user.type(screen.getByLabelText("Benutzername"), "admin");
    await user.type(screen.getByLabelText("Passwort"), "StrongPass123!");
    await user.click(
      screen.getByRole("button", { name: "Administrator anlegen" }),
    );

    expect(
      await screen.findByRole("button", { name: "Signed in: admin" }),
    ).toBeInTheDocument();
    expect(postSpy).toHaveBeenCalledWith("/api/auth/setup", {
      body: { username: "admin", password: "StrongPass123!" },
    });
  });

  it("shows error alert on invalid login credentials and signs out cleanly", async () => {
    const user = userEvent.setup();

    vi.spyOn(apiClient, "GET").mockImplementation(async (path) => {
      if (path === "/api/auth/setup-status") {
        return {
          data: { needsSetup: false },
          response: new Response(null, { status: 200 }),
        } as never;
      }
      return {
        data: undefined,
        response: new Response(null, { status: 401 }),
      } as never;
    });

    vi.spyOn(apiClient, "POST")
      .mockResolvedValueOnce({
        data: undefined,
        response: new Response(null, { status: 401 }),
      } as never)
      .mockResolvedValueOnce({
        data: { id: "1", username: "admin", role: "Admin" },
        response: new Response(null, { status: 200 }),
      } as never)
      .mockResolvedValueOnce({
        data: undefined,
        response: new Response(null, { status: 204 }),
      } as never);

    renderWithQuery(<TestHarness />);

    expect(
      await screen.findByRole("heading", { name: "WunderHorst Anmeldung" }),
    ).toBeInTheDocument();

    await user.type(screen.getByLabelText("Benutzername"), "admin");
    await user.type(screen.getByLabelText("Passwort"), "wrongpass");
    await user.click(screen.getByRole("button", { name: "Anmelden" }));

    expect(await screen.findByRole("alert")).toHaveTextContent(
      "Invalid username or password.",
    );

    await user.clear(screen.getByLabelText("Passwort"));
    await user.type(screen.getByLabelText("Passwort"), "StrongPass123!");
    await user.click(screen.getByRole("button", { name: "Anmelden" }));

    const signedInButton = await screen.findByRole("button", {
      name: "Signed in: admin",
    });
    await user.click(signedInButton);

    await waitFor(() => {
      expect(
        screen.getByRole("heading", { name: "WunderHorst Anmeldung" }),
      ).toBeInTheDocument();
    });
  });
});
