import React, { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { loginUser, saveAuth } from "../services/api";

const LoginPage = () => {
  const navigate = useNavigate();
  const [formData, setFormData] = useState({ userNameOrEmail: "", password: "" });
  const [status, setStatus] = useState({ loading: false, error: "" });

  const handleChange = (event) => {
    const { name, value } = event.target;
    setFormData((current) => ({ ...current, [name]: value }));
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setStatus({ loading: true, error: "" });

    try {
      const response = await loginUser(formData);
      saveAuth(response);
      navigate("/dashboard");
    } catch (error) {
      setStatus({ loading: false, error: error.message });
      return;
    }

    setStatus({ loading: false, error: "" });
  };

  return (
    <div className="row justify-content-center">
      <div className="col-lg-5">
        <div className="card shadow-sm">
          <div className="card-body p-4">
            <h2 className="mb-3">Welcome back</h2>
            <p className="text-muted">Sign in with your email or username.</p>

            <form onSubmit={handleSubmit} className="row g-3">
              <div className="col-12">
                <label htmlFor="userNameOrEmail" className="form-label">
                  Email or username
                </label>
                <input
                  id="userNameOrEmail"
                  name="userNameOrEmail"
                  className="form-control"
                  value={formData.userNameOrEmail}
                  onChange={handleChange}
                  required
                />
              </div>
              <div className="col-12">
                <label htmlFor="password" className="form-label">
                  Password
                </label>
                <input id="password" name="password" type="password" className="form-control" value={formData.password} onChange={handleChange} required />
              </div>

              {status.error ? <div className="alert alert-danger mb-0">{status.error}</div> : null}

              <div className="col-12 d-grid">
                <button type="submit" className="btn btn-primary" disabled={status.loading}>
                  {status.loading ? "Signing in..." : "Login"}
                </button>
              </div>
            </form>

            <p className="mt-3 mb-0">
              Don&apos;t have an account? <Link to="/registerpage">Register here</Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default LoginPage;
