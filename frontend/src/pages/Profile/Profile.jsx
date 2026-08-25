import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { getMe } from "../../api/authApi";
import { useAuth } from "../../context/AuthContext";
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

  return (
    <PageContainer title="Profile">
      <div className={styles.card}>
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

        <div className={styles.actions}>
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