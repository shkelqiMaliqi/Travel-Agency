import React from "react";
import { Link } from "react-router-dom";

const Home = () => {
  return (
    <section className="home-page">
      <div className="home-hero">
        <div className="home-hero-content">
          <p className="dashboard-kicker">Plan with confidence</p>
          <h1>Find your next trip, compare packages, and book with less guesswork.</h1>
          <p>
            Browse hand-picked destinations, check hotel details, review travel dates and seats, then reserve the package that fits your plans.
          </p>
          <div className="hero-actions">
            <Link to="/packages" className="btn btn-primary btn-lg">
              View packages
            </Link>
            <Link to="/destinations" className="btn btn-light btn-lg">
              Explore destinations
            </Link>
          </div>
        </div>
      </div>

      <div className="row g-4">
        <div className="col-md-4">
          <div className="dashboard-panel h-100">
            <div className="card-body">
              <p className="stat-label">Destinations</p>
              <h2 className="h5">Choose by travel style</h2>
              <p className="text-muted mb-0">Find beach escapes, city breaks, historic routes, nature trips, and relaxing resort stays.</p>
            </div>
          </div>
        </div>
        <div className="col-md-4">
          <div className="dashboard-panel h-100">
            <div className="card-body">
              <p className="stat-label">Packages</p>
              <h2 className="h5">See the full trip</h2>
              <p className="text-muted mb-0">Compare destination, hotel, dates, price per person, available seats, and package details.</p>
            </div>
          </div>
        </div>
        <div className="col-md-4">
          <div className="dashboard-panel h-100">
            <div className="card-body">
              <p className="stat-label">Bookings</p>
              <h2 className="h5">Reserve simply</h2>
              <p className="text-muted mb-0">Create an account, choose travelers, book a package, and follow the request from your dashboard.</p>
            </div>
          </div>
        </div>
      </div>

      <div className="dashboard-panel mt-4">
        <div className="panel-heading">
          <div>
            <p className="dashboard-kicker">How it works</p>
            <h2>From idea to reservation</h2>
          </div>
          <Link to="/aboutus" className="btn btn-outline-primary">
            Learn about us
          </Link>
        </div>
        <div className="process-grid">
          <div>
            <strong>1</strong>
            <h3>Explore</h3>
            <p>Search destinations and shortlist the places that match your travel mood.</p>
          </div>
          <div>
            <strong>2</strong>
            <h3>Compare</h3>
            <p>Open package details to understand hotels, dates, activities, and travel notes.</p>
          </div>
          <div>
            <strong>3</strong>
            <h3>Book</h3>
            <p>Log in, choose travelers, submit your booking, and track it in My bookings.</p>
          </div>
        </div>
      </div>
    </section>
  );
};

export default Home;
