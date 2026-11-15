import {
  createContext,
  useContext,
  type ReactNode,
} from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiClient, type components } from "../../api";

export type CurrentUser = components["schemas"]["CurrentUserResponse"];
export type LoginPayload = components["schemas"]["LoginRequest"];
export type SetupAdminPayload = components["schemas"]["SetupAdminRequest"];

export interface AuthContextValue {
  user: CurrentUser | null;
  needsSetup: boolean;
  isLoading: boolean;
  login: (payload: LoginPayload) => Promise<void>;
  setupAdmin: (payload: SetupAdminPayload) => Promise<void>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

const SETUP_STATUS_KEY = ["auth", "setup-status"] as const;
const CURRENT_USER_KEY = ["auth", "me"] as const;

export function AuthProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient();

  const setupStatusQuery = useQuery({
    queryKey: SETUP_STATUS_KEY,
    queryFn: async () => {
      const { data, error } = await apiClient.GET("/api/auth/setup-status");
      if (error || !data) {
        throw new Error("Failed to check setup status.");
      }
      return data;
    },
    retry: false,
  });

  const needsSetup = setupStatusQuery.data?.needsSetup ?? false;

  const currentUserQuery = useQuery({
    queryKey: CURRENT_USER_KEY,
    enabled: setupStatusQuery.isSuccess && !needsSetup,
    queryFn: async (): Promise<CurrentUser | null> => {
      const { data, response } = await apiClient.GET("/api/auth/me");
      if (response.status === 401) {
        return null;
      }
      if (!response.ok || !data) {
        throw new Error("Failed to fetch current user.");
      }
      return data;
    },
    retry: false,
  });

  const loginMutation = useMutation({
    mutationFn: async (payload: LoginPayload) => {
      const { data, response } = await apiClient.POST("/api/auth/login", {
        body: payload,
      });
      if (!response.ok || !data) {
        throw new Error("Invalid username or password.");
      }
      return data;
    },
    onSuccess: (data) => {
      queryClient.setQueryData(CURRENT_USER_KEY, {
        id: data.id,
        username: data.username,
        role: data.role,
      });
    },
  });

  const setupMutation = useMutation({
    mutationFn: async (payload: SetupAdminPayload) => {
      const { response } = await apiClient.POST("/api/auth/setup", {
        body: payload,
      });
      if (!response.ok) {
        throw new Error("Initial admin setup failed.");
      }
      await loginMutation.mutateAsync({
        username: payload.username,
        password: payload.password,
      });
    },
    onSuccess: () => {
      queryClient.setQueryData(SETUP_STATUS_KEY, { needsSetup: false });
    },
  });

  const logoutMutation = useMutation({
    mutationFn: async () => {
      await apiClient.POST("/api/auth/logout");
    },
    onSettled: () => {
      queryClient.setQueryData(CURRENT_USER_KEY, null);
    },
  });

  const isLoading =
    setupStatusQuery.isPending ||
    (!needsSetup && setupStatusQuery.isSuccess && currentUserQuery.isPending);

  const value: AuthContextValue = {
    user: currentUserQuery.data ?? null,
    needsSetup,
    isLoading,
    login: async (payload) => {
      await loginMutation.mutateAsync(payload);
    },
    setupAdmin: async (payload) => {
      await setupMutation.mutateAsync(payload);
    },
    logout: async () => {
      await logoutMutation.mutateAsync();
    },
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider.");
  }
  return context;
}
