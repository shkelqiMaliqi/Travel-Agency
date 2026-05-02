import React, { useEffect, useState } from "react";
import { Navigate } from "react-router-dom";
import { getMyBookings, getStoredAuth } from "../services/api";

const valueOf = (item, camel, pascal) => item?.[camel] ?? item?.[pascal];
const money = (value) => Number(value || 0).toLocaleString(undefined, { style: "currency", currency: "EUR" });

const MyBookings = () => {
  const [auth] = useState(() => getStoredAuth());
  const [bookings, setBookings] = useState([]);
  const [status, setStatus] = useState({ loading: true, error: "" });

  useEffect(() => {
    let active = true;

    if (auth) {
      getMyBookings()
        .then((response) => {
          if (active) {
            setBookings(response);
            setStatus({ loading: false, error: "" });
          }
        })
        .catch((error) => {
          if (active) {
            setStatus({ loading: false, error: error.message });
          }
        });
    }

    return () => {
      active = false;
    };
  }, [auth]);

  if (!auth) {
    return <Navigate to="/loginpage" replace />;
  }

  return (
    <section>
      <div className="mb-4">
        <p className="dashboard-kicker">Your trips</p>
        <h1>My bookings</h1>
      </div>

      {status.loading ? <div className="alert alert-info">Loading bookings...</div> : null}
      {status.error ? <div className="alert alert-danger">{status.error}</div> : null}

      <div className="dashboard-panel">
        {bookings.length > 0 ? (
          <div className="table-responsive">
            <table className="table align-middle">
              <thead>
                <tr>
                  <th>Package</th>
                  <th>Destination</th>
                  <th>Hotel</th>
                  <th>Travelers</th>
                  <th>Total</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {bookings.map((booking) => (
                  <tr key={valueOf(booking, "booking_Id", "Booking_Id")}>
                    <td>{valueOf(booking, "package_Name", "Package_Name")}</td>
                    <td>{valueOf(booking, "place_Name", "Place_Name")}</td>
                    <td>{valueOf(booking, "hotel_Name", "Hotel_Name")}</td>
                    <td>{valueOf(booking, "travelers", "Travelers")}</td>
                    <td>{money(valueOf(booking, "total_Price", "Total_Price"))}</td>
                    <td>
                      <span className="badge text-bg-primary">{valueOf(booking, "booking_Status", "Booking_Status")}</span>
                    </td>
                  </tr>
                ))}
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
