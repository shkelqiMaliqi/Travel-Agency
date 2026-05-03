import React, { useEffect, useMemo, useState } from "react";
import { Link, Navigate } from "react-router-dom";
import {
  addHotel,
  addPackage,
  addPlace,
  archiveContactMessage,
  deleteUser,
  deleteContactMessage,
  deleteHotel,
  deletePackage,
  deletePlace,
  getBookings,
  getAdminStats,
  getContactMessages,
  getHotels,
  getPackages,
  getPlaces,
  getStoredAuth,
  getUsers,
  markContactMessageRead,
  updateBookingStatus,
  updateHotel,
  updatePackage,
  updatePlace,
  updateUserProfile,
  updateUserRole,
} from "../services/api";

const initialPlaceForm = { place_Name: "", place_Description: "", place_Url: "" };
const initialHotelForm = { place_Id: "", hotel_Name: "", hotel_Description: "", hotel_Stars: 4, hotel_Url: "" };
const initialPackageForm = {
  place_Id: "",
  hotel_Id: "",
  package_Name: "",
  package_Description: "",
  price_Per_Person: "",
  start_Date: "",
  end_Date: "",
  available_Seats: "",
  package_Url: "",
};
const initialUserForm = {
  u_Name: "",
  u_Surname: "",
  u_Email: "",
  u_Username: "",
  u_Phone: "",
};

const valueOf = (item, camel, pascal) => item?.[camel] ?? item?.[pascal];
const money = (value) => Number(value || 0).toLocaleString(undefined, { style: "currency", currency: "EUR" });

