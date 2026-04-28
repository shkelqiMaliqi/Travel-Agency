import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { getStoredAuth, getUserProfile, saveAuth, updateUserProfile } from "../services/api";

const initialForm = {
  u_Name: "",
  u_Surname: "",
  u_Email: "",
  u_Username: "",
  u_Phone: "",
};

const Profile = () => {
  const [auth, setAuth] = useState(() => getStoredAuth());
  const [formData, setFormData] = useState(initialForm);
  const [status, setStatus] = useState({ loading: true, saving: false, error: "", success: "" });

  useEffect(() => {
    let active = true;

    if (!auth) {
      setStatus({ loading: false, saving: false, error: "", success: "" });
      return () => {
        active = false;
      };
    }

    getUserProfile(auth.userId)
      .then((profile) => {
        if (active) {
          setFormData({
            u_Name: profile.u_Name ?? profile.U_Name ?? "",
            u_Surname: profile.u_Surname ?? profile.U_Surname ?? "",
            u_Email: profile.u_Email ?? profile.U_Email ?? "",
            u_Username: profile.u_Username ?? profile.U_Username ?? "",
            u_Phone: profile.u_Phone ?? profile.U_Phone ?? "",
          });
          setStatus({ loading: false, saving: false, error: "", success: "" });
        }
      })
      .catch((error) => {
        if (active) {
          setStatus({ loading: false, saving: false, error: error.message, success: "" });
        }
      });

    return () => {
      active = false;
    };
  }, [auth]);

  const handleChange = (event) => {
    const { name, value } = event.target;
    setFormData((current) => ({ ...current, [name]: value }));
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setStatus({ loading: false, saving: true, error: "", success: "" });

    try {
      await updateUserProfile(auth.userId, formData);
      const nextAuth = {
        ...auth,
        name: `${formData.u_Name} ${formData.u_Surname}`.trim(),
        email: formData.u_Email,
      };
      saveAuth(nextAuth);
      setAuth(nextAuth);
      setStatus({ loading: false, saving: false, error: "", success: "Profile updated successfully." });
    } catch (error) {
      setStatus({ loading: false, saving: false, error: error.message, success: "" });
    }
  };

  if (!auth) {
    return (
      <div className="alert alert-warning">
        You are not logged in yet. <Link to="/loginpage">Go to login</Link>.
      </div>
    );
  }

  if (status.loading) {
    return <div className="alert alert-info">Loading profile...</div>;
  }

  return (
    <section className="profile-page">
      <div className="dashboard-header">
        <div>
          <p className="dashboard-kicker">User profile</p>
          <h1>Profile settings</h1>
          <p className="text-muted mb-0">Keep your account details up to date for future travel requests.</p>
        </div>
        <Link to="/dashboard" className="btn btn-outline-primary">
          Back to dashboard
        </Link>
      </div>

      <div className="row g-4">
        <div className="col-lg-8">
          <div className="dashboard-panel">
            <h2>Account details</h2>
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

              <div className="col-12">
                <label htmlFor="u_Phone" className="form-label">
                  Phone
                </label>
                <input id="u_Phone" name="u_Phone" className="form-control" value={formData.u_Phone} onChange={handleChange} />
              </div>

              {status.error ? <div className="alert alert-danger mb-0">{status.error}</div> : null}
              {status.success ? <div className="alert alert-success mb-0">{status.success}</div> : null}

              <div className="col-12 d-grid d-md-block">
                <button type="submit" className="btn btn-primary" disabled={status.saving}>
                  {status.saving ? "Saving profile..." : "Save profile"}
                </button>
              </div>
            </form>
          </div>
        </div>

        <div className="col-lg-4">
          <div className="dashboard-panel">
            <h2>Session</h2>
            <dl className="profile-list">
              <dt>Role</dt>
              <dd className="text-capitalize">{auth.role}</dd>
              <dt>Session expires</dt>
              <dd>{auth.expiresAtUtc ? new Date(auth.expiresAtUtc).toLocaleString() : "Not available"}</dd>
            </dl>
          </div>
        </div>
      </div>
    </section>
  );
};

export default Profile;
