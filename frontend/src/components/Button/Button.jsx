import styles from "./Button.module.css";

export default function Button({
  variant = "primary",
  type = "button",
  disabled = false,
  onClick,
  children,
}) {
  return (
    <button
      type={type}
      disabled={disabled}
      onClick={onClick}
      className={`${styles.button} ${styles[variant]}`}
    >
      {children}
    </button>
  );
}