import type { ReactNode } from "react";
import styles from "./Heading.module.css";

export type HeadingLevel = 1 | 2 | 3 | 4;

export interface HeadingProps {
  /** Heading text. */
  children: ReactNode;
  /** Semantic heading level (`h1`–`h4`). Defaults to `2`. */
  level?: HeadingLevel;
}

const LEVEL_CLASSES: Record<HeadingLevel, string> = {
  1: styles.level1,
  2: styles.level2,
  3: styles.level3,
  4: styles.level4,
};

/**
 * Semantic heading component (`h1`–`h4`) styled via typography tokens.
 */
export function Heading({ children, level = 2 }: HeadingProps) {
  const className = `${styles.heading} ${LEVEL_CLASSES[level]}`;

  switch (level) {
    case 1:
      return <h1 className={className}>{children}</h1>;
    case 2:
      return <h2 className={className}>{children}</h2>;
    case 3:
      return <h3 className={className}>{children}</h3>;
    case 4:
      return <h4 className={className}>{children}</h4>;
  }
}
