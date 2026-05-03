import React, { useState } from "react";
import { Link } from "react-router-dom";
import { forgotPassword } from "../services/api";

const initialForm = {
  email: "",
  newPassword: "",
  confirmPassword: "",
};

const ForgotPassword = () => {
  const [formData, setFormData] = useState(initialForm);
  const [status, setStatus] = useState({ loading: false, error: "", success: "" });

  const handleChange = (event) => {
    const { name, value } = event.target;
    setFormData((current) => ({ ...current, [name]: value }));
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setStatus({ loading: true, error: "", success: "" });

    try {
      const response = await forgotPassword(formData);
      setStatus({ loading: false, error: "", success: response.message || "Password reset successfully." });
      setFormData(initialForm);
    } catch (error) {
      setStatus({ loading: false, error: error.message, success: "" });
    }
  };

  return (
    <div className="auth-layout">
      <aside className="auth-aside">
        <p className="dashboard-kicker">Account help</p>
        <h1>Reset your password and get back to planning.</h1>
        <p>Use your account email and choose a fresh password to regain access.</p>
      </aside>
      <div>
        <div className="card shadow-sm">
          <div className="card-body p-4">
            <h2 className="mb-3">Reset password</h2>
            <p className="text-muted">Enter your account email and choose a new password.</p>

            <form onSubmit={handleSubmit} className="row g-3">
              <div className="col-12">
                <label htmlFor="email" className="form-label">
                  Email
                </label>
                <input id="email" name="email" type="email" className="form-control" value={formData.email} onChange={handleChange} required />
              </div>
              <div className="col-12">
                <label htmlFor="newPassword" className="form-label">
                  New password
                </label>
                <input id="newPassword" name="newPassword" type="password" className="form-control" value={formData.newPassword} onChange={handleChange} required />
              </div>
              <div className="col-12">
                <label htmlFor="confirmPassword" className="form-label">
                  Confirm password
                </label>
                <input id="confirmPassword" name="confirmPassword" type="password" className="form-control" value={formData.confirmPassword} onChange={handleChange} required />
              </div>

              {status.error ? <div className="alert alert-danger mb-0">{status.error}</div> : null}
              {status.success ? <div className="alert alert-success mb-0">{status.success}</div> : null}

              <div className="col-12 d-grid">
                <button type="submit" className="btn btn-primary" disabled={status.loading}>
                  {status.loading ? "Resetting..." : "Reset password"}
                </button>
              </div>
            </form>

            <p className="mt-3 mb-0">
              Remembered it? <Link to="/loginpage">Back to login</Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ForgotPassword;
