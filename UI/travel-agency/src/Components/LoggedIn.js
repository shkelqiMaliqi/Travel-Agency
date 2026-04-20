import React from "react";
import { Link } from "react-router-dom";
import { getStoredAuth } from "../services/api";

const LoggedIn = () => {
  const auth = getStoredAuth();

  if (!auth) {
    return (
      <div className="alert alert-warning">
        You are not logged in yet. <Link to="/loginpage">Go to login</Link>.
      </div>
    );
  }

  return (
    <section className="row justify-content-center">
      <div className="col-lg-7">
        <div className="card shadow-sm">
          <div className="card-body p-4">
            <h1 className="h3 mb-3">Dashboard</h1>
            <p className="text-muted">Your account session is stored locally after a successful JWT login.</p>

            <dl className="row mb-0">
              <dt className="col-sm-4">Name</dt>
              <dd className="col-sm-8">{auth.name}</dd>

              <dt className="col-sm-4">Email</dt>
              <dd className="col-sm-8">{auth.email}</dd>

              <dt className="col-sm-4">Role</dt>
              <dd className="col-sm-8 text-capitalize">{auth.role}</dd>

              <dt className="col-sm-4">Expires</dt>
              <dd className="col-sm-8">{new Date(auth.expiresAtUtc).toLocaleString()}</dd>
            </dl>
          </div>
        </div>
      </div>
    </section>
  );
};

export default LoggedIn;
