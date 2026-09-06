import { useNavigate } from "react-router-dom";
import { useAuth } from "../../context/AuthContext";
import Button from "../../components/Button/Button";
import styles from "./NotFound.module.css";

export default function NotFound() {
  const navigate = useNavigate();
  const { isAuthenticated } = useAuth();

  function handleGoBack() {
    navigate(isAuthenticated ? "/dashboard" : "/login");
  }

  return (
    <div className={styles.wrapper}>
      <div className={styles.content}>
        <h1 className={styles.code}>404</h1>
        <p className={styles.message}>Page not found.</p>
        <Button onClick={handleGoBack}>
          {isAuthenticated ? "Back to Dashboard" : "Back to Login"}
        </Button>
      </div>
    </div>
  );
}