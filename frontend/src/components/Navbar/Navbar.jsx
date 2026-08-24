import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../../context/AuthContext";
import Button from "../Button/Button";
import styles from "./Navbar.module.css";

export default function Navbar() {
  const { user, isAdmin, logout } = useAuth();
  const navigate = useNavigate();

  function handleLogout() {
    logout();
    navigate("/login");
  }

  return (
    <nav className={styles.navbar}>
      <div className={styles.left}>
        <span className={styles.brand}>Task Manager</span>
        <Link to="/dashboard" className={styles.link}>Dashboard</Link>
        <Link to="/tasks" className={styles.link}>Tasks</Link>
        {isAdmin && (
          <Link to="/admin/users" className={styles.link}>Users</Link>
        )}
      </div>
      <div className={styles.right}>
        <Link to="/profile" className={styles.link}>{user?.fullName}</Link>
        <Button variant="secondary" onClick={handleLogout}>Logout</Button>
      </div>
    </nav>
  );
}