export default {
  extends: ["stylelint-config-standard"],
  rules: {
    "color-no-hex": true,
    "color-named": "never",
    "function-disallowed-list": [
      "rgb",
      "rgba",
      "hsl",
      "hsla",
      "hwb",
      "lab",
      "lch",
      "oklab",
      "oklch",
    ],
    "selector-class-pattern": "^[a-z][a-zA-Z0-9]+$",
  },
  overrides: [
    {
      files: ["src/ui/theme/tokens.css"],
      rules: {
        "color-no-hex": null,
        "color-named": null,
        "function-disallowed-list": null,
      },
    },
  ],
};
