import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./ui/theme/tokens.css";
import "./ui/theme/global.css";
import { App } from "./app/App";

const rootElement = document.getElementById("root");
if (rootElement) {
  createRoot(rootElement).render(
    <StrictMode>
      <App />
    </StrictMode>,
  );
}
