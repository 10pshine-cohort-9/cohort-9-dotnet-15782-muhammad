import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { getMe } from "../../api/authApi";
import { useAuth } from "../../context/AuthContext";
import Badge from "../../components/Badge/Badge";
import PageContainer from "../../components/PageContainer/PageContainer";
import Button from "../../components/Button/Button";
import styles from "./Profile.module.css";

export default function Profile() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [showConfirm, setShowConfirm] = useState(false);

  const { data: me, isLoading, isError, error } = useQuery({
    queryKey: ["me"],
    queryFn: getMe,
  });

  function handleLogout() {
    logout();
    navigate("/login");
  }

  if (isLoading) {
    return (
      <PageContainer title="Profile">
        <p>Loading profile...</p>
      </PageContainer>
    );
  }

  if (isError) {
    return (
      <PageContainer title="Profile">
        <div className={styles.errorBox}>{error.message}</div>
      </PageContainer>
    );
  }

   const initials = user?.fullName
    ?.split(" ")
    .map((n) => n[0])
    .slice(0, 2)
    .join("")
    .toUpperCase();

  return (
    <PageContainer title="Profile" titleClassName={styles.centeredTitle}>
      <div className={styles.wrapper}>
        <div className={styles.hero}>
          <div className={styles.avatarLarge}>{initials}</div>
          <h2 className={styles.name}>{user?.fullName}</h2>
          <Badge text={me.role} />
          <p className={styles.email}>{me.email}</p>
        </div>

        <div className={styles.card}>
          <h3 className={styles.sectionTitle}>Account Information</h3>
          <div className={styles.metaGrid}>
            <div>
              <span className={styles.metaLabel}>Full Name</span>
              <span className={styles.metaValue}>{user?.fullName}</span>
            </div>
            <div>
              <span className={styles.metaLabel}>Email</span>
              <span className={styles.metaValue}>{me.email}</span>
            </div>
            <div>
              <span className={styles.metaLabel}>Role</span>
              <span className={styles.metaValue}>{me.role}</span>
            </div>
          </div>
        </div>

        <div className={styles.dangerCard}>
          <div>
            <h3 className={styles.dangerTitle}>Session</h3>
            <p className={styles.dangerText}>Logging out will end your current session on this device.</p>
          </div>
          <Button variant="danger" onClick={() => setShowConfirm(true)}>
            Logout
          </Button>
        </div>
      </div>

      {showConfirm && (
        <div className={styles.overlay}>
          <div className={styles.confirmBox}>
            <p>Are you sure you want to logout?</p>
            <div className={styles.confirmActions}>
              <Button variant="danger" onClick={handleLogout}>
                Logout
              </Button>
              <Button variant="secondary" onClick={() => setShowConfirm(false)}>
                Cancel
              </Button>
            </div>
          </div>
        </div>
      )}
    </PageContainer>
  );
}