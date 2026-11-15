import type { ReactNode } from "react";
import styles from "./Stack.module.css";

export type StackDirection = "vertical" | "horizontal";
export type StackGap = "xs" | "sm" | "md" | "lg" | "xl";
export type StackAlign = "start" | "center" | "end" | "stretch";
export type StackJustify = "start" | "center" | "end" | "between";

export interface StackProps {
  /** Child elements arranged by the flex container. */
  children: ReactNode;
  /** Flex direction. Defaults to `"vertical"`. */
  direction?: StackDirection;
  /** Spacing token between items. Defaults to `"md"`. */
  gap?: StackGap;
  /** Cross-axis alignment. Defaults to `"stretch"`. */
  align?: StackAlign;
  /** Main-axis alignment. Defaults to `"start"`. */
  justify?: StackJustify;
  /** Allows items to wrap onto multiple lines. */
  wrap?: boolean;
  /** Optional ARIA role (e.g. `"form"` or `"region"`). */
  role?: string;
}

const GAP_CLASSES: Record<StackGap, string> = {
  xs: styles.gapXs,
  sm: styles.gapSm,
  md: styles.gapMd,
  lg: styles.gapLg,
  xl: styles.gapXl,
};

const ALIGN_CLASSES: Record<StackAlign, string> = {
  start: styles.alignStart,
  center: styles.alignCenter,
  end: styles.alignEnd,
  stretch: styles.alignStretch,
};

const JUSTIFY_CLASSES: Record<StackJustify, string> = {
  start: styles.justifyStart,
  center: styles.justifyCenter,
  end: styles.justifyEnd,
  between: styles.justifyBetween,
};

/**
 * Flex layout primitive for vertical and horizontal spacing using design tokens.
 */
export function Stack({
  children,
  direction = "vertical",
  gap = "md",
  align = "stretch",
  justify = "start",
  wrap = false,
  role,
}: StackProps) {
  const classNames = [
    styles.stack,
    direction === "horizontal" ? styles.horizontal : styles.vertical,
    GAP_CLASSES[gap],
    ALIGN_CLASSES[align],
    JUSTIFY_CLASSES[justify],
    wrap ? styles.wrap : "",
  ]
    .filter(Boolean)
    .join(" ");

  return (
    <div className={classNames} role={role}>
      {children}
    </div>
  );
}
