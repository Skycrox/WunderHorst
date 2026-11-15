import type { ReactNode } from "react";
import styles from "./AppShellLayout.module.css";

export interface NavItem {
  /** Unique route/section identifier. */
  id: string;
  /** Visible navigation label. */
  label: string;
}

export interface AppShellLayoutProps {
  /** Left header slot (brand/title). */
  brand: ReactNode;
  /** Right header slot (status badge, theme toggle, user actions). */
  headerActions: ReactNode;
  /** Navigation items displayed in the sidebar. */
  navItems: readonly NavItem[];
  /** Currently active navigation item ID. */
  activeNavId: string;
  /** Callback invoked when a navigation item is selected. */
  onSelectNav: (id: string) => void;
  /** Main page content. */
  children: ReactNode;
}

/**
 * Responsive application shell layout with sticky header, sidebar navigation, and main landmark.
 */
export function AppShellLayout({
  brand,
  headerActions,
  navItems,
  activeNavId,
  onSelectNav,
  children,
}: AppShellLayoutProps) {
  return (
    <div className={styles.shell}>
      <header className={styles.header}>
        <div>{brand}</div>
        <div>{headerActions}</div>
      </header>
      <aside className={styles.sidebar} aria-label="Hauptnavigation">
        <nav className={styles.navList}>
          {navItems.map((item) => {
            const isActive = item.id === activeNavId;
            return (
              <button
                key={item.id}
                type="button"
                className={[
                  styles.navButton,
                  isActive ? styles.navButtonActive : "",
                ]
                  .filter(Boolean)
                  .join(" ")}
                aria-current={isActive ? "page" : undefined}
                onClick={() => onSelectNav(item.id)}
              >
                {item.label}
              </button>
            );
          })}
        </nav>
      </aside>
      <main className={styles.main}>{children}</main>
    </div>
  );
}
