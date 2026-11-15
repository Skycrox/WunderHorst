import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { apiClient } from "../../api";
import { AppShell, ThemeProvider } from "./index";

function renderAppShell(onLogout = vi.fn().mockResolvedValue(undefined)) {
  const client = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });

  return render(
    <QueryClientProvider client={client}>
      <ThemeProvider>
        <AppShell username="admin" onLogout={onLogout} />
      </ThemeProvider>
    </QueryClientProvider>,
  );
}

describe("AppShell Feature", () => {
  beforeEach(() => {
    window.localStorage.clear();
    delete document.documentElement.dataset.theme;
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("displays health status, switches navigation sections, and persists theme changes", async () => {
    const user = userEvent.setup();
    const onLogout = vi.fn().mockResolvedValue(undefined);

    vi.spyOn(apiClient, "GET").mockResolvedValue({
      data: { status: "Healthy", checks: { sqlite: "Healthy" } },
      response: new Response(null, { status: 200 }),
    } as never);

    renderAppShell(onLogout);

    expect(await screen.findByText("System: Healthy")).toBeInTheDocument();
    expect(
      screen.getByRole("heading", { level: 2, name: "Bibliothek" }),
    ).toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: "Tonieboxen" }));
    expect(
      screen.getByRole("heading", { level: 2, name: "Tonieboxen" }),
    ).toBeInTheDocument();

    const themeButton = screen.getByRole("button", { name: "Theme umschalten" });
    expect(document.documentElement.dataset.theme).toBe("light");

    await user.click(themeButton);
    expect(document.documentElement.dataset.theme).toBe("dark");
    expect(window.localStorage.getItem("wunderhorst-theme")).toBe("dark");

    await user.click(screen.getByRole("button", { name: "Abmelden" }));
    expect(onLogout).toHaveBeenCalledTimes(1);
  });
});
