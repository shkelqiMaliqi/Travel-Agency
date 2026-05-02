import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { createBooking, getPackages, getStoredAuth } from "../services/api";

const valueOf = (item, camel, pascal) => item?.[camel] ?? item?.[pascal];
const money = (value) => Number(value || 0).toLocaleString(undefined, { style: "currency", currency: "EUR" });
const date = (value) => (value ? new Date(value).toLocaleDateString() : "");

const Packages = () => {
  const [auth] = useState(() => getStoredAuth());
  const [packages, setPackages] = useState([]);
  const [travelersByPackage, setTravelersByPackage] = useState({});
  const [status, setStatus] = useState({ loading: true, error: "", success: "" });

  const loadPackages = () => {
    setStatus((current) => ({ ...current, loading: true }));
    getPackages()
      .then((response) => {
        setPackages(response);
        setStatus({ loading: false, error: "", success: "" });
      })
      .catch((error) => setStatus({ loading: false, error: error.message, success: "" }));
  };

  useEffect(() => {
    loadPackages();
  }, []);

  const handleTravelerChange = (packageId, value) => {
    setTravelersByPackage((current) => ({ ...current, [packageId]: value }));
  };

  const handleBook = async (tripPackage) => {
    if (!auth) {
      return;
    }

    const packageId = valueOf(tripPackage, "package_Id", "Package_Id");
    const travelers = Number(travelersByPackage[packageId] || 1);
    setStatus({ loading: false, error: "", success: "" });

    try {
      await createBooking({ package_Id: packageId, travelers });
      setStatus({ loading: false, error: "", success: "Booking created. You can review it in My bookings." });
      loadPackages();
    } catch (error) {
      setStatus({ loading: false, error: error.message, success: "" });
    }
  };

  if (status.loading) {
    return <div className="alert alert-info">Loading packages...</div>;
  }

  return (
    <section>
      <div className="dashboard-header">
        <div>
          <p className="dashboard-kicker">Bookable trips</p>
          <h1>Travel packages</h1>
          <p className="text-muted mb-0">Choose a package that includes a destination, hotel, dates, and available seats.</p>
        </div>
        {auth ? (
          <Link to="/my-bookings" className="btn btn-outline-primary">
            My bookings
          </Link>
        ) : null}
      </div>

      {status.error ? <div className="alert alert-danger">{status.error}</div> : null}
      {status.success ? <div className="alert alert-success">{status.success}</div> : null}

      <div className="row g-4">
        {packages.map((tripPackage) => {
          const packageId = valueOf(tripPackage, "package_Id", "Package_Id");
          const seats = valueOf(tripPackage, "available_Seats", "Available_Seats");
          const price = valueOf(tripPackage, "price_Per_Person", "Price_Per_Person");

          return (
            <div className="col-md-6 col-lg-4" key={packageId}>
              <article className="card h-100 shadow-sm package-card">
                <img
                  src={valueOf(tripPackage, "package_Url", "Package_Url") || "https://via.placeholder.com/600x300?text=Travel+Package"}
                  className="card-img-top"
                  alt={valueOf(tripPackage, "package_Name", "Package_Name")}
                />
                <div className="card-body d-flex flex-column">
                  <h2 className="h5">{valueOf(tripPackage, "package_Name", "Package_Name")}</h2>
                  <p className="text-muted mb-2">
                    {valueOf(tripPackage, "place_Name", "Place_Name")} - {valueOf(tripPackage, "hotel_Name", "Hotel_Name")}
                  </p>
                  <p>{valueOf(tripPackage, "package_Description", "Package_Description")}</p>
                  <dl className="package-facts">
                    <div>
                      <dt>Dates</dt>
                      <dd>
                        {date(valueOf(tripPackage, "start_Date", "Start_Date"))} - {date(valueOf(tripPackage, "end_Date", "End_Date"))}
                      </dd>
                    </div>
                    <div>
                      <dt>Price</dt>
                      <dd>{money(price)} / person</dd>
                    </div>
                    <div>
                      <dt>Seats</dt>
                      <dd>{seats}</dd>
                    </div>
                  </dl>

                  <div className="mt-auto">
                    {auth ? (
                      <div className="booking-controls">
                        <input
                          type="number"
                          min="1"
                          max={Math.max(1, seats)}
                          className="form-control"
                          value={travelersByPackage[packageId] || 1}
                          onChange={(event) => handleTravelerChange(packageId, event.target.value)}
                          aria-label="Travelers"
                        />
                        <button className="btn btn-primary" disabled={seats < 1} onClick={() => handleBook(tripPackage)}>
                          Book
                        </button>
                      </div>
                    ) : (
                      <Link to="/loginpage" className="btn btn-primary w-100">
                        Login to book
                      </Link>
                    )}
                  </div>
                </div>
              </article>
            </div>
          );
        })}
      </div>
    </section>
  );
};

export default Packages;