const AdminDashboard = () => {
  const [auth] = useState(() => getStoredAuth());
  const [activeTab, setActiveTab] = useState("destinations");
  const [places, setPlaces] = useState([]);
  const [hotels, setHotels] = useState([]);
  const [packages, setPackages] = useState([]);
  const [bookings, setBookings] = useState([]);
  const [contactMessages, setContactMessages] = useState([]);
  const [users, setUsers] = useState([]);
  const [stats, setStats] = useState(null);
  const [placeForm, setPlaceForm] = useState(initialPlaceForm);
  const [hotelForm, setHotelForm] = useState(initialHotelForm);
  const [packageForm, setPackageForm] = useState(initialPackageForm);
  const [userForm, setUserForm] = useState(initialUserForm);
  const [editingPlaceId, setEditingPlaceId] = useState(null);
  const [editingHotelId, setEditingHotelId] = useState(null);
  const [editingPackageId, setEditingPackageId] = useState(null);
  const [editingUserId, setEditingUserId] = useState(null);
  const [status, setStatus] = useState({ loading: false, error: "", success: "" });

  const isAdmin = auth?.role?.toLowerCase() === "admin";

  const hotelsForPackagePlace = useMemo(
    () => hotels.filter((hotel) => String(valueOf(hotel, "place_Id", "Place_Id")) === String(packageForm.place_Id)),
    [hotels, packageForm.place_Id]
  );

  const loadAll = () => {
    Promise.all([getPlaces(), getHotels(), getPackages(), getBookings(), getContactMessages(), getUsers(), getAdminStats()])
      .then(([placesResponse, hotelsResponse, packagesResponse, bookingsResponse, contactMessagesResponse, usersResponse, statsResponse]) => {
        setPlaces(placesResponse);
        setHotels(hotelsResponse);
        setPackages(packagesResponse);
        setBookings(bookingsResponse);
        setContactMessages(contactMessagesResponse);
        setUsers(usersResponse);
        setStats(statsResponse);
      })
      .catch((error) => setStatus((current) => ({ ...current, error: error.message })));
  };

  useEffect(() => {
    if (isAdmin) {
      loadAll();
    }
  }, [isAdmin]);

  const updateForm = (setter) => (event) => {
    const { name, value } = event.target;
    setter((current) => ({ ...current, [name]: value }));
  };

  const savePlace = async (event) => {
    event.preventDefault();
    setStatus({ loading: true, error: "", success: "" });

    try {
      if (editingPlaceId) {
        await updatePlace(editingPlaceId, placeForm);
      } else {
        await addPlace(placeForm);
      }

      setPlaceForm(initialPlaceForm);
      setEditingPlaceId(null);
      setStatus({ loading: false, error: "", success: "Destination saved successfully." });
      loadAll();
    } catch (error) {
      setStatus({ loading: false, error: error.message, success: "" });
    }
  };

  const saveHotel = async (event) => {
    event.preventDefault();
    setStatus({ loading: true, error: "", success: "" });

    try {
      const payload = { ...hotelForm, place_Id: Number(hotelForm.place_Id), hotel_Stars: Number(hotelForm.hotel_Stars) };
      if (editingHotelId) {
        await updateHotel(editingHotelId, payload);
      } else {
        await addHotel(payload);
      }

      setHotelForm(initialHotelForm);
      setEditingHotelId(null);
      setStatus({ loading: false, error: "", success: "Hotel saved successfully." });
      loadAll();
    } catch (error) {
      setStatus({ loading: false, error: error.message, success: "" });
    }
  };

  const savePackage = async (event) => {
    event.preventDefault();
    setStatus({ loading: true, error: "", success: "" });

    try {
      const payload = {
        ...packageForm,
        place_Id: Number(packageForm.place_Id),
        hotel_Id: Number(packageForm.hotel_Id),
        price_Per_Person: Number(packageForm.price_Per_Person),
        available_Seats: Number(packageForm.available_Seats),
      };

      if (editingPackageId) {
        await updatePackage(editingPackageId, payload);
      } else {
        await addPackage(payload);
      }

      setPackageForm(initialPackageForm);
      setEditingPackageId(null);
      setStatus({ loading: false, error: "", success: "Package saved successfully." });
      loadAll();
    } catch (error) {
      setStatus({ loading: false, error: error.message, success: "" });
    }
  };

  const saveUser = async (event) => {
    event.preventDefault();

    if (!editingUserId) {
      setStatus({ loading: false, error: "Choose a user to edit first.", success: "" });
      return;
    }

    setStatus({ loading: true, error: "", success: "" });

    try {
      await updateUserProfile(editingUserId, userForm);
      setUserForm(initialUserForm);
      setEditingUserId(null);
      setStatus({ loading: false, error: "", success: "User updated successfully." });
      loadAll();
    } catch (error) {
      setStatus({ loading: false, error: error.message, success: "" });
    }
  };

  const editPlace = (place) => {
    setEditingPlaceId(valueOf(place, "place_Id", "Place_Id"));
    setPlaceForm({
      place_Name: valueOf(place, "place_Name", "Place_Name") || "",
      place_Description: valueOf(place, "place_Description", "Place_Description") || "",
      place_Url: valueOf(place, "place_Url", "Place_Url") || "",
    });
  };

  const editHotel = (hotel) => {
    setEditingHotelId(valueOf(hotel, "hotel_Id", "Hotel_Id"));
    setHotelForm({
      place_Id: valueOf(hotel, "place_Id", "Place_Id") || "",
      hotel_Name: valueOf(hotel, "hotel_Name", "Hotel_Name") || "",
      hotel_Description: valueOf(hotel, "hotel_Description", "Hotel_Description") || "",
      hotel_Stars: valueOf(hotel, "hotel_Stars", "Hotel_Stars") || 4,
      hotel_Url: valueOf(hotel, "hotel_Url", "Hotel_Url") || "",
    });
  };

  const editPackage = (tripPackage) => {
    setEditingPackageId(valueOf(tripPackage, "package_Id", "Package_Id"));
    setPackageForm({
      place_Id: valueOf(tripPackage, "place_Id", "Place_Id") || "",
      hotel_Id: valueOf(tripPackage, "hotel_Id", "Hotel_Id") || "",
      package_Name: valueOf(tripPackage, "package_Name", "Package_Name") || "",
      package_Description: valueOf(tripPackage, "package_Description", "Package_Description") || "",
      price_Per_Person: valueOf(tripPackage, "price_Per_Person", "Price_Per_Person") || "",
      start_Date: String(valueOf(tripPackage, "start_Date", "Start_Date") || "").slice(0, 10),
      end_Date: String(valueOf(tripPackage, "end_Date", "End_Date") || "").slice(0, 10),
      available_Seats: valueOf(tripPackage, "available_Seats", "Available_Seats") || "",
      package_Url: valueOf(tripPackage, "package_Url", "Package_Url") || "",
    });
  };

  const editUser = (user) => {
    setEditingUserId(valueOf(user, "u_Id", "U_Id"));
    setUserForm({
      u_Name: valueOf(user, "u_Name", "U_Name") || "",
      u_Surname: valueOf(user, "u_Surname", "U_Surname") || "",
      u_Email: valueOf(user, "u_Email", "U_Email") || "",
      u_Username: valueOf(user, "u_Username", "U_Username") || "",
      u_Phone: valueOf(user, "u_Phone", "U_Phone") || "",
    });
    setActiveTab("users");
  };

  const cancelUserEdit = () => {
    setEditingUserId(null);
    setUserForm(initialUserForm);
    setStatus({ loading: false, error: "", success: "" });
  };

  const removeItem = async (label, action) => {
    if (!window.confirm(`Delete ${label}? This cannot be undone.`)) {
      return;
    }

    setStatus({ loading: true, error: "", success: "" });
    try {
      await action();
      setStatus({ loading: false, error: "", success: `${label} deleted successfully.` });
      loadAll();
    } catch (error) {
      setStatus({ loading: false, error: error.message, success: "" });
    }
  };

  const changeBookingStatus = async (bookingId, nextStatus) => {
    setStatus({ loading: true, error: "", success: "" });
    try {
      await updateBookingStatus(bookingId, nextStatus);
      setStatus({ loading: false, error: "", success: "Booking status updated." });
      loadAll();
    } catch (error) {
      setStatus({ loading: false, error: error.message, success: "" });
    }
  };

  const markMessageRead = async (messageId) => {
    setStatus({ loading: true, error: "", success: "" });
    try {
      await markContactMessageRead(messageId);
      setStatus({ loading: false, error: "", success: "Message marked as read." });
      loadAll();
    } catch (error) {
      setStatus({ loading: false, error: error.message, success: "" });
    }
  };

  const archiveMessage = async (messageId) => {
    setStatus({ loading: true, error: "", success: "" });
    try {
      await archiveContactMessage(messageId);
      setStatus({ loading: false, error: "", success: "Message archived." });
      loadAll();
    } catch (error) {
      setStatus({ loading: false, error: error.message, success: "" });
    }
  };

  const changeUserRole = async (userId, role) => {
    setStatus({ loading: true, error: "", success: "" });
    try {
      await updateUserRole(userId, role);
      setStatus({ loading: false, error: "", success: "User role updated." });
      loadAll();
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
          <h1>Manage travel catalog</h1>
          <p className="text-muted mb-0">Create destinations, hotels, bookable packages, and review customer bookings.</p>
        </div>
        <Link to="/packages" className="btn btn-outline-primary">
          View packages
        </Link>
        <button type="button" className="btn btn-primary" onClick={() => setActiveTab("packages")}>
          Manage packages
        </button>
      </div>

      <div className="admin-tabs mb-4">
        {["destinations", "hotels", "packages", "bookings", "messages", "users"].map((tab) => (
          <button key={tab} type="button" className={`btn ${activeTab === tab ? "btn-primary" : "btn-outline-primary"}`} onClick={() => setActiveTab(tab)}>
            {tab.charAt(0).toUpperCase() + tab.slice(1)}
          </button>
        ))}
      </div>

      {status.error ? <div className="alert alert-danger">{status.error}</div> : null}
      {status.success ? <div className="alert alert-success">{status.success}</div> : null}

      {stats ? (
        <div className="row g-3 mb-4">
          <div className="col-md-2">
            <div className="dashboard-stat">
              <span className="stat-label">Users</span>
              <strong>{stats.usersCount}</strong>
            </div>
          </div>
          <div className="col-md-2">
            <div className="dashboard-stat">
              <span className="stat-label">Bookings</span>
              <strong>{stats.bookingsCount}</strong>
            </div>
          </div>
          <div className="col-md-2">
            <div className="dashboard-stat">
              <span className="stat-label">Pending</span>
              <strong>{stats.pendingBookingsCount}</strong>
            </div>
          </div>
          <div className="col-md-2">
            <div className="dashboard-stat">
              <span className="stat-label">Packages</span>
              <strong>{stats.packagesCount}</strong>
            </div>
          </div>
          <div className="col-md-2">
            <div className="dashboard-stat">
              <span className="stat-label">Sold out</span>
              <strong>{stats.soldOutPackagesCount}</strong>
            </div>
          </div>
          <div className="col-md-2">
            <div className="dashboard-stat">
              <span className="stat-label">Revenue</span>
              <strong>{money(stats.revenue)}</strong>
            </div>
          </div>
        </div>
      ) : null}

      {activeTab === "destinations" ? (
        <div className="row g-4">
          <div className="col-lg-5">
            <div className="dashboard-panel">
              <h2>{editingPlaceId ? "Edit destination" : "Add destination"}</h2>
              <form className="row g-3" onSubmit={savePlace}>
                <Input label="Destination name" name="place_Name" value={placeForm.place_Name} onChange={updateForm(setPlaceForm)} required />
                <Textarea label="Description" name="place_Description" value={placeForm.place_Description} onChange={updateForm(setPlaceForm)} required />
                <Input label="Image URL" name="place_Url" type="url" value={placeForm.place_Url} onChange={updateForm(setPlaceForm)} />
                <SubmitButton loading={status.loading} label={editingPlaceId ? "Save destination" : "Add destination"} />
              </form>
            </div>
          </div>
          <ListPanel title="Current destinations" count={places.length}>
            {places.map((place) => (
              <Row key={valueOf(place, "place_Id", "Place_Id")} image={valueOf(place, "place_Url", "Place_Url")} title={valueOf(place, "place_Name", "Place_Name")} text={valueOf(place, "place_Description", "Place_Description")}>
                <button className="btn btn-outline-primary btn-sm" onClick={() => editPlace(place)}>Edit</button>
                <button className="btn btn-outline-danger btn-sm" onClick={() => removeItem("Destination", () => deletePlace(valueOf(place, "place_Id", "Place_Id")))}>Delete</button>
              </Row>
            ))}
          </ListPanel>
        </div>
      ) : null}

      {activeTab === "hotels" ? (
        <div className="row g-4">
          <div className="col-lg-5">
            <div className="dashboard-panel">
              <h2>{editingHotelId ? "Edit hotel" : "Add hotel"}</h2>
              <form className="row g-3" onSubmit={saveHotel}>
                <Select label="Destination" name="place_Id" value={hotelForm.place_Id} onChange={updateForm(setHotelForm)} required options={places.map((place) => ({ value: valueOf(place, "place_Id", "Place_Id"), label: valueOf(place, "place_Name", "Place_Name") }))} />
                <Input label="Hotel name" name="hotel_Name" value={hotelForm.hotel_Name} onChange={updateForm(setHotelForm)} required />
                <Textarea label="Description" name="hotel_Description" value={hotelForm.hotel_Description} onChange={updateForm(setHotelForm)} required />
                <Input label="Stars" name="hotel_Stars" type="number" min="1" max="5" value={hotelForm.hotel_Stars} onChange={updateForm(setHotelForm)} required />
                <Input label="Image URL" name="hotel_Url" type="url" value={hotelForm.hotel_Url} onChange={updateForm(setHotelForm)} />
                <SubmitButton loading={status.loading} label={editingHotelId ? "Save hotel" : "Add hotel"} />
              </form>
            </div>
          </div>
          <ListPanel title="Current hotels" count={hotels.length}>
            {hotels.map((hotel) => (
              <Row key={valueOf(hotel, "hotel_Id", "Hotel_Id")} image={valueOf(hotel, "hotel_Url", "Hotel_Url")} title={valueOf(hotel, "hotel_Name", "Hotel_Name")} text={`${valueOf(hotel, "place_Name", "Place_Name")} - ${valueOf(hotel, "hotel_Stars", "Hotel_Stars")} stars`}>
                <button className="btn btn-outline-primary btn-sm" onClick={() => editHotel(hotel)}>Edit</button>
                <button className="btn btn-outline-danger btn-sm" onClick={() => removeItem("Hotel", () => deleteHotel(valueOf(hotel, "hotel_Id", "Hotel_Id")))}>Delete</button>
              </Row>
            ))}
          </ListPanel>
        </div>
      ) : null}

      {activeTab === "packages" ? (
        <div className="row g-4">
          <div className="col-lg-5">
            <div className="dashboard-panel">
              <h2>{editingPackageId ? "Edit package" : "Add package"}</h2>
              <form className="row g-3" onSubmit={savePackage}>
                <Select label="Destination" name="place_Id" value={packageForm.place_Id} onChange={updateForm(setPackageForm)} required options={places.map((place) => ({ value: valueOf(place, "place_Id", "Place_Id"), label: valueOf(place, "place_Name", "Place_Name") }))} />
                <Select label="Hotel" name="hotel_Id" value={packageForm.hotel_Id} onChange={updateForm(setPackageForm)} required options={hotelsForPackagePlace.map((hotel) => ({ value: valueOf(hotel, "hotel_Id", "Hotel_Id"), label: valueOf(hotel, "hotel_Name", "Hotel_Name") }))} />
                <Input label="Package name" name="package_Name" value={packageForm.package_Name} onChange={updateForm(setPackageForm)} required />
                <Textarea label="Description" name="package_Description" value={packageForm.package_Description} onChange={updateForm(setPackageForm)} required />
                <Input label="Price per person" name="price_Per_Person" type="number" min="1" step="0.01" value={packageForm.price_Per_Person} onChange={updateForm(setPackageForm)} required />
                <Input label="Start date" name="start_Date" type="date" value={packageForm.start_Date} onChange={updateForm(setPackageForm)} required />
                <Input label="End date" name="end_Date" type="date" value={packageForm.end_Date} onChange={updateForm(setPackageForm)} required />
                <Input label="Available seats" name="available_Seats" type="number" min="0" value={packageForm.available_Seats} onChange={updateForm(setPackageForm)} required />
                <Input label="Image URL" name="package_Url" type="url" value={packageForm.package_Url} onChange={updateForm(setPackageForm)} />
                <SubmitButton loading={status.loading} label={editingPackageId ? "Save package" : "Add package"} />
                {editingPackageId ? (
                  <div className="col-12 d-grid">
                    <button type="button" className="btn btn-outline-secondary" onClick={() => {
                      setEditingPackageId(null);
                      setPackageForm(initialPackageForm);
                    }}>
                      Cancel edit
                    </button>
                  </div>
                ) : null}
              </form>
            </div>
          </div>
          <ListPanel title="Current packages" count={packages.length}>
            {packages.map((tripPackage) => (
              <Row key={valueOf(tripPackage, "package_Id", "Package_Id")} image={valueOf(tripPackage, "package_Url", "Package_Url")} title={valueOf(tripPackage, "package_Name", "Package_Name")} text={`${valueOf(tripPackage, "place_Name", "Place_Name")} - ${valueOf(tripPackage, "hotel_Name", "Hotel_Name")} - ${valueOf(tripPackage, "available_Seats", "Available_Seats")} seats`}>
                <button className="btn btn-outline-primary btn-sm" onClick={() => editPackage(tripPackage)}>Edit</button>
                <button className="btn btn-outline-danger btn-sm" onClick={() => removeItem("Package", () => deletePackage(valueOf(tripPackage, "package_Id", "Package_Id")))}>Delete</button>
              </Row>
            ))}
          </ListPanel>
        </div>
      ) : null}

      {activeTab === "bookings" ? (
        <div className="dashboard-panel">
          <h2>Customer bookings</h2>
          <div className="table-responsive">
            <table className="table align-middle">
              <thead>
                <tr>
                  <th>Package</th>
                  <th>Customer</th>
                  <th>Email</th>
                  <th>Phone</th>
                  <th>Destination</th>
                  <th>Hotel</th>
                  <th>Travelers</th>
                  <th>Status</th>
                  <th>Change status</th>
                </tr>
              </thead>
              <tbody>
                {bookings.map((booking) => {
                  const bookingId = valueOf(booking, "booking_Id", "Booking_Id");
                  return (
                    <tr key={bookingId}>
                      <td>{valueOf(booking, "package_Name", "Package_Name")}</td>
                      <td>{valueOf(booking, "customer_Name", "Customer_Name")}</td>
                      <td>{valueOf(booking, "customer_Email", "Customer_Email")}</td>
                      <td>{valueOf(booking, "customer_Phone", "Customer_Phone") || "-"}</td>
                      <td>{valueOf(booking, "place_Name", "Place_Name")}</td>
                      <td>{valueOf(booking, "hotel_Name", "Hotel_Name")}</td>
                      <td>{valueOf(booking, "travelers", "Travelers")}</td>
                      <td>{valueOf(booking, "booking_Status", "Booking_Status")}</td>
                      <td>
                        <select className="form-select form-select-sm" value={valueOf(booking, "booking_Status", "Booking_Status")} onChange={(event) => changeBookingStatus(bookingId, event.target.value)}>
                          <option>Pending</option>
                          <option>Confirmed</option>
                          <option>Cancelled</option>
                        </select>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      ) : null}

      {activeTab === "users" ? (
        <div className="row g-4">
          <div className="col-lg-5">
            <div className="dashboard-panel">
              <h2>{editingUserId ? "Edit user" : "User editor"}</h2>
              <form className="row g-3" onSubmit={saveUser}>
                <Input label="Name" name="u_Name" value={userForm.u_Name} onChange={updateForm(setUserForm)} required disabled={!editingUserId} />
                <Input label="Surname" name="u_Surname" value={userForm.u_Surname} onChange={updateForm(setUserForm)} required disabled={!editingUserId} />
                <Input label="Email" name="u_Email" type="email" value={userForm.u_Email} onChange={updateForm(setUserForm)} required disabled={!editingUserId} />
                <Input label="Username" name="u_Username" value={userForm.u_Username} onChange={updateForm(setUserForm)} required disabled={!editingUserId} />
                <Input label="Phone" name="u_Phone" value={userForm.u_Phone} onChange={updateForm(setUserForm)} disabled={!editingUserId} />
                <SubmitButton loading={status.loading} label="Save user" />
                {editingUserId ? (
                  <div className="col-12 d-grid">
                    <button type="button" className="btn btn-outline-secondary" onClick={cancelUserEdit}>
                      Cancel edit
                    </button>
                  </div>
                ) : null}
              </form>
            </div>
          </div>

          <div className="col-lg-7">
            <div className="dashboard-panel">
              <h2>All users</h2>
              <div className="table-responsive">
                <table className="table align-middle">
                  <thead>
                    <tr>
                      <th>Name</th>
                      <th>Email</th>
                      <th>Username</th>
                      <th>Phone</th>
                      <th>Role</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {users.map((user) => {
                      const userId = valueOf(user, "u_Id", "U_Id");
                      return (
                        <tr key={userId}>
                          <td>{valueOf(user, "u_Name", "U_Name")} {valueOf(user, "u_Surname", "U_Surname")}</td>
                          <td>{valueOf(user, "u_Email", "U_Email")}</td>
                          <td>{valueOf(user, "u_Username", "U_Username")}</td>
                          <td>{valueOf(user, "u_Phone", "U_Phone") || "-"}</td>
                          <td>
                            <select className="form-select form-select-sm" value={valueOf(user, "u_Type", "U_Type")} onChange={(event) => changeUserRole(userId, event.target.value)}>
                              <option value="user">user</option>
                              <option value="admin">admin</option>
                            </select>
                          </td>
                          <td>
                            <button className="btn btn-outline-primary btn-sm me-2" onClick={() => editUser(user)}>
                              Edit
                            </button>
                            <button className="btn btn-outline-danger btn-sm" onClick={() => removeItem("User", () => deleteUser(userId))}>
                              Delete
                            </button>
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
              {users.length === 0 ? <div className="empty-state">No users found.</div> : null}
            </div>
          </div>
        </div>
      ) : null}

      {activeTab === "messages" ? (
        <div className="dashboard-panel">
          <h2>Contact messages</h2>
          <div className="table-responsive">
            <table className="table align-middle">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Email</th>
                  <th>Subject</th>
                  <th>Message</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {contactMessages.map((message) => {
                  const messageId = valueOf(message, "c_Id", "C_Id");
                  const isRead = valueOf(message, "c_IsRead", "C_IsRead");

                  return (
                    <tr key={messageId}>
                      <td>{valueOf(message, "c_Name", "C_Name")} {valueOf(message, "c_Surname", "C_Surname")}</td>
                      <td>{valueOf(message, "c_Email", "C_Email")}</td>
                      <td>{valueOf(message, "c_Subject", "C_Subject")}</td>
                      <td>{valueOf(message, "c_Message", "C_Message")}</td>
                      <td>{isRead ? "Read" : "Unread"}</td>
                      <td>
                        <button type="button" className="btn btn-outline-primary btn-sm" disabled={isRead} onClick={() => markMessageRead(messageId)}>
                          Mark read
                        </button>
                        <button type="button" className="btn btn-outline-secondary btn-sm ms-2" onClick={() => archiveMessage(messageId)}>
                          Archive
                        </button>
                        <button type="button" className="btn btn-outline-danger btn-sm ms-2" onClick={() => removeItem("Message", () => deleteContactMessage(messageId))}>
                          Delete
                        </button>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
          {contactMessages.length === 0 ? <div className="empty-state">No contact messages yet.</div> : null}
        </div>
      ) : null}
    </section>
  );
};

function Input({ label, ...props }) {
  return (
    <div className="col-12">
      <label className="form-label" htmlFor={props.name}>{label}</label>
      <input id={props.name} className="form-control" {...props} />
    </div>
  );
}

function Textarea({ label, ...props }) {
  return (
    <div className="col-12">
      <label className="form-label" htmlFor={props.name}>{label}</label>
      <textarea id={props.name} className="form-control" rows="4" {...props} />
    </div>
  );
}

function Select({ label, options, ...props }) {
  return (
    <div className="col-12">
      <label className="form-label" htmlFor={props.name}>{label}</label>
      <select id={props.name} className="form-select" {...props}>
        <option value="">Choose...</option>
        {options.map((option) => (
          <option key={option.value} value={option.value}>{option.label}</option>
        ))}
      </select>
    </div>
  );
}

function SubmitButton({ loading, label }) {
  return (
    <div className="col-12 d-grid">
      <button type="submit" className="btn btn-primary" disabled={loading}>{loading ? "Saving..." : label}</button>
    </div>
  );
}

function ListPanel({ title, count, children }) {
  return (
    <div className="col-lg-7">
      <div className="dashboard-panel">
        <div className="panel-heading">
          <div>
            <h2>{title}</h2>
            <p className="text-muted mb-0">{count} records.</p>
          </div>
        </div>
        <div className="destination-list">{count > 0 ? children : <div className="empty-state">No records yet.</div>}</div>
      </div>
    </div>
  );
}

function Row({ image, title, text, children }) {
  return (
    <article className="destination-row">
      <img src={image || "https://via.placeholder.com/120x90?text=Trip"} alt={title} />
      <div>
        <h3>{title}</h3>
        <p>{text}</p>
      </div>
      <div className="destination-actions">{children}</div>
    </article>
  );
}

export default AdminDashboard;
