import React from "react";
import { Link, NavLink, useNavigate } from "react-router-dom";
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
    <nav className="navbar navbar-expand-lg navbar-dark app-navbar">
      <div className="container">
        <Link to="/" className="navbar-brand">
          <span className="brand-mark">TA</span>
          <span>Travel Agency</span>
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
            <CustomLink to="/packages">Packages</CustomLink>
            <CustomLink to="/contactus">Contact</CustomLink>
          </ul>
          <div className="nav-actions">
            {auth ? (
              <>
                {isAdmin ? (
                  <Link to="/admin" className="btn btn-warning btn-sm nav-cta">
                    Admin
                  </Link>
                ) : null}
                <Link to="/dashboard" className="btn btn-light btn-sm nav-cta">
                  Dashboard
                </Link>
                <Link to="/profile" className="btn btn-outline-light btn-sm nav-cta">
                  Profile
                </Link>
                <Link to="/my-bookings" className="btn btn-outline-light btn-sm nav-cta">
                  Bookings
                </Link>
                <button type="button" className="btn btn-outline-light btn-sm nav-cta" onClick={handleLogout}>
                  Logout
                </button>
              </>
            ) : (
              <>
                <Link to="/loginpage" className="btn btn-light btn-sm nav-cta">
                  Login
                </Link>
                <Link to="/registerpage" className="btn btn-outline-light btn-sm nav-cta">
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
      <NavLink to={to} end={to === "/"} className={({ isActive }) => `nav-link${isActive ? " active" : ""}`}>
        {children}
      </NavLink>
    </li>
  );
}
