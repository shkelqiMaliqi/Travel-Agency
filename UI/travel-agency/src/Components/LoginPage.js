import React, { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { loginUser, saveAuth, verifyMfa } from "../services/api";

const LoginPage = () => {
  const navigate = useNavigate();
  const [formData, setFormData] = useState({ userNameOrEmail: "", password: "" });
  const [mfaData, setMfaData] = useState({ email: "", code: "", demoCode: "", expiresAt: "" });
  const [status, setStatus] = useState({ loading: false, error: "", mfaRequired: false });

  const handleChange = (event) => {
    const { name, value } = event.target;
    setFormData((current) => ({ ...current, [name]: value }));
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setStatus({ loading: true, error: "", mfaRequired: false });

    try {
      const response = await loginUser(formData);
      if (response.requiresMfa) {
        setMfaData({
          email: response.email,
          code: "",
          demoCode: response.mfaCode || "",
          expiresAt: response.mfaExpiresAtUtc || "",
        });
        setStatus({ loading: false, error: "", mfaRequired: true });
        return;
      }
      saveAuth(response);
      navigate("/dashboard");
    } catch (error) {
      setStatus({ loading: false, error: error.message, mfaRequired: false });
      return;
    }

    setStatus({ loading: false, error: "", mfaRequired: false });
  };

  const handleMfaSubmit = async (event) => {
    event.preventDefault();
    setStatus((current) => ({ ...current, loading: true, error: "" }));

    try {
      const response = await verifyMfa({ email: mfaData.email, code: mfaData.code });
      saveAuth(response);
      navigate("/dashboard");
    } catch (error) {
      setStatus((current) => ({ ...current, loading: false, error: error.message }));
      return;
    }

    setStatus({ loading: false, error: "", mfaRequired: false });
  };

  return (
    <div className="auth-layout">
      <aside className="auth-aside">
        <p className="dashboard-kicker">Welcome back</p>
        <h1>Pick up your travel plans where you left off.</h1>
        <p>Sign in to book packages, review reservations, and manage your profile.</p>
      </aside>
      <div>
        <div className="card shadow-sm">
          <div className="card-body p-4">
            <h2 className="mb-3">Welcome back</h2>
            <p className="text-muted">Sign in with your email or username.</p>

            {!status.mfaRequired ? (
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
            ) : (
              <form onSubmit={handleMfaSubmit} className="row g-3">
                <div className="alert alert-warning mb-0">
                  Multi-factor authentication is required for this account.
                  {mfaData.demoCode ? (
                    <>
                      {" "}Demo code: <strong>{mfaData.demoCode}</strong>
                    </>
                  ) : null}
                </div>
                {mfaData.expiresAt ? <div className="text-muted small">Code expires at {new Date(mfaData.expiresAt).toLocaleString()}.</div> : null}
                <div className="col-12">
                  <label htmlFor="mfaCode" className="form-label">
                    MFA code
                  </label>
                  <input
                    id="mfaCode"
                    className="form-control"
                    value={mfaData.code}
                    onChange={(event) => setMfaData((current) => ({ ...current, code: event.target.value }))}
                    required
                  />
                </div>
                {status.error ? <div className="alert alert-danger mb-0">{status.error}</div> : null}
                <div className="col-12 d-grid">
                  <button type="submit" className="btn btn-primary" disabled={status.loading}>
                    {status.loading ? "Verifying..." : "Verify MFA"}
                  </button>
                </div>
              </form>
            )}

            <p className="mt-3 mb-0">
              Don&apos;t have an account? <Link to="/registerpage">Register here</Link>
            </p>
            <p className="mt-2 mb-0">
              Forgot your password? <Link to="/forgot-password">Reset it here</Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default LoginPage;
