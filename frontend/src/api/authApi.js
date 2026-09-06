import axiosInstance from "./axiosInstance";

export function register(payload) {
  return axiosInstance.post("/auth/register", payload).then((res) => res.data);
}

export function login(payload) {
  return axiosInstance.post("/auth/login", payload).then((res) => res.data);
}

export function getMe() {
  return axiosInstance.get("/auth/me").then((res) => res.data);
}