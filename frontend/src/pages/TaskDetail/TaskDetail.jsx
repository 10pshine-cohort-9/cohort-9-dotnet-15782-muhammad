import { useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { format } from "date-fns";
import { getTaskById, deleteTask } from "../../api/taskApi";
import { useAuth } from "../../context/AuthContext";
import PageContainer from "../../components/PageContainer/PageContainer";
import Button from "../../components/Button/Button";
import Badge from "../../components/Badge/Badge";
import styles from "./TaskDetail.module.css";

export default function TaskDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { isAdmin } = useAuth();
  const queryClient = useQueryClient();
  const [showConfirm, setShowConfirm] = useState(false);

  const { data: task, isLoading, isError, error } = useQuery({
    queryKey: ["task", id],
    queryFn: () => getTaskById(id),
  });

  const deleteMutation = useMutation({
    mutationFn: () => deleteTask(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["tasks"] });
      navigate("/tasks");
    },
  });

  if (isLoading) {
    return (
      <PageContainer title="Task Detail">
        <p>Loading task...</p>
      </PageContainer>
    );
  }

  if (isError) {
    return (
      <PageContainer title="Task Detail">
        <div className={styles.errorBox}>{error.message}</div>
        <Button variant="secondary" onClick={() => navigate("/tasks")}>
          Back to Tasks
        </Button>
      </PageContainer>
    );
  }

  const canDelete = !isAdmin;

  return (
    <PageContainer title={task.title}>
      <div className={styles.card}>
        <div className={styles.badges}>
          <Badge text={task.statusName} />
          <Badge text={task.priorityName} />
          <Badge text={task.categoryName} />
        </div>

        {task.description && (
          <p className={styles.description}>{task.description}</p>
        )}

        <div className={styles.metaGrid}>
          <div>
            <span className={styles.metaLabel}>Task ID</span>
            <span className={styles.metaValue}>{task.id}</span>
          </div>
          <div>
            <span className={styles.metaLabel}>Due Date</span>
            <span className={styles.metaValue}>
              {task.dueDate ? format(new Date(task.dueDate), "MMM d, yyyy") : "No due date"}
            </span>
          </div>
          <div>
            <span className={styles.metaLabel}>Created By</span>
            <span className={styles.metaValue}>{task.createdByUserName}</span>
          </div>
          <div>
            <span className={styles.metaLabel}>Assigned To</span>
            <span className={styles.metaValue}>{task.assignedToUserName}</span>
          </div>
          <div>
            <span className={styles.metaLabel}>Created</span>
            <span className={styles.metaValue}>
              {format(new Date(task.createdAt), "MMM d, yyyy")}
            </span>
          </div>
        </div>

        <div className={styles.actions}>
          <Button onClick={() => navigate(`/tasks/${id}/edit`)}>Edit</Button>
          {canDelete && (
            <Button variant="danger" onClick={() => setShowConfirm(true)}>
              Delete
            </Button>
          )}
          <Button variant="secondary" onClick={() => navigate("/tasks")}>
            Back
          </Button>
        </div>
      </div>

      {showConfirm && (
        <div className={styles.overlay}>
          <div className={styles.confirmBox}>
            <p>Are you sure you want to delete this task?</p>
            <div className={styles.confirmActions}>
              <Button variant="danger" onClick={() => deleteMutation.mutate()} disabled={deleteMutation.isPending}>
                {deleteMutation.isPending ? "Deleting..." : "Delete"}
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