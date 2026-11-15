import type { ReactNode } from "react";
import styles from "./Badge.module.css";

export type BadgeTone = "neutral" | "success" | "warning" | "danger" | "info";

export interface BadgeProps {
  /** Short status label. */
  children: ReactNode;
  /** Semantic color tone. Defaults to `"neutral"`. */
  tone?: BadgeTone;
}

const TONE_CLASSES: Record<BadgeTone, string> = {
  neutral: styles.neutral,
  success: styles.success,
  warning: styles.warning,
  danger: styles.danger,
  info: styles.info,
};

/**
 * Compact status indicator pill using semantic status tokens.
 */
export function Badge({ children, tone = "neutral" }: BadgeProps) {
  return (
    <span className={`${styles.badge} ${TONE_CLASSES[tone]}`}>{children}</span>
  );
}
