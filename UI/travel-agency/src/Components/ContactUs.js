import React, { useState } from "react";
import { getStoredAuth, sendContactMessage } from "../services/api";

const initialForm = {
  c_Name: "",
  c_Surname: "",
  c_Email: "",
  c_Subject: "",
  c_Message: "",
  u_Id: null,
};

const ContactUs = () => {
  const [auth] = useState(() => getStoredAuth());
  const [formData, setFormData] = useState(() => {
    if (!auth) {
      return initialForm;
    }

    const [name = "", ...surnameParts] = (auth.name || "").split(" ");
    return {
      ...initialForm,
      c_Name: name,
      c_Surname: surnameParts.join(" "),
      c_Email: auth.email || "",
      u_Id: auth.userId ?? null,
    };
  });
  const [status, setStatus] = useState({ loading: false, error: "", success: "" });

  const handleChange = (event) => {
    const { name, value } = event.target;
    setFormData((current) => ({ ...current, [name]: value }));
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setStatus({ loading: true, error: "", success: "" });

    try {
      await sendContactMessage(formData);
      setStatus({ loading: false, error: "", success: "Your message has been sent to the travel agency staff. We will respond as fast as we can." });
      setFormData(initialForm);
    } catch (error) {
      setStatus({ loading: false, error: error.message, success: "" });
    }
  };

  return (
    <div className="contact-layout">
      <aside className="dashboard-panel contact-info">
        <p className="dashboard-kicker">Support</p>
        <h1>We are here before and after booking.</h1>
        <p>Send a question about a package, destination, reservation status, or special travel request.</p>
        <dl className="profile-list">
          <dt>Typical response</dt>
          <dd>Within one business day</dd>
          <dt>Best for</dt>
          <dd>Booking questions, destination advice, profile help</dd>
        </dl>
      </aside>
      <div>
        <div className="card shadow-sm">
          <div className="card-body p-4">
            <p className="dashboard-kicker">Contact</p>
            <h2>Talk to the travel team</h2>
            <p className="text-muted">Ask about destinations, packages, bookings, or changes to an upcoming trip.</p>

            <form onSubmit={handleSubmit} className="row g-3">
              <div className="col-md-6">
                <label htmlFor="c_Name" className="form-label">
                  Name
                </label>
                <input id="c_Name" name="c_Name" className="form-control" value={formData.c_Name} onChange={handleChange} required />
              </div>
              <div className="col-md-6">
                <label htmlFor="c_Surname" className="form-label">
                  Surname
                </label>
                <input id="c_Surname" name="c_Surname" className="form-control" value={formData.c_Surname} onChange={handleChange} required />
              </div>
              <div className="col-12">
                <label htmlFor="c_Email" className="form-label">
                  Email
                </label>
                <input id="c_Email" name="c_Email" type="email" className="form-control" value={formData.c_Email} onChange={handleChange} required />
              </div>
              <div className="col-12">
                <label htmlFor="c_Subject" className="form-label">
                  Subject
                </label>
                <input id="c_Subject" name="c_Subject" className="form-control" value={formData.c_Subject} onChange={handleChange} required />
              </div>
              <div className="col-12">
                <label htmlFor="c_Message" className="form-label">
                  Message
                </label>
                <textarea id="c_Message" name="c_Message" className="form-control" rows="5" value={formData.c_Message} onChange={handleChange} required />
              </div>

              {status.error ? <div className="alert alert-danger mb-0">{status.error}</div> : null}
              {status.success ? <div className="alert alert-success mb-0">{status.success}</div> : null}

              <div className="col-12 d-grid">
                <button type="submit" className="btn btn-primary" disabled={status.loading}>
                  {status.loading ? "Sending..." : "Send message"}
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ContactUs;
