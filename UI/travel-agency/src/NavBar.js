import React from "react";
import { Link, useNavigate } from "react-router-dom";
import { clearAuth, getStoredAuth } from "./services/api";

export default function Navbar() {
  const navigate = useNavigate();
  const auth = getStoredAuth();
  const isAdmin = auth?.role?.toLowerCase() === "admin";

  const handleLogout = () => {
    clearAuth();
    navigate("/loginpage");
  };

  return (
    <nav className="navbar navbar-expand-lg navbar-dark bg-primary shadow-sm">
      <div className="container">
        <Link to="/" className="navbar-brand fw-semibold">
          Travel Agency
        </Link>
        <button
          className="navbar-toggler"
          type="button"
          data-bs-toggle="collapse"
          data-bs-target="#navbarNav"
          aria-controls="navbarNav"
          aria-expanded="false"
          aria-label="Toggle navigation"
        >
          <span className="navbar-toggler-icon" />
        </button>
        <div className="collapse navbar-collapse" id="navbarNav">
          <ul className="navbar-nav me-auto">
            <CustomLink to="/">Home</CustomLink>
            <CustomLink to="/aboutus">About Us</CustomLink>
            <CustomLink to="/destinations">Destinations</CustomLink>
            <CustomLink to="/contactus">Contact</CustomLink>
          </ul>
          <div className="d-flex gap-2">
            {auth ? (
              <>
                {isAdmin ? (
                  <Link to="/admin" className="btn btn-warning btn-sm">
                    Admin
                  </Link>
                ) : null}
                <Link to="/dashboard" className="btn btn-light btn-sm">
                  Dashboard
                </Link>
                <Link to="/profile" className="btn btn-outline-light btn-sm">
                  Profile
                </Link>
                <button type="button" className="btn btn-outline-light btn-sm" onClick={handleLogout}>
                  Logout
                </button>
              </>
            ) : (
              <>
                <Link to="/loginpage" className="btn btn-light btn-sm">
                  Login
                </Link>
                <Link to="/registerpage" className="btn btn-outline-light btn-sm">
                  Register
                </Link>
              </>
            )}
          </div>
        </div>
      </div>
    </nav>
  );
}

function CustomLink({ to, children }) {
  return (
    <li className="nav-item">
      <Link to={to} className="nav-link">
        {children}
      </Link>
    </li>
  );
}
