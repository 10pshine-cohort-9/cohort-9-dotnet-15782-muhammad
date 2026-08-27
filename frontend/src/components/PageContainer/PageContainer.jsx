import styles from "./PageContainer.module.css";

export default function PageContainer({ title, titleClassName, children }) {
  return (
    <div className={styles.page}>
      <div className={styles.content}>
        {title && <h1 className={`${styles.title} ${titleClassName || ""}`}>{title}</h1>}
        {children}
      </div>
    </div>
  );
}