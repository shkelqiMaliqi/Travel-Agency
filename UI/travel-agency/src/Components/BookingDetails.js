import React, { useCallback, useEffect, useState } from "react";
import { Link, Navigate, useParams } from "react-router-dom";
import { cancelBooking, getBooking, getStoredAuth } from "../services/api";

const valueOf = (item, camel, pascal) => item?.[camel] ?? item?.[pascal];
const money = (value) => Number(value || 0).toLocaleString(undefined, { style: "currency", currency: "EUR" });
const dateTime = (value) => (value ? new Date(value).toLocaleString() : "");

const BookingDetails = () => {
  const { id } = useParams();
  const [auth] = useState(() => getStoredAuth());
  const [booking, setBooking] = useState(null);
  const [status, setStatus] = useState({ loading: true, error: "", success: "" });

  const loadBooking = useCallback(() => {
    setStatus((current) => ({ ...current, loading: true }));
    getBooking(id)
      .then((response) => {
        setBooking(response);
        setStatus({ loading: false, error: "", success: "" });
      })
      .catch((error) => setStatus({ loading: false, error: error.message, success: "" }));
  }, [id]);

  useEffect(() => {
    loadBooking();
  }, [loadBooking]);

  if (!auth) {
    return <Navigate to="/loginpage" replace />;
  }

  const bookingStatus = valueOf(booking, "booking_Status", "Booking_Status");

  const handleCancel = async () => {
    if (!window.confirm("Cancel this booking?")) {
      return;
    }

    try {
      await cancelBooking(id);
      setStatus({ loading: false, error: "", success: "Booking cancelled." });
      loadBooking();
    } catch (error) {
      setStatus({ loading: false, error: error.message, success: "" });
    }
  };

  if (status.loading) {
    return <div className="alert alert-info">Loading booking...</div>;
  }

  if (!booking && status.error) {
    return <div className="alert alert-danger">{status.error}</div>;
  }

  return (
    <section>
      <div className="dashboard-header">
        <div>
          <p className="dashboard-kicker">Booking details</p>
          <h1>{valueOf(booking, "package_Name", "Package_Name")}</h1>
        </div>
        <Link to="/my-bookings" className="btn btn-outline-primary">
          Back to bookings
        </Link>
      </div>

      {status.error ? <div className="alert alert-danger">{status.error}</div> : null}
      {status.success ? <div className="alert alert-success">{status.success}</div> : null}

      <div className="dashboard-panel">
        <dl className="profile-list">
          <dt>Destination</dt>
          <dd>{valueOf(booking, "place_Name", "Place_Name")}</dd>
          <dt>Hotel</dt>
          <dd>{valueOf(booking, "hotel_Name", "Hotel_Name")}</dd>
          <dt>Customer</dt>
          <dd>{valueOf(booking, "customer_Name", "Customer_Name")}</dd>
          <dt>Email</dt>
          <dd>{valueOf(booking, "customer_Email", "Customer_Email")}</dd>
          <dt>Phone</dt>
          <dd>{valueOf(booking, "customer_Phone", "Customer_Phone") || "-"}</dd>
          <dt>Travelers</dt>
          <dd>{valueOf(booking, "travelers", "Travelers")}</dd>
          <dt>Total price</dt>
          <dd>{money(valueOf(booking, "total_Price", "Total_Price"))}</dd>
          <dt>Status</dt>
          <dd>{bookingStatus}</dd>
          <dt>Booked at</dt>
          <dd>{dateTime(valueOf(booking, "booking_Date", "Booking_Date"))}</dd>
        </dl>

        <button type="button" className="btn btn-outline-danger mt-3" disabled={bookingStatus !== "Pending"} onClick={handleCancel}>
          Cancel booking
        </button>
      </div>
    </section>
  );
};

export default BookingDetails;
