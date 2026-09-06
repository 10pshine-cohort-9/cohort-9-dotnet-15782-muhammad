import axiosInstance from "./axiosInstance";

export function getTasks({ search, userId } = {}) {
  const params = {};
  if (search) params.search = search;
  if (userId) params.userId = userId;
  return axiosInstance.get("/tasks", { params }).then((res) => res.data);
}

export function getTaskById(id) {
  return axiosInstance.get(`/tasks/${id}`).then((res) => res.data);
}

export function createTask(payload) {
  return axiosInstance.post("/tasks", payload).then((res) => res.data);
}

export function updateTask(id, payload) {
  return axiosInstance.patch(`/tasks/${id}`, payload).then((res) => res.data);
}

export function deleteTask(id) {
  return axiosInstance.delete(`/tasks/${id}`).then((res) => res.data);
}