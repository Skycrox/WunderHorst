import js from "@eslint/js";
import globals from "globals";
import reactHooks from "eslint-plugin-react-hooks";
import reactRefresh from "eslint-plugin-react-refresh";
import boundaries from "eslint-plugin-boundaries";
import tseslint from "typescript-eslint";

export default tseslint.config(
  { ignores: ["dist", "src/api/schema.d.ts"] },
  {
    extends: [js.configs.recommended, ...tseslint.configs.recommended],
    files: ["**/*.{ts,tsx}"],
    languageOptions: {
      ecmaVersion: 2022,
      globals: globals.browser,
    },
    plugins: {
      "react-hooks": reactHooks,
      "react-refresh": reactRefresh,
      boundaries,
    },
    settings: {
      "import/resolver": {
        typescript: {
          alwaysTryTypes: true,
          project: "./tsconfig.json",
        },
      },
      "boundaries/include": ["src/**/*"],
      "boundaries/elements": [
        { type: "ui", pattern: "src/ui/**/*" },
        { type: "api", pattern: "src/api/**/*" },
        { type: "app", pattern: "src/app/**/*" },
        { type: "test", pattern: "src/test/**/*" },
        {
          type: "feature",
          pattern: "src/features/*",
          capture: ["feature"],
        },
      ],
    },
    rules: {
      ...reactHooks.configs.recommended.rules,
      "react-refresh/only-export-components": [
        "warn",
        { allowConstantExport: true },
      ],
      "boundaries/element-types": [
        "error",
        {
          default: "disallow",
          rules: [
            { from: "ui", allow: ["ui"] },
            { from: "api", allow: ["api"] },
            { from: "app", allow: ["app", "ui", "api", "feature"] },
            {
              from: "feature",
              allow: [
                "ui",
                "api",
                ["feature", { feature: "${from.feature}" }],
              ],
            },
            { from: "test", allow: ["test", "ui", "api", "app", "feature"] },
          ],
        },
      ],
    },
  },
  {
    files: ["src/**/*.{ts,tsx}"],
    ignores: ["src/ui/**/*", "src/test/**/*"],
    rules: {
      "no-restricted-syntax": [
        "error",
        {
          selector: "JSXOpeningElement[name.name=/^[a-z]/]",
          message:
            "Raw HTML tags are only allowed inside src/ui/. Use UI components from src/ui instead.",
        },
      ],
    },
  },
  {
    files: ["src/features/**/*.{ts,tsx}"],
    rules: {
      "no-restricted-imports": [
        "error",
        {
          patterns: [
            {
              group: ["*.css", "**/*.css"],
              message:
                "CSS files are only allowed inside src/ui/. Compose UI components from src/ui instead.",
            },
          ],
        },
      ],
    },
  },
);
