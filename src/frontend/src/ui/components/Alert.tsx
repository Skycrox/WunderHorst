import type { ReactNode } from "react";
import styles from "./Alert.module.css";

export type AlertTone = "info" | "success" | "warning" | "danger";

export interface AlertProps {
  /** Alert body message. */
  children: ReactNode;
  /** Semantic severity tone. Defaults to `"info"`. */
  tone?: AlertTone;
  /** Optional bold summary title. */
  title?: string;
}

const TONE_CLASSES: Record<AlertTone, string> = {
  info: styles.info,
  success: styles.success,
  warning: styles.warning,
  danger: styles.danger,
};

/**
 * Accessible status banner (`role="alert"` for warnings/errors, `role="status"` otherwise).
 */
export function Alert({ children, tone = "info", title }: AlertProps) {
  const role = tone === "danger" || tone === "warning" ? "alert" : "status";

  return (
    <div className={`${styles.alert} ${TONE_CLASSES[tone]}`} role={role}>
      {title ? <span className={styles.title}>{title}</span> : null}
      <div>{children}</div>
    </div>
  );
}
