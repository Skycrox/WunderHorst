import type { ReactNode } from "react";
import styles from "./Text.module.css";

export type TextVariant = "body" | "secondary" | "muted" | "caption" | "error";

export interface TextProps {
  /** Text content to display. */
  children: ReactNode;
  /** Semantic text style variant. Defaults to `"body"`. */
  variant?: TextVariant;
  /** HTML element tag (`"p"` or `"span"`). Defaults to `"p"`. */
  as?: "p" | "span";
}

const VARIANT_CLASSES: Record<TextVariant, string> = {
  body: "",
  secondary: styles.secondary,
  muted: styles.muted,
  caption: styles.caption,
  error: styles.error,
};

/**
 * Typography primitive for body copy, captions, and inline status messages.
 */
export function Text({
  children,
  variant = "body",
  as = "p",
}: TextProps) {
  const className = [styles.text, VARIANT_CLASSES[variant]]
    .filter(Boolean)
    .join(" ");

  if (as === "span") {
    return <span className={className}>{children}</span>;
  }

  return <p className={className}>{children}</p>;
}
