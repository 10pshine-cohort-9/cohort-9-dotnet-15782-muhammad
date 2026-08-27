import { useMemo } from "react";
import { Link } from "react-router-dom";
import { getTasks } from "../../api/taskApi";
import { useQuery } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { ListTodo, Clock, Loader, CheckCircle2 } from "lucide-react";
import { useAuth } from "../../context/AuthContext";
import { getDashboard } from "../../api/dashboardApi";
import Badge from "../../components/Badge/Badge";
import PageContainer from "../../components/PageContainer/PageContainer";
import Button from "../../components/Button/Button";
import styles from "./Dashboard.module.css";


export default function Dashboard() {
  const { user, isAdmin } = useAuth();
  const navigate = useNavigate();

  const { data, isLoading, isError } = useQuery({
    queryKey: ["dashboard"],
    queryFn: getDashboard,
  });

  const { data: tasksData } = useQuery({
    queryKey: ["tasks"],
    queryFn: () => getTasks(),
  });

  const recentTasks = useMemo(() => {
    if (!tasksData) return [];
    return [...tasksData]
      .sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt))
      .slice(0, 5);
  }, [tasksData]);

  // Percentage of total tasks, used for the small share bar on each stat card.
  const share = (count) =>
    data?.totalTasks ? Math.round((count / data.totalTasks) * 100) : 0;

  return (
    <PageContainer title={`Welcome back, ${user?.fullName}`}>
      {isLoading && (
        <p className={styles.loading}>
          <span className={styles.spinner} aria-hidden="true" />
          Loading dashboard...
        </p>
      )}
      {isError && <p className={styles.error}>Failed to load dashboard data.</p>}

      {data && (
        <>
          <div className={styles.statsGrid}>
            <div className={`${styles.statCard} ${styles.total}`}>
              <span className={styles.iconChip}>
                <ListTodo size={23} className={styles.icon} />
              </span>
              <span className={styles.statNumber}>{data.totalTasks}</span>
              <span className={styles.statLabel}>Total Tasks</span>
              <div className={styles.barTrack} aria-hidden="true">
                <div className={styles.barFill} style={{ width: "100%" }} />
              </div>
            </div>
            <div className={`${styles.statCard} ${styles.todo}`}>
              <span className={styles.iconChip}>
                <Clock size={23} className={styles.icon} />
              </span>
              <span className={styles.statNumber}>{data.toDoCount}</span>
              <span className={styles.statLabel}>To Do</span>
              <div className={styles.barTrack} aria-hidden="true">
                <div className={styles.barFill} style={{ width: `${share(data.toDoCount)}%` }} />
              </div>
            </div>
            <div className={`${styles.statCard} ${styles.inProgress}`}>
              <span className={styles.iconChip}>
                <Loader size={23} className={styles.icon} />
              </span>
              <span className={styles.statNumber}>{data.inProgressCount}</span>
              <span className={styles.statLabel}>In Progress</span>
              <div className={styles.barTrack} aria-hidden="true">
                <div className={styles.barFill} style={{ width: `${share(data.inProgressCount)}%` }} />
              </div>
            </div>
            <div className={`${styles.statCard} ${styles.completed}`}>
              <span className={styles.iconChip}>
                <CheckCircle2 size={23} className={styles.icon} />
              </span>
              <span className={styles.statNumber}>{data.completedCount}</span>
              <span className={styles.statLabel}>Completed</span>
              <div className={styles.barTrack} aria-hidden="true">
                <div className={styles.barFill} style={{ width: `${share(data.completedCount)}%` }} />
              </div>
            </div>
          </div>

          <div className={styles.actions}>
            {isAdmin ? (
              <>
                <Button variant="flat" onClick={() => navigate("/admin/users")}>Manage Users</Button>
                <Button variant="secondary" onClick={() => navigate("/tasks")}>
                  View All Tasks
                </Button>
              </>
            ) : (
              <>
                <Button variant="flat" onClick={() => navigate("/tasks/new")}>New Task</Button>
                <Button variant="secondary" onClick={() => navigate("/tasks")}>
                  View All Tasks
                </Button>
              </>
            )}
          </div>
            <div className={styles.recentSection}>
            <h2 className={styles.sectionTitle}>Recent Tasks</h2>
            {recentTasks.length === 0 ? (
              <p className={styles.emptyText}>No tasks yet.</p>
            ) : (
              <div className={styles.recentList}>
                {recentTasks.map((task) => (
                  <Link
                    key={task.id}
                    to={`/tasks/${task.id}`}
                    className={styles.recentItem}
                  >
                    <span className={styles.recentTitle}>
                      {task.title}
                      {isAdmin && (
                        <span className={styles.assignee}> — {task.assignedToUserName}</span>
                      )}
                    </span>
                      <span className={styles.recentMeta}>
                          <Badge text={task.statusName} />
                          <Badge text={task.priorityName} />
                          <Badge text={task.categoryName} />
                    </span>
                  </Link>
                ))}
              </div>
            )}
          </div>
        </>
      )}
    </PageContainer>
  );
}