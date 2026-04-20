import React from "react";
import { Link } from "react-router-dom";

const Home = () => {
  return (
    <section className="py-4">
      <div className="p-5 mb-4 bg-light rounded-3 border">
        <div className="container-fluid py-4">
          <h1 className="display-5 fw-bold">Travel smarter with a real API-backed portal</h1>
          <p className="col-md-8 fs-5 text-muted">
            This project now has a working .NET API foundation, JWT authentication, destination endpoints, and a live React client for the core user flows.
          </p>
          <div className="d-flex gap-3 flex-wrap">
            <Link to="/destinations" className="btn btn-primary btn-lg">
              Explore destinations
            </Link>
            <Link to="/registerpage" className="btn btn-outline-primary btn-lg">
              Create account
            </Link>
          </div>
        </div>
      </div>

      <div className="row g-4">
        <div className="col-md-4">
          <div className="card h-100 shadow-sm">
            <div className="card-body">
              <h2 className="h5">Secure access</h2>
              <p className="text-muted mb-0">JWT-based authentication is now part of the API layer to align with the project requirements.</p>
            </div>
          </div>
        </div>
        <div className="col-md-4">
          <div className="card h-100 shadow-sm">
            <div className="card-body">
              <h2 className="h5">REST endpoints</h2>
              <p className="text-muted mb-0">Versioned REST routes are exposed under <code>/api/v1</code> for users, auth, places, and contact messages.</p>
            </div>
          </div>
        </div>
        <div className="col-md-4">
          <div className="card h-100 shadow-sm">
            <div className="card-body">
              <h2 className="h5">Travel content</h2>
              <p className="text-muted mb-0">Destinations are loaded from the database instead of hardcoded placeholders in the UI.</p>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
};

export default Home;
