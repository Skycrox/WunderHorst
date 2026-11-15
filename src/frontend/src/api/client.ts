import createClient from "openapi-fetch";
import type { paths } from "./schema";

/**
 * Typed OpenAPI fetch client configured for HttpOnly cookie sessions.
 */
export const apiClient = createClient<paths>({
  baseUrl: "",
  credentials: "include",
});
