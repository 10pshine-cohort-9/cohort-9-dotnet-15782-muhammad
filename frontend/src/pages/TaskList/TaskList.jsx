import { useState, useMemo, useEffect } from "react";
import { useQuery } from "@tanstack/react-query";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { Search, Plus } from "lucide-react";
import { getTasks } from "../../api/taskApi";
import { useAuth } from "../../context/AuthContext";
import { getUserSummaries } from "../../api/dashboardApi";
import { STATUSES, PRIORITIES, CATEGORIES } from "../../constants/lookups";
import PageContainer from "../../components/PageContainer/PageContainer";
import Button from "../../components/Button/Button";
import Badge from "../../components/Badge/Badge";
import styles from "./TaskList.module.css";

export default function TaskList() {
  const { isAdmin } = useAuth();
  const navigate = useNavigate();

  const [searchParams] = useSearchParams();
  const userId = searchParams.get("userId");

  const { data: userSummaries } = useQuery({
    queryKey: ["userSummaries"],
    queryFn: getUserSummaries,
    enabled: Boolean(userId) && isAdmin,
  });
  const viewedUser = userSummaries?.find((u) => String(u.userId) === userId);

  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [priorityFilter, setPriorityFilter] = useState("");
  const [categoryFilter, setCategoryFilter] = useState("");

  useEffect(() => {
    const timer = setTimeout(() => setSearch(searchInput), 300);
    return () => clearTimeout(timer);
  }, [searchInput]);

  const { data, isLoading, isError } = useQuery({
    queryKey: ["tasks", search, userId],
    queryFn: () => getTasks({ search: search || undefined, userId: userId || undefined }),
  });

  const filteredTasks = useMemo(() => {
    if (!data) return [];
    return data.filter((task) => {
      if (statusFilter && task.statusName !== statusFilter) return false;
      if (priorityFilter && task.priorityName !== priorityFilter) return false;
      if (categoryFilter && task.categoryName !== categoryFilter) return false;
      return true;
    });
  }, [data, statusFilter, priorityFilter, categoryFilter]);

  return (
    <PageContainer title={userId ? `Tasks — ${viewedUser?.fullName ?? "..."}` : "Tasks"}>
      {userId && (
        <div className={styles.scopedHeader}>
          <Link to="/admin/users" className={styles.backLink}>
            ← Back to Users
          </Link>
          <Button onClick={() => navigate(`/tasks/new?userId=${userId}`)}>
            <Plus size={16} /> New Task for {viewedUser?.fullName ?? "User"}
          </Button>
        </div>
      )}
        <div className={styles.toolbar}>
        <div className={styles.searchBox}>
          <Search size={18} className={styles.searchIcon} />
          <input
            type="text"
            placeholder="Search by title..."
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            className={styles.searchInput}
          />
        </div>

        <select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
          className={styles.select}
        >
          <option value="">All Statuses</option>
          {STATUSES.map((s) => (
            <option key={s.id} value={s.name}>{s.name}</option>
          ))}
        </select>

        <select
          value={priorityFilter}
          onChange={(e) => setPriorityFilter(e.target.value)}
          className={styles.select}
        >
          <option value="">All Priorities</option>
          {PRIORITIES.map((p) => (
            <option key={p.id} value={p.name}>{p.name}</option>
          ))}
        </select>

        <select
          value={categoryFilter}
          onChange={(e) => setCategoryFilter(e.target.value)}
          className={styles.select}
        >
          <option value="">All Categories</option>
          {CATEGORIES.map((c) => (
            <option key={c.id} value={c.name}>{c.name}</option>
          ))}
        </select>

        {!userId && (
          <Button onClick={() => navigate("/tasks/new")}>
            <Plus size={16} /> New Task
          </Button>
        )}
      </div>

      {isLoading && <p>Loading tasks...</p>}
      {isError && <p className={styles.error}>Failed to load tasks.</p>}

      {data && filteredTasks.length === 0 && (
        <p className={styles.emptyText}>No tasks match your filters.</p>
      )}

      {filteredTasks.length > 0 && (
        <div className={styles.list}>
          {filteredTasks.map((task) => (
            <Link key={task.id} to={`/tasks/${task.id}`} className={styles.row}>
              <div className={styles.rowMain}>
                <span className={styles.title}>{task.title}</span>
                {isAdmin && (
                  <span className={styles.assignee}>{task.assignedToUserName}</span>
                )}
              </div>
              <div className={styles.rowBadges}>
                <Badge text={task.statusName} />
                <Badge text={task.priorityName} />
                <Badge text={task.categoryName} />
              </div>
            </Link>
          ))}
        </div>
      )}
    </PageContainer>
  );
}