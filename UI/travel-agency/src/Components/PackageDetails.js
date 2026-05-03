import React, { useCallback, useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { createBooking, getPackage, getStoredAuth } from "../services/api";

const valueOf = (item, camel, pascal) => item?.[camel] ?? item?.[pascal];
const money = (value) => Number(value || 0).toLocaleString(undefined, { style: "currency", currency: "EUR" });
const date = (value) => (value ? new Date(value).toLocaleDateString() : "");

const PackageDetails = () => {
  const { id } = useParams();
  const [auth] = useState(() => getStoredAuth());
  const [tripPackage, setTripPackage] = useState(null);
  const [travelers, setTravelers] = useState(1);
  const [status, setStatus] = useState({ loading: true, error: "", success: "" });

  const loadPackage = useCallback(() => {
    setStatus((current) => ({ ...current, loading: true }));
    getPackage(id)
      .then((response) => {
        setTripPackage(response);
        setStatus({ loading: false, error: "", success: "" });
      })
      .catch((error) => setStatus({ loading: false, error: error.message, success: "" }));
  }, [id]);

  useEffect(() => {
    loadPackage();
  }, [loadPackage]);

  const handleBook = async () => {
    if (!auth) {
      return;
    }

    try {
      await createBooking({ package_Id: Number(id), travelers: Number(travelers) });
      setStatus({ loading: false, error: "", success: "Booking created. You can review it in My bookings." });
      loadPackage();
    } catch (error) {
      setStatus({ loading: false, error: error.message, success: "" });
    }
  };

  if (status.loading) {
    return <div className="alert alert-info">Loading package...</div>;
  }

  if (!tripPackage && status.error) {
    return <div className="alert alert-danger">{status.error}</div>;
  }

  const seats = valueOf(tripPackage, "available_Seats", "Available_Seats");
  const price = valueOf(tripPackage, "price_Per_Person", "Price_Per_Person");

  return (
    <section>
      <div className="dashboard-header">
        <div>
          <p className="dashboard-kicker">{valueOf(tripPackage, "place_Name", "Place_Name")}</p>
          <h1>{valueOf(tripPackage, "package_Name", "Package_Name")}</h1>
          <p className="text-muted mb-0">{valueOf(tripPackage, "hotel_Name", "Hotel_Name")}</p>
        </div>
        <Link to="/packages" className="btn btn-outline-primary">
          Back to packages
        </Link>
      </div>

      {status.error ? <div className="alert alert-danger">{status.error}</div> : null}
      {status.success ? <div className="alert alert-success">{status.success}</div> : null}

      <div className="row g-4">
        <div className="col-lg-7">
          <article className="card shadow-sm">
            <img
              src={valueOf(tripPackage, "package_Url", "Package_Url") || "https://via.placeholder.com/900x400?text=Travel+Package"}
              className="card-img-top"
              alt={valueOf(tripPackage, "package_Name", "Package_Name")}
              style={{ maxHeight: "420px", objectFit: "cover" }}
            />
            <div className="card-body">
              <p>{valueOf(tripPackage, "package_Description", "Package_Description")}</p>
            </div>
          </article>
        </div>

        <div className="col-lg-5">
          <div className="dashboard-panel">
            <h2>Booking summary</h2>
            <dl className="profile-list">
              <dt>Dates</dt>
              <dd>{date(valueOf(tripPackage, "start_Date", "Start_Date"))} - {date(valueOf(tripPackage, "end_Date", "End_Date"))}</dd>
              <dt>Price per person</dt>
              <dd>{money(price)}</dd>
              <dt>Available seats</dt>
              <dd>{seats}</dd>
              <dt>Total</dt>
              <dd>{money(Number(price) * Number(travelers || 1))}</dd>
            </dl>

            {auth ? (
              <div className="mt-3">
                <label className="form-label" htmlFor="travelers">Travelers</label>
                <input
                  id="travelers"
                  type="number"
                  min="1"
                  max={Math.max(1, seats)}
                  className="form-control mb-3"
                  value={travelers}
                  onChange={(event) => setTravelers(event.target.value)}
                />
                <button type="button" className="btn btn-primary w-100" disabled={seats < 1} onClick={handleBook}>
                  Book this package
                </button>
              </div>
            ) : (
              <Link to="/loginpage" className="btn btn-primary w-100 mt-3">
                Login to book
              </Link>
            )}
          </div>
        </div>
      </div>
    </section>
  );
};

export default PackageDetails;
