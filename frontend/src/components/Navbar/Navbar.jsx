import { NavLink, useNavigate } from "react-router-dom";
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

  const initials = user?.fullName
    ?.split(" ")
    .map((n) => n[0])
    .slice(0, 2)
    .join("")
    .toUpperCase();

  return (
    <nav className={styles.navbar}>
      <div className={styles.left}>
        <span className={styles.brand}>Task Manager</span>
        <NavLink
          to="/dashboard"
          className={({ isActive }) => `${styles.link} ${isActive ? styles.active : ""}`}
        >
          Dashboard
        </NavLink>
        <NavLink
          to="/tasks"
          className={({ isActive }) => `${styles.link} ${isActive ? styles.active : ""}`}
        >
          Tasks
        </NavLink>
        {isAdmin && (
          <NavLink
            to="/admin/users"
            className={({ isActive }) => `${styles.link} ${isActive ? styles.active : ""}`}
          >
            Users
          </NavLink>
        )}
      </div>
      <div className={styles.right}>
        <NavLink to="/profile" className={styles.profileLink}>
          <span className={styles.avatar}>{initials}</span>
          <span className={styles.userName}>{user?.fullName}</span>
        </NavLink>
        <Button variant="secondary" onClick={handleLogout}>Logout</Button>
      </div>
    </nav>
  );
}