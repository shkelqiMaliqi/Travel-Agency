import React from "react";
import { Navigate } from "react-router-dom";
import { clearAuth, getStoredAuth } from "../services/api";

const ProtectedRoute = ({ children, requiredRole }) => {
  const auth = getStoredAuth();

  if (!auth) {
    return <Navigate to="/loginpage" replace />;
  }

  if (auth.expiresAtUtc && new Date(auth.expiresAtUtc) <= new Date()) {
    clearAuth();
    return <Navigate to="/loginpage" replace />;
  }

  if (requiredRole && auth.role?.toLowerCase() !== requiredRole.toLowerCase()) {
    return <Navigate to="/dashboard" replace />;
  }

  return children;
};

export default ProtectedRoute;
