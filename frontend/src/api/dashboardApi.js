import axiosInstance from "./axiosInstance";

export function getDashboard() {
  return axiosInstance.get("/dashboard").then((res) => res.data);
}

export function getUserSummaries() {
  return axiosInstance.get("/dashboard/users").then((res) => res.data);
}