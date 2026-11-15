import type { ReactNode } from "react";
import styles from "./Card.module.css";

export type CardVariant = "default" | "subtle";
export type CardPadding = "sm" | "md" | "lg";

export interface CardProps {
  /** Content rendered inside the card surface. */
  children: ReactNode;
  /** Visual surface variant. Defaults to `"default"`. */
  variant?: CardVariant;
  /** Internal padding token. Defaults to `"lg"`. */
  padding?: CardPadding;
}

const PADDING_CLASSES: Record<CardPadding, string> = {
  sm: styles.padSm,
  md: styles.padMd,
  lg: styles.padLg,
};

/**
 * Surface container for grouping related UI content.
 */
export function Card({
  children,
  variant = "default",
  padding = "lg",
}: CardProps) {
  const classNames = [
    styles.card,
    variant === "subtle" ? styles.subtle : "",
    PADDING_CLASSES[padding],
  ]
    .filter(Boolean)
    .join(" ");

  return <section className={classNames}>{children}</section>;
}
