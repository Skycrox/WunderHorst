import { useId, type ChangeEventHandler } from "react";
import styles from "./Input.module.css";

export type InputType = "text" | "password" | "email" | "url" | "search";

export interface InputProps {
  /** Visible label associated with the input via `htmlFor`. */
  label: string;
  /** Controlled input value. */
  value: string;
  /** Change event handler. */
  onChange: ChangeEventHandler<HTMLInputElement>;
  /** Input type. Defaults to `"text"`. */
  type?: InputType;
  /** Optional HTML `name` attribute. */
  name?: string;
  /** Optional placeholder text. */
  placeholder?: string;
  /** Optional autocomplete hint. */
  autoComplete?: string;
  /** Marks input as required. */
  required?: boolean;
  /** Disables user input. */
  disabled?: boolean;
  /** Validation error message; sets `aria-invalid="true"` when present. */
  error?: string;
  /** Optional helper text shown when there is no error. */
  helperText?: string;
}

/**
 * Accessible form input with bound label, helper text, and error state.
 */
export function Input({
  label,
  value,
  onChange,
  type = "text",
  name,
  placeholder,
  autoComplete,
  required = false,
  disabled = false,
  error,
  helperText,
}: InputProps) {
  const inputId = useId();
  const descriptionId = useId();
  const hasDescription = Boolean(error || helperText);

  return (
    <div className={styles.field}>
      <label className={styles.label} htmlFor={inputId}>
        {label}
      </label>
      <input
        id={inputId}
        name={name}
        type={type}
        value={value}
        onChange={onChange}
        placeholder={placeholder}
        autoComplete={autoComplete}
        required={required}
        disabled={disabled}
        aria-invalid={error ? true : undefined}
        aria-describedby={hasDescription ? descriptionId : undefined}
        className={[styles.input, error ? styles.inputError : ""]
          .filter(Boolean)
          .join(" ")}
      />
      {error ? (
        <span id={descriptionId} className={styles.errorText} role="alert">
          {error}
        </span>
      ) : helperText ? (
        <span id={descriptionId} className={styles.helper}>
          {helperText}
        </span>
      ) : null}
    </div>
  );
}
