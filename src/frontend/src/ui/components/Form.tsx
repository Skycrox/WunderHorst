import type { FormEventHandler, ReactNode } from "react";

export interface FormProps {
  /** Form controls and actions. */
  children: ReactNode;
  /** Submit handler. */
  onSubmit: FormEventHandler<HTMLFormElement>;
}

/**
 * Accessible form wrapper encapsulating native `<form>` submission.
 */
export function Form({ children, onSubmit }: FormProps) {
  return (
    <form onSubmit={onSubmit} noValidate>
      {children}
    </form>
  );
}
