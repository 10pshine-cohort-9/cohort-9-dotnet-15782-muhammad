import { useState, useEffect } from "react";
import { useParams, useNavigate, useSearchParams } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { getTaskById, createTask, updateTask } from "../../api/taskApi";
import { getUserSummaries } from "../../api/dashboardApi";
import { useAuth } from "../../context/AuthContext";
import { STATUSES, PRIORITIES, CATEGORIES } from "../../constants/lookups";
import PageContainer from "../../components/PageContainer/PageContainer";
import Input from "../../components/Input/Input";
import Button from "../../components/Button/Button";
import Select from "../../components/Select/Select";
import styles from "./TaskForm.module.css";

export default function TaskForm() {
  const { id } = useParams();
  const isEdit = Boolean(id);
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const presetUserId = searchParams.get("userId");
  const { isAdmin } = useAuth();
  const queryClient = useQueryClient();

  const [form, setForm] = useState({
    title: "",
    description: "",
    dueDate: "",
    statusId: "",
    priorityId: "",
    categoryId: "",
    assignedToUserId: "",
  });
  const [fieldErrors, setFieldErrors] = useState({});
  const [submitError, setSubmitError] = useState("");

  const { data: existingTask, isLoading: isLoadingTask } = useQuery({
    queryKey: ["task", id],
    queryFn: () => getTaskById(id),
    enabled: isEdit,
  });

  const { data: userSummaries } = useQuery({
    queryKey: ["userSummaries"],
    queryFn: getUserSummaries,
    enabled: isAdmin && !isEdit && !presetUserId,
  });

  useEffect(() => {
    if (existingTask) {
      setForm({
        title: existingTask.title ?? "",
        description: existingTask.description ?? "",
        dueDate: existingTask.dueDate ? existingTask.dueDate.slice(0, 10) : "",
        statusId: String(
          STATUSES.find((s) => s.name === existingTask.statusName)?.id ?? ""
        ),
        priorityId: String(
          PRIORITIES.find((p) => p.name === existingTask.priorityName)?.id ?? ""
        ),
        categoryId: String(
          CATEGORIES.find((c) => c.name === existingTask.categoryName)?.id ?? ""
        ),
        assignedToUserId: "",
      });
    }
  }, [existingTask]);

  const mutation = useMutation({
    mutationFn: (payload) =>
      isEdit ? updateTask(id, payload) : createTask(payload),
    onSuccess: (result) => {
      queryClient.invalidateQueries({ queryKey: ["tasks"] });
      if (isEdit) queryClient.invalidateQueries({ queryKey: ["task", id] });
      navigate(`/tasks/${isEdit ? id : result.id}`);
    },
    onError: (err) => {
      setSubmitError(err.message);
    },
  });

  function handleChange(field, value) {
    setForm((prev) => ({ ...prev, [field]: value }));
    setFieldErrors((prev) => ({ ...prev, [field]: undefined }));
  }

  function validate() {
    const errors = {};
    if (!form.title.trim()) errors.title = "Title is required.";
    if (!form.priorityId) errors.priorityId = "Priority is required.";
    if (isAdmin && !isEdit && !presetUserId && !form.assignedToUserId) {
      errors.assignedToUserId = "Please select a user to assign this task to.";
    }
    setFieldErrors(errors);
    return Object.keys(errors).length === 0;
  }

  function handleSubmit(e) {
    e.preventDefault();
    setSubmitError("");
    if (!validate()) return;

    const payload = {
      title: form.title.trim(),
      description: form.description.trim() || null,
      dueDate: form.dueDate || null,
      statusId: form.statusId ? Number(form.statusId) : null,
      priorityId: Number(form.priorityId),
      categoryId: form.categoryId ? Number(form.categoryId) : null,
    };
    if (!isEdit) {
      payload.assignedToUserId = isAdmin
        ? Number(presetUserId || form.assignedToUserId)
        : null;
    }

    mutation.mutate(payload);
  }

  if (isEdit && isLoadingTask) {
    return (
      <PageContainer title="Edit Task">
        <p>Loading task...</p>
      </PageContainer>
    );
  }

  if (isEdit && !isLoadingTask && !existingTask) {
  return (
    <PageContainer title="Edit Task">
      <div className={styles.submitError}>Task not found.</div>
    </PageContainer>
  );
}

  return (
    <PageContainer title={isEdit ? "Edit Task" : "New Task"}>
      <form onSubmit={handleSubmit} className={styles.formLayout}>
        <div className={styles.mainCard}>
          <h2 className={styles.sectionTitle}>Task Details</h2>

          <Input
            id="title"
            label="Title"
            value={form.title}
            onChange={(e) => handleChange("title", e.target.value)}
            error={fieldErrors.title}
          />

          <div className={styles.field}>
            <label className={styles.label} htmlFor="description">
              Description
            </label>
            <textarea
              id="description"
              value={form.description}
              onChange={(e) => handleChange("description", e.target.value)}
              className={styles.textarea}
              rows={8}
            />
          </div>

          {submitError && (
            <div className={styles.submitError}>{submitError}</div>
          )}

          <div className={styles.actions}>
            <Button
              type="submit"
              variant="flat"
              disabled={mutation.isPending}
            >
              {mutation.isPending
                ? "Saving..."
                : isEdit
                ? "Save Changes"
                : "Create Task"}
            </Button>
            <Button variant="secondary" onClick={() => navigate(-1)}>
              Cancel
            </Button>
          </div>
        </div>

        <div className={styles.sideCard}>
          <h2 className={styles.sectionTitle}>
            {isAdmin && !isEdit && !presetUserId
              ? "Scheduling & Assignment"
              : "Scheduling"}
          </h2>

          {isAdmin && !isEdit && !presetUserId && (
            <div className={styles.field}>
              <label className={styles.label}>Assign To</label>
              <Select
                value={form.assignedToUserId}
                onChange={(val) => handleChange("assignedToUserId", val)}
                options={(userSummaries ?? []).map((u) => ({
                  value: u.userId,
                  label: `${u.fullName} (${u.email})`,
                }))}
                placeholder="Select a user..."
              />
              {fieldErrors.assignedToUserId && (
                <span className={styles.error}>
                  {fieldErrors.assignedToUserId}
                </span>
              )}
            </div>
          )}

          {isEdit && (
            <div className={styles.field}>
              <label className={styles.label}>Assigned To</label>
              <div className={styles.readOnlyValue}>
                {existingTask?.assignedToUserName}
              </div>
            </div>
          )}

          <div className={styles.field}>
            <label className={styles.label} htmlFor="dueDate">Due Date</label>
            <input
              id="dueDate"
              type="date"
              value={form.dueDate}
              onChange={(e) => handleChange("dueDate", e.target.value)}
              className={styles.dateInput}
            />
          </div>

          <div className={styles.field}>
            <label className={styles.label}>Status</label>
            <Select
              value={form.statusId}
              onChange={(val) => handleChange("statusId", val)}
              options={STATUSES.map((s) => ({
                value: s.id,
                label: s.name,
              }))}
              placeholder="Default (To Do)"
            />
          </div>

          <div className={styles.field}>
            <label className={styles.label}>Priority</label>
            <Select
              value={form.priorityId}
              onChange={(val) => handleChange("priorityId", val)}
              options={PRIORITIES.map((p) => ({
                value: p.id,
                label: p.name,
              }))}
              placeholder="Select Priority..."
            />
            {fieldErrors.priorityId && (
              <span className={styles.error}>{fieldErrors.priorityId}</span>
            )}
          </div>

          <div className={styles.field}>
            <label className={styles.label}>Category</label>
            <Select
              value={form.categoryId}
              onChange={(val) => handleChange("categoryId", val)}
              options={CATEGORIES.filter(
                (c) => !isAdmin || c.name !== "Personal"
              ).map((c) => ({
                value: c.id,
                label: c.name,
              }))}
              placeholder="Select Category..."
            />
          </div>
        </div>
      </form>
    </PageContainer>
  );
}