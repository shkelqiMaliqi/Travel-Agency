import React, { useCallback, useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { createBooking, getPackages, getPlaces, getStoredAuth } from "../services/api";

const valueOf = (item, camel, pascal) => item?.[camel] ?? item?.[pascal];
const money = (value) => Number(value || 0).toLocaleString(undefined, { style: "currency", currency: "EUR" });
const date = (value) => (value ? new Date(value).toLocaleDateString() : "");
const byPackageName = (first, second) =>
  String(valueOf(first, "package_Name", "Package_Name") || "").localeCompare(String(valueOf(second, "package_Name", "Package_Name") || ""), undefined, {
    sensitivity: "base",
  });

const Packages = () => {
  const [auth] = useState(() => getStoredAuth());
  const [packages, setPackages] = useState([]);
  const [places, setPlaces] = useState([]);
  const [filters, setFilters] = useState({ search: "", placeId: "", minPrice: "", maxPrice: "" });
  const [appliedFilters, setAppliedFilters] = useState({ search: "", placeId: "", minPrice: "", maxPrice: "" });
  const [travelersByPackage, setTravelersByPackage] = useState({});
  const [status, setStatus] = useState({ loading: true, error: "", success: "" });

  const loadPackages = useCallback((nextFilters = appliedFilters) => {
    setStatus((current) => ({ ...current, loading: true }));
    getPackages(nextFilters)
      .then((response) => {
        setPackages([...response].sort(byPackageName));
        setStatus({ loading: false, error: "", success: "" });
      })
      .catch((error) => setStatus({ loading: false, error: error.message, success: "" }));
  }, [appliedFilters]);

  useEffect(() => {
    getPlaces().then(setPlaces).catch(() => setPlaces([]));
  }, []);

  useEffect(() => {
    loadPackages();
  }, [loadPackages]);

  const handleFilterChange = (event) => {
    const { name, value } = event.target;
    setFilters((current) => ({ ...current, [name]: value }));
  };

  const clearFilters = () => {
    const emptyFilters = { search: "", placeId: "", minPrice: "", maxPrice: "" };
    setFilters(emptyFilters);
    setAppliedFilters(emptyFilters);
    loadPackages(emptyFilters);
  };

  const submitFilters = (event) => {
    event.preventDefault();

    if (filters.minPrice && filters.maxPrice && Number(filters.minPrice) > Number(filters.maxPrice)) {
      setStatus({ loading: false, error: "Minimum price cannot be greater than maximum price.", success: "" });
      return;
    }

    setAppliedFilters(filters);
    loadPackages(filters);
  };

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

      <form className="dashboard-panel mb-4" onSubmit={submitFilters}>
        <div className="row g-3">
          <div className="col-md-4">
            <label className="form-label" htmlFor="search">Search</label>
            <input id="search" name="search" className="form-control" value={filters.search} onChange={handleFilterChange} />
          </div>
          <div className="col-md-3">
            <label className="form-label" htmlFor="placeId">Destination</label>
            <select id="placeId" name="placeId" className="form-select" value={filters.placeId} onChange={handleFilterChange}>
              <option value="">All destinations</option>
              {places.map((place) => (
                <option key={valueOf(place, "place_Id", "Place_Id")} value={valueOf(place, "place_Id", "Place_Id")}>
                  {valueOf(place, "place_Name", "Place_Name")}
                </option>
              ))}
            </select>
          </div>
          <div className="col-md-2">
            <label className="form-label" htmlFor="minPrice">Min price</label>
            <input id="minPrice" name="minPrice" type="number" min="0" className="form-control" value={filters.minPrice} onChange={handleFilterChange} />
          </div>
          <div className="col-md-2">
            <label className="form-label" htmlFor="maxPrice">Max price</label>
            <input id="maxPrice" name="maxPrice" type="number" min="0" className="form-control" value={filters.maxPrice} onChange={handleFilterChange} />
          </div>
          <div className="col-md-1 d-grid align-items-end">
            <button type="submit" className="btn btn-primary">
              Search
            </button>
          </div>
          <div className="col-md-1 d-grid align-items-end">
            <button type="button" className="btn btn-outline-secondary" onClick={clearFilters}>
              Clear
            </button>
          </div>
        </div>
      </form>

      <div className="row g-4">
        {packages.length === 0 ? (
          <div className="col-12">
            <div className="empty-state">No packages match your filters.</div>
          </div>
        ) : null}
        {packages.map((tripPackage) => {
          const packageId = valueOf(tripPackage, "package_Id", "Package_Id");
          const seats = valueOf(tripPackage, "available_Seats", "Available_Seats");
          const price = valueOf(tripPackage, "price_Per_Person", "Price_Per_Person");
          const isSoldOut = seats < 1;

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
                      <dd>{isSoldOut ? "Sold out" : seats}</dd>
                    </div>
                  </dl>

                  <div className="mt-auto">
                    <Link to={`/packages/${packageId}`} className="btn btn-outline-primary w-100 mb-2">
                      Details
                    </Link>
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
                        <button className="btn btn-primary" disabled={isSoldOut} onClick={() => handleBook(tripPackage)}>
                          {isSoldOut ? "Sold out" : "Book"}
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
