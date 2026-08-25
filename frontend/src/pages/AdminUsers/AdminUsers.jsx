import { useQuery } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { getUserSummaries } from "../../api/dashboardApi";
import { useAuth } from "../../context/AuthContext";
import PageContainer from "../../components/PageContainer/PageContainer";
import styles from "./AdminUsers.module.css";

export default function AdminUsers() {
  const navigate = useNavigate();

  const { user } = useAuth();

  const { data, isLoading, isError } = useQuery({
    queryKey: ["userSummaries"],
    queryFn: getUserSummaries,
  });

  const users = data?.filter((u) => u.userId !== user.userId);

  return (
    <PageContainer title="Users">
      {isLoading && <p>Loading users...</p>}
      {isError && <p className={styles.error}>Failed to load users.</p>}

      {users && users.length === 0 && (
        <p className={styles.emptyText}>No users found.</p>
      )}

      {users && users.length > 0 && (
        <div className={styles.list}>
          {users.map((u) => (
            <div
              key={u.userId}
              className={styles.row}
              onClick={() => navigate(`/tasks?userId=${u.userId}`)}
            >
              <div className={styles.rowMain}>
                <span className={styles.name}>{u.fullName}</span>
                <span className={styles.email}>{u.email}</span>
              </div>
              <span className={styles.taskCount}>
                {u.taskCount} {u.taskCount === 1 ? "task" : "tasks"}
              </span>
            </div>
          ))}
        </div>
      )}
    </PageContainer>
  );
}