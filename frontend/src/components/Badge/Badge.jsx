import styles from "./Badge.module.css";

const COLOR_MAP = {
  "To Do": "warning",
  "In Progress": "purple",
  "Completed": "success",
  "Low": "muted",
  "Medium": "warning",
  "High": "danger",
};

export default function Badge({ text }) {
  const variant = COLOR_MAP[text] || "muted";
  return <span className={`${styles.badge} ${styles[variant]}`}>{text}</span>;
}