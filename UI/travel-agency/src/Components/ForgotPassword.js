import React, { useState } from "react";
import { Link } from "react-router-dom";
import { requestPasswordReset, resetPassword } from "../services/api";

const passwordRulesText = "Password must be at least 8 characters and include uppercase, lowercase, number, and special character.";

const initialForm = {
  email: "",
  resetCode: "",
  newPassword: "",
  confirmPassword: "",
};

function isPasswordValid(password) {
  return (
    password.length >= 8 &&
    /[A-Z]/.test(password) &&
    /[a-z]/.test(password) &&
    /\d/.test(password) &&
    /[^A-Za-z0-9]/.test(password)
  );
}

const ForgotPassword = () => {
  const [formData, setFormData] = useState(initialForm);
  const [generatedCode, setGeneratedCode] = useState("");
  const [status, setStatus] = useState({ loading: false, error: "", success: "" });

  const handleChange = (event) => {
    const { name, value } = event.target;
    setFormData((current) => ({ ...current, [name]: value }));
  };

  const handleRequestCode = async (event) => {
    event.preventDefault();
    setStatus({ loading: true, error: "", success: "" });

    try {
      const response = await requestPasswordReset({ email: formData.email });
      setGeneratedCode(response.resetCode || "");
      setFormData((current) => ({ ...current, resetCode: response.resetCode || "" }));
      setStatus({ loading: false, error: "", success: response.message || "Reset code generated." });
    } catch (error) {
      setStatus({ loading: false, error: error.message, success: "" });
    }
  };

  const handleResetPassword = async (event) => {
    event.preventDefault();

    if (!isPasswordValid(formData.newPassword)) {
      setStatus({ loading: false, error: passwordRulesText, success: "" });
      return;
    }

    setStatus({ loading: true, error: "", success: "" });

    try {
      const response = await resetPassword(formData);
      setStatus({ loading: false, error: "", success: response.message || "Password reset successfully." });
      setFormData(initialForm);
      setGeneratedCode("");
    } catch (error) {
      setStatus({ loading: false, error: error.message, success: "" });
    }
  };

  return (
<<<<<<< HEAD
    <div className="auth-layout">
      <aside className="auth-aside">
        <p className="dashboard-kicker">Account help</p>
        <h1>Reset your password and get back to planning.</h1>
        <p>Use your account email and choose a fresh password to regain access.</p>
      </aside>
      <div>
=======
    <div className="row justify-content-center">
      <div className="col-lg-6">
>>>>>>> f8072e85f68938e07d84cd8fe8cd3d14b1b6d078
        <div className="card shadow-sm">
          <div className="card-body p-4">
            <h2 className="mb-3">Reset password</h2>
            <p className="text-muted">Request a reset code, copy it, then enter the code with your new password.</p>

            <form onSubmit={handleRequestCode} className="row g-3 mb-4">
              <div className="col-12">
                <label htmlFor="email" className="form-label">
                  Account email
                </label>
                <input id="email" name="email" type="email" className="form-control" value={formData.email} onChange={handleChange} required />
              </div>
              <div className="col-12 d-grid">
                <button type="submit" className="btn btn-outline-primary" disabled={status.loading || !formData.email}>
                  {status.loading ? "Generating code..." : "Generate reset code"}
                </button>
              </div>
            </form>

            {generatedCode ? (
              <div className="alert alert-info">
                Reset code: <strong>{generatedCode}</strong>
              </div>
            ) : null}

            <form onSubmit={handleResetPassword} className="row g-3">
              <div className="col-12">
                <label htmlFor="resetCode" className="form-label">
                  Reset code
                </label>
                <input id="resetCode" name="resetCode" className="form-control" value={formData.resetCode} onChange={handleChange} required />
              </div>
              <div className="col-12">
                <label htmlFor="newPassword" className="form-label">
                  New password
                </label>
                <input id="newPassword" name="newPassword" type="password" className="form-control" value={formData.newPassword} onChange={handleChange} required />
                <div className="form-text">{passwordRulesText}</div>
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
