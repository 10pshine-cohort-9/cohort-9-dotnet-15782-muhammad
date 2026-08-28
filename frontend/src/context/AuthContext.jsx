import { createContext, useContext, useState } from "react";
import { login as loginApi, register as registerApi } from "../api/authApi";
import { getUserIdFromToken } from "../utils/jwt";
import { useQueryClient } from "@tanstack/react-query";


const AuthContext = createContext(null);

function loadUserFromStorage() {
  const token = localStorage.getItem("token");
  const userStr = localStorage.getItem("user");
  if (!token || !userStr) return null;

  try {
    const user = JSON.parse(userStr);
    return { ...user, token };
  } catch {
    return null;
  }
}

export function AuthProvider({ children }) {
  const [user, setUser] = useState(loadUserFromStorage);
  const queryClient = useQueryClient();



  function setSession(authResponse) {
    if(!authResponse?.token) {
      throw new Error("Invalid authentication response: missing token.");
    }
    const { token, email, fullName, role } = authResponse;
    const userId = getUserIdFromToken(token);
    const userData = { userId, email, fullName, role };

    localStorage.setItem("token", token);
    localStorage.setItem("user", JSON.stringify(userData));
    setUser({ ...userData, token });
  }

  async function login(credentials) {
    const data = await loginApi(credentials);
    setSession(data);
  }

  async function signup(details) {
    await registerApi(details);
  }

  function logout() {
    localStorage.removeItem("token");
    localStorage.removeItem("user");
    setUser(null);
    queryClient.clear();
  }

  const value = {
    user,
    isAuthenticated: !!user,
    isAdmin: user?.role === "Admin",
    login,
    signup,
    logout,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error("useAuth must be used within AuthProvider");
  return context;
}