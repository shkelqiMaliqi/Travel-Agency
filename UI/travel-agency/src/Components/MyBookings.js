import React, { useEffect, useState } from "react";
import { Link, Navigate } from "react-router-dom";
import { cancelBooking, getMyBookings, getStoredAuth } from "../services/api";

const valueOf = (item, camel, pascal) => item?.[camel] ?? item?.[pascal];
const money = (value) => Number(value || 0).toLocaleString(undefined, { style: "currency", currency: "EUR" });
const date = (value) => (value ? new Date(value).toLocaleDateString() : "-");

const MyBookings = () => {
  const [auth] = useState(() => getStoredAuth());
  const [bookings, setBookings] = useState([]);
  const [status, setStatus] = useState({ loading: true, error: "", success: "" });

  const loadBookings = () => {
    setStatus((current) => ({ ...current, loading: true }));
    getMyBookings()
      .then((response) => {
        setBookings(response);
        setStatus({ loading: false, error: "", success: "" });
      })
      .catch((error) => setStatus({ loading: false, error: error.message, success: "" }));
  };

  useEffect(() => {
    let active = true;

    if (auth && active) {
      loadBookings();
    }

    return () => {
      active = false;
    };
  }, [auth]);

  if (!auth) {
    return <Navigate to="/loginpage" replace />;
  }

  const handleCancel = async (bookingId) => {
    if (!window.confirm("Cancel this booking?")) {
      return;
    }

    setStatus({ loading: false, error: "", success: "" });
    try {
      await cancelBooking(bookingId);
      setStatus({ loading: false, error: "", success: "Booking cancelled." });
      loadBookings();
    } catch (error) {
      setStatus({ loading: false, error: error.message, success: "" });
    }
  };

  return (
    <section>
      <div className="mb-4">
        <p className="dashboard-kicker">Your trips</p>
        <h1>My bookings</h1>
      </div>

      {status.loading ? <div className="alert alert-info">Loading bookings...</div> : null}
      {status.error ? <div className="alert alert-danger">{status.error}</div> : null}
      {status.success ? <div className="alert alert-success">{status.success}</div> : null}

      <div className="dashboard-panel">
        {bookings.length > 0 ? (
          <div className="table-responsive">
            <table className="table align-middle">
              <thead>
                <tr>
                  <th>Package</th>
                  <th>Destination</th>
                  <th>Hotel</th>
                  <th>Travel date</th>
                  <th>Travelers</th>
                  <th>Total</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {bookings.map((booking) => {
                  const bookingId = valueOf(booking, "booking_Id", "Booking_Id");
                  const bookingStatus = valueOf(booking, "booking_Status", "Booking_Status");

                  return (
                    <tr key={bookingId}>
                      <td>{valueOf(booking, "package_Name", "Package_Name")}</td>
                      <td>{valueOf(booking, "place_Name", "Place_Name")}</td>
                      <td>{valueOf(booking, "hotel_Name", "Hotel_Name")}</td>
                      <td>{date(valueOf(booking, "travel_Date", "Travel_Date"))}</td>
                      <td>{valueOf(booking, "travelers", "Travelers")}</td>
                      <td>{money(valueOf(booking, "total_Price", "Total_Price"))}</td>
                      <td>
                        <span className="badge text-bg-primary">{bookingStatus}</span>
                      </td>
                      <td>
                        <Link to={`/bookings/${bookingId}`} className="btn btn-outline-primary btn-sm me-2">
                          Details
                        </Link>
                        <button type="button" className="btn btn-outline-danger btn-sm" disabled={bookingStatus !== "Pending"} onClick={() => handleCancel(bookingId)}>
                          Cancel
                        </button>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="empty-state">You do not have bookings yet.</div>
        )}
      </div>
    </section>
  );
};

export default MyBookings;
