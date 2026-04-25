import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { getPlaces, getStoredAuth } from "../services/api";

const LoggedIn = () => {
  const [auth] = useState(() => getStoredAuth());
  const [places, setPlaces] = useState([]);

  useEffect(() => {
    let active = true;

    if (auth) {
      getPlaces()
        .then((response) => {
          if (active) {
            setPlaces(response.slice(0, 3));
          }
        })
        .catch(() => {
          if (active) {
            setPlaces([]);
          }
        });
    }

    return () => {
      active = false;
    };
  }, [auth]);

  if (!auth) {
    return (
      <div className="alert alert-warning">
        You are not logged in yet. <Link to="/loginpage">Go to login</Link>.
      </div>
    );
  }

  const firstName = auth.name?.split(" ")[0] || auth.name || "Traveler";
  const expiresAt = auth.expiresAtUtc ? new Date(auth.expiresAtUtc).toLocaleString() : "Not available";

  return (
    <section className="user-dashboard">
      <div className="dashboard-header">
        <div>
          <p className="dashboard-kicker">User dashboard</p>
          <h1>Welcome back, {firstName}</h1>
          <p className="text-muted mb-0">Plan your next trip, review your profile, and continue exploring destinations.</p>
        </div>
        <Link to="/destinations" className="btn btn-primary">
          Browse destinations
        </Link>
      </div>

      <div className="row g-4 mb-4">
        <div className="col-md-4">
          <div className="dashboard-stat">
            <span className="stat-label">Saved trips</span>
            <strong>0</strong>
            <small>Ready for your first plan</small>
          </div>
        </div>
        <div className="col-md-4">
          <div className="dashboard-stat">
            <span className="stat-label">Available places</span>
            <strong>{places.length}</strong>
            <small>Featured by the agency</small>
          </div>
        </div>
        <div className="col-md-4">
          <div className="dashboard-stat">
            <span className="stat-label">Account role</span>
            <strong className="text-capitalize">{auth.role}</strong>
            <small>Active user access</small>
          </div>
        </div>
      </div>

      <div className="row g-4">
        <div className="col-lg-8">
          <div className="dashboard-panel">
            <div className="panel-heading">
              <div>
                <h2>Recommended destinations</h2>
                <p className="text-muted mb-0">Start with one of the latest places from the travel catalog.</p>
              </div>
              <Link to="/destinations" className="btn btn-outline-primary btn-sm">
                View all
              </Link>
            </div>

            <div className="destination-list">
              {places.length > 0 ? (
                places.map((place) => (
                  <article className="destination-row" key={place.place_Id ?? place.Place_Id}>
                    <img
                      src={place.place_Url ?? place.Place_Url ?? "https://via.placeholder.com/120x90?text=Trip"}
                      alt={place.place_Name ?? place.Place_Name}
                    />
                    <div>
                      <h3>{place.place_Name ?? place.Place_Name}</h3>
                      <p>{place.place_Description ?? place.Place_Description}</p>
                    </div>
                  </article>
                ))
              ) : (
                <div className="empty-state">
                  No destinations are available yet. Add places in the database to show recommendations here.
                </div>
              )}
            </div>
          </div>
        </div>

        <div className="col-lg-4">
          <div className="dashboard-panel mb-4">
            <h2>Profile</h2>
            <dl className="profile-list">
              <dt>Name</dt>
              <dd>{auth.name}</dd>
              <dt>Email</dt>
              <dd>{auth.email}</dd>
              <dt>Session expires</dt>
              <dd>{expiresAt}</dd>
            </dl>
          </div>

          <div className="dashboard-panel">
            <h2>Quick actions</h2>
            <div className="quick-actions">
              <Link to="/destinations" className="btn btn-outline-primary">
                Explore trips
              </Link>
              <Link to="/contactus" className="btn btn-outline-secondary">
                Contact agency
              </Link>
              <Link to="/aboutus" className="btn btn-outline-secondary">
                About us
              </Link>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
};

export default LoggedIn;
