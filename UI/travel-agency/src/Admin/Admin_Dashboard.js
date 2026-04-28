import React, { useEffect, useState } from "react";
import { Link, Navigate } from "react-router-dom";
import { addPlace, deletePlace, getPlaces, getStoredAuth, updatePlace } from "../services/api";

const initialForm = {
  place_Name: "",
  place_Description: "",
  place_Url: "",
};

const AdminDashboard = () => {
  const [auth] = useState(() => getStoredAuth());
  const [places, setPlaces] = useState([]);
  const [formData, setFormData] = useState(initialForm);
  const [editingId, setEditingId] = useState(null);
  const [status, setStatus] = useState({ loading: false, error: "", success: "" });

  const isAdmin = auth?.role?.toLowerCase() === "admin";

  const loadPlaces = () => {
    getPlaces()
      .then((response) => setPlaces(response))
      .catch((error) => setStatus((current) => ({ ...current, error: error.message })));
  };

  useEffect(() => {
    if (isAdmin) {
      loadPlaces();
    }
  }, [isAdmin]);

  const handleChange = (event) => {
    const { name, value } = event.target;
    setFormData((current) => ({ ...current, [name]: value }));
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setStatus({ loading: true, error: "", success: "" });

    try {
      if (editingId) {
        await updatePlace(editingId, formData);
      } else {
        await addPlace(formData);
      }

      setFormData(initialForm);
      setEditingId(null);
      setStatus({
        loading: false,
        error: "",
        success: editingId ? "Destination updated successfully." : "Destination added successfully.",
      });
      loadPlaces();
    } catch (error) {
      setStatus({ loading: false, error: error.message, success: "" });
    }
  };

  const handleEdit = (place) => {
    setEditingId(place.place_Id ?? place.Place_Id);
    setFormData({
      place_Name: place.place_Name ?? place.Place_Name ?? "",
      place_Description: place.place_Description ?? place.Place_Description ?? "",
      place_Url: place.place_Url ?? place.Place_Url ?? "",
    });
    setStatus({ loading: false, error: "", success: "" });
  };

  const handleCancelEdit = () => {
    setEditingId(null);
    setFormData(initialForm);
    setStatus({ loading: false, error: "", success: "" });
  };

  const handleDelete = async (place) => {
    const placeId = place.place_Id ?? place.Place_Id;
    const placeName = place.place_Name ?? place.Place_Name;
    const confirmed = window.confirm(`Delete ${placeName}? This cannot be undone.`);

    if (!confirmed) {
      return;
    }

    setStatus({ loading: true, error: "", success: "" });

    try {
      await deletePlace(placeId);
      if (editingId === placeId) {
        handleCancelEdit();
      }
      setStatus({ loading: false, error: "", success: "Destination deleted successfully." });
      loadPlaces();
    } catch (error) {
      setStatus({ loading: false, error: error.message, success: "" });
    }
  };

  if (!auth) {
    return <Navigate to="/loginpage" replace />;
  }

  if (!isAdmin) {
    return <Navigate to="/dashboard" replace />;
  }

  return (
    <section className="admin-dashboard">
      <div className="dashboard-header">
        <div>
          <p className="dashboard-kicker">Admin dashboard</p>
          <h1>Manage destinations</h1>
          <p className="text-muted mb-0">Add, update, and remove destinations that customers see on the Destinations page.</p>
        </div>
        <Link to="/destinations" className="btn btn-outline-primary">
          View public page
        </Link>
      </div>

      <div className="row g-4">
        <div className="col-lg-5">
          <div className="dashboard-panel">
            <h2>{editingId ? "Edit destination" : "Add destination"}</h2>
            <form onSubmit={handleSubmit} className="row g-3">
              <div className="col-12">
                <label htmlFor="place_Name" className="form-label">
                  Destination name
                </label>
                <input
                  id="place_Name"
                  name="place_Name"
                  className="form-control"
                  value={formData.place_Name}
                  onChange={handleChange}
                  required
                />
              </div>

              <div className="col-12">
                <label htmlFor="place_Description" className="form-label">
                  Description
                </label>
                <textarea
                  id="place_Description"
                  name="place_Description"
                  className="form-control"
                  rows="5"
                  value={formData.place_Description}
                  onChange={handleChange}
                  required
                />
              </div>

              <div className="col-12">
                <label htmlFor="place_Url" className="form-label">
                  Image URL
                </label>
                <input
                  id="place_Url"
                  name="place_Url"
                  type="url"
                  className="form-control"
                  value={formData.place_Url}
                  onChange={handleChange}
                  placeholder="https://..."
                />
              </div>

              {status.error ? <div className="alert alert-danger mb-0">{status.error}</div> : null}
              {status.success ? <div className="alert alert-success mb-0">{status.success}</div> : null}

              <div className="col-12 d-grid">
                <button type="submit" className="btn btn-primary" disabled={status.loading}>
                  {status.loading ? "Saving destination..." : editingId ? "Save changes" : "Add destination"}
                </button>
              </div>
              {editingId ? (
                <div className="col-12 d-grid">
                  <button type="button" className="btn btn-outline-secondary" onClick={handleCancelEdit} disabled={status.loading}>
                    Cancel edit
                  </button>
                </div>
              ) : null}
            </form>
          </div>
        </div>

        <div className="col-lg-7">
          <div className="dashboard-panel">
            <div className="panel-heading">
              <div>
                <h2>Current destinations</h2>
                <p className="text-muted mb-0">{places.length} destinations in the catalog.</p>
              </div>
            </div>

            <div className="destination-list">
              {places.map((place) => (
                <article className="destination-row" key={place.place_Id ?? place.Place_Id}>
                  <img
                    src={place.place_Url ?? place.Place_Url ?? "https://via.placeholder.com/120x90?text=Trip"}
                    alt={place.place_Name ?? place.Place_Name}
                  />
                  <div>
                    <h3>{place.place_Name ?? place.Place_Name}</h3>
                    <p>{place.place_Description ?? place.Place_Description}</p>
                  </div>
                  <div className="destination-actions">
                    <button type="button" className="btn btn-outline-primary btn-sm" onClick={() => handleEdit(place)}>
                      Edit
                    </button>
                    <button type="button" className="btn btn-outline-danger btn-sm" onClick={() => handleDelete(place)}>
                      Delete
                    </button>
                  </div>
                </article>
              ))}
            </div>
          </div>
        </div>
      </div>
    </section>
  );
};

export default AdminDashboard;
