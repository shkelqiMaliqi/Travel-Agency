import React, { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { registerUser, saveAuth } from "../services/api";

const initialForm = {
  u_Name: "",
  u_Surname: "",
  u_Email: "",
  u_Username: "",
  u_Phone: "",
  password: "",
  confirmPassword: "",
};

const RegisterPage = () => {
  const navigate = useNavigate();
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
      const response = await registerUser(formData);
      saveAuth(response);
      setStatus({ loading: false, error: "", success: "Registration successful." });
      navigate("/dashboard");
    } catch (error) {
      setStatus({ loading: false, error: error.message, success: "" });
    }
  };

  return (
    <div className="auth-layout auth-layout-wide">
      <aside className="auth-aside">
        <p className="dashboard-kicker">Join Travel Agency</p>
        <h1>Create an account for easier booking.</h1>
        <p>Save your details once, reserve packages faster, and keep all trip requests in one dashboard.</p>
      </aside>
      <div>
        <div className="card shadow-sm">
          <div className="card-body p-4">
            <h2 className="mb-3">Create your account</h2>
            <p className="text-muted">Register to start managing bookings and travel preferences.</p>

            <form onSubmit={handleSubmit} className="row g-3">
              <div className="col-md-6">
                <label htmlFor="u_Name" className="form-label">
                  Name
                </label>
                <input id="u_Name" name="u_Name" className="form-control" value={formData.u_Name} onChange={handleChange} required />
              </div>
              <div className="col-md-6">
                <label htmlFor="u_Surname" className="form-label">
                  Surname
                </label>
                <input id="u_Surname" name="u_Surname" className="form-control" value={formData.u_Surname} onChange={handleChange} required />
              </div>
              <div className="col-md-6">
                <label htmlFor="u_Email" className="form-label">
                  Email
                </label>
                <input id="u_Email" name="u_Email" type="email" className="form-control" value={formData.u_Email} onChange={handleChange} required />
              </div>
              <div className="col-md-6">
                <label htmlFor="u_Username" className="form-label">
                  Username
                </label>
                <input id="u_Username" name="u_Username" className="form-control" value={formData.u_Username} onChange={handleChange} required />
              </div>
              <div className="col-md-6">
                <label htmlFor="u_Phone" className="form-label">
                  Phone
                </label>
                <input id="u_Phone" name="u_Phone" className="form-control" value={formData.u_Phone} onChange={handleChange} />
              </div>
              <div className="col-md-6">
                <label htmlFor="password" className="form-label">
                  Password
                </label>
                <input id="password" name="password" type="password" className="form-control" value={formData.password} onChange={handleChange} required />
              </div>
              <div className="col-md-6">
                <label htmlFor="confirmPassword" className="form-label">
                  Confirm password
                </label>
                <input
                  id="confirmPassword"
                  name="confirmPassword"
                  type="password"
                  className="form-control"
                  value={formData.confirmPassword}
                  onChange={handleChange}
                  required
                />
              </div>

              {status.error ? <div className="alert alert-danger mb-0">{status.error}</div> : null}
              {status.success ? <div className="alert alert-success mb-0">{status.success}</div> : null}

              <div className="col-12 d-grid">
                <button type="submit" className="btn btn-primary" disabled={status.loading}>
                  {status.loading ? "Creating account..." : "Register"}
                </button>
              </div>
            </form>

            <p className="mt-3 mb-0">
              Already have an account? <Link to="/loginpage">Login here</Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default RegisterPage;
