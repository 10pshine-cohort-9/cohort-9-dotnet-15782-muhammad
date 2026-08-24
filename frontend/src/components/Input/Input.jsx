import styles from "./Input.module.css";

export default function Input({ label, error, id, ...rest }) {
  return (
    <div className={styles.field}>
      {label && (
        <label htmlFor={id} className={styles.label}>
          {label}
        </label>
      )}
      <input id={id} className={styles.input} {...rest} />
      {error && <span className={styles.error}>{error}</span>}
    </div>
  );
}