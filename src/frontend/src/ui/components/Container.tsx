import type { ReactNode } from "react";
import styles from "./Container.module.css";

export type ContainerSize = "sm" | "md" | "lg";

export interface ContainerProps {
  /** Content rendered inside the width-constrained container. */
  children: ReactNode;
  /** Maximum width preset. Defaults to `"lg"`. */
  size?: ContainerSize;
  /** Centers content vertically in the viewport (e.g. for auth screens). */
  centerInViewport?: boolean;
}

const SIZE_CLASSES: Record<ContainerSize, string> = {
  sm: styles.sm,
  md: styles.md,
  lg: styles.lg,
};

/**
 * Responsive page width container with optional full-viewport centering.
 */
export function Container({
  children,
  size = "lg",
  centerInViewport = false,
}: ContainerProps) {
  const inner = (
    <div className={`${styles.container} ${SIZE_CLASSES[size]}`}>
      {children}
    </div>
  );

  if (centerInViewport) {
    return <main className={styles.centeredViewport}>{inner}</main>;
  }

  return inner;
}
