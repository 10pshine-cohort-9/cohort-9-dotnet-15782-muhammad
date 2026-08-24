import axios from "axios";

const axiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

// Attach JWT to every outgoing request, if present
axiosInstance.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Normalize error responses + handle expired/invalid tokens globally
axiosInstance.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem("token");
      localStorage.removeItem("user");
      if (window.location.pathname !== "/login") {
        window.location.href = "/login";
      }
    }

    const data = error.response?.data;
    const firstValidationError = data?.errors
      ? Object.values(data.errors)[0]?.[0]
      : null;
    const message =
      data?.message || firstValidationError || "Something went wrong. Please try again.";

    return Promise.reject({ message, statusCode: error.response?.status });
  }
);

export default axiosInstance;