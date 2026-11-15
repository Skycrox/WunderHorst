import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { apiClient } from "../../api";
import {
  AppShellLayout,
  Badge,
  Button,
  Card,
  Heading,
  Stack,
  Text,
  type NavItem,
} from "../../ui";
import { useTheme } from "./ThemeContext";

const NAV_ITEMS: readonly NavItem[] = [
  { id: "library", label: "Bibliothek" },
  { id: "boxes", label: "Tonieboxen" },
  { id: "logs", label: "Logs" },
  { id: "settings", label: "Einstellungen" },
];

const SECTION_DESCRIPTIONS: Record<string, string> = {
  library: "Verwalte Tonies, Audio-Dateien und Tag-Zuordnungen.",
  boxes: "Übersicht über verbundene Tonieboxen und Zertifikate.",
  logs: "Live-Telemetrie und Systemprotokolle.",
  settings: "Systemkonfiguration, Sicherheit und Speicherpfade.",
};

export interface AppShellProps {
  username: string;
  onLogout: () => Promise<void>;
}

/**
 * Authenticated application shell with navigation, theme switcher, and backend health badge.
 */
export function AppShell({ username, onLogout }: AppShellProps) {
  const { theme, toggleTheme } = useTheme();
  const [activeNavId, setActiveNavId] = useState("library");

  const healthQuery = useQuery({
    queryKey: ["system", "health"],
    queryFn: async () => {
      const { data, response } = await apiClient.GET("/api/health");
      if (!response.ok || !data) {
        return { status: "Unhealthy", checks: {} };
      }
      return data;
    },
    refetchInterval: 30_000,
    retry: false,
  });

  const healthStatus = healthQuery.data?.status;
  const healthTone = healthQuery.isPending
    ? "neutral"
    : healthStatus === "Healthy"
      ? "success"
      : "danger";
  const healthLabel = healthQuery.isPending
    ? "System: Prüfe…"
    : `System: ${healthStatus ?? "Unhealthy"}`;

  const activeItem =
    NAV_ITEMS.find((item) => item.id === activeNavId) ?? NAV_ITEMS[0];

  return (
    <AppShellLayout
      brand={
        <Stack direction="horizontal" align="center" gap="sm">
          <Heading level={1}>WunderHorst</Heading>
          <Badge tone={healthTone}>{healthLabel}</Badge>
        </Stack>
      }
      headerActions={
        <Stack direction="horizontal" align="center" gap="sm">
          <Text variant="muted" as="span">
            {username}
          </Text>
          <Button
            variant="ghost"
            size="sm"
            onClick={toggleTheme}
            ariaLabel="Theme umschalten"
          >
            {theme === "light" ? "Dark Mode" : "Light Mode"}
          </Button>
          <Button
            variant="secondary"
            size="sm"
            onClick={() => void onLogout()}
          >
            Abmelden
          </Button>
        </Stack>
      }
      navItems={NAV_ITEMS}
      activeNavId={activeNavId}
      onSelectNav={setActiveNavId}
    >
      <Card>
        <Stack gap="sm">
          <Heading level={2}>{activeItem.label}</Heading>
          <Text variant="secondary">
            {SECTION_DESCRIPTIONS[activeItem.id]}
          </Text>
        </Stack>
      </Card>
    </AppShellLayout>
  );
}
