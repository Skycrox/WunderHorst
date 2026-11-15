import type { MouseEventHandler, ReactNode } from "react";
import styles from "./Button.module.css";

export type ButtonVariant = "primary" | "secondary" | "ghost" | "danger";
export type ButtonSize = "sm" | "md" | "lg";

export interface ButtonProps {
  /** Button label or content. */
  children: ReactNode;
  /** Visual button style variant. Defaults to `"primary"`. */
  variant?: ButtonVariant;
  /** Button size preset. Defaults to `"md"`. */
  size?: ButtonSize;
  /** Native button type. Defaults to `"button"`. */
  type?: "button" | "submit" | "reset";
  /** Disables interaction and applies disabled styling. */
  disabled?: boolean;
  /** Marks the button as busy (`aria-busy`) and disables clicks while loading. */
  loading?: boolean;
  /** Click handler. */
  onClick?: MouseEventHandler<HTMLButtonElement>;
  /** Accessible label override when icon-only or condensed. */
  ariaLabel?: string;
}

const VARIANT_CLASSES: Record<ButtonVariant, string> = {
  primary: styles.primary,
  secondary: styles.secondary,
  ghost: styles.ghost,
  danger: styles.danger,
};

const SIZE_CLASSES: Record<ButtonSize, string> = {
  sm: styles.sm,
  md: styles.md,
  lg: styles.lg,
};

/**
 * Accessible action button supporting semantic variants, sizes, and loading state.
 */
export function Button({
  children,
  variant = "primary",
  size = "md",
  type = "button",
  disabled = false,
  loading = false,
  onClick,
  ariaLabel,
}: ButtonProps) {
  const isDisabled = disabled || loading;
  const className = `${styles.button} ${VARIANT_CLASSES[variant]} ${SIZE_CLASSES[size]}`;

  return (
    <button
      type={type}
      className={className}
      disabled={isDisabled}
      aria-busy={loading || undefined}
      aria-label={ariaLabel}
      onClick={onClick}
    >
      {children}
    </button>
  );
}
