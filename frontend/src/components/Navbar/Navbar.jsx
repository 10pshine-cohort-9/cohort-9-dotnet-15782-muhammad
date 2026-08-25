import { NavLink } from "react-router-dom";
import { useAuth } from "../../context/AuthContext";
import styles from "./Navbar.module.css";

export default function Navbar() {
  const { user, isAdmin } = useAuth();

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
      </div>
    </nav>
  );
}