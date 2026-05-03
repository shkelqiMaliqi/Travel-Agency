import React, { useCallback, useEffect, useState } from "react";
import { getPlaces } from "../services/api";

const Destinations = () => {
  const [places, setPlaces] = useState([]);
  const [search, setSearch] = useState("");
  const [appliedSearch, setAppliedSearch] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const loadPlaces = useCallback((nextSearch = appliedSearch) => {
    let active = true;

    setLoading(true);
    getPlaces({ search: nextSearch })
      .then((response) => {
        if (active) {
          setPlaces(response);
          setError("");
        }
      })
      .catch((requestError) => {
        if (active) {
          setError(requestError.message);
        }
      })
      .finally(() => {
        if (active) {
          setLoading(false);
        }
      });

    return () => {
      active = false;
    };
  }, [appliedSearch]);

  useEffect(() => loadPlaces(), [loadPlaces]);

  const handleSearch = (event) => {
    event.preventDefault();
    setAppliedSearch(search);
    loadPlaces(search);
  };

  const clearSearch = () => {
    setSearch("");
    setAppliedSearch("");
    loadPlaces("");
  };

  return (
    <section>
      <div className="dashboard-header">
        <div>
          <p className="dashboard-kicker">Travel catalog</p>
          <h1>Destinations</h1>
          <p className="text-muted mb-0">Explore cities, islands, beaches, historic routes, and nature escapes curated for your next trip.</p>
        </div>
      </div>

      <form className="dashboard-panel mb-4" onSubmit={handleSearch}>
        <div className="row g-3">
          <div className="col-md-8">
            <label className="form-label" htmlFor="destinationSearch">Search destinations</label>
            <input id="destinationSearch" className="form-control" value={search} onChange={(event) => setSearch(event.target.value)} />
          </div>
          <div className="col-md-2 d-grid align-items-end">
            <button type="submit" className="btn btn-primary">Search</button>
          </div>
          <div className="col-md-2 d-grid align-items-end">
            <button type="button" className="btn btn-outline-secondary" onClick={clearSearch}>Clear</button>
          </div>
        </div>
      </form>

      {loading ? <div className="alert alert-info">Loading destinations...</div> : null}
      {error ? <div className="alert alert-danger">{error}</div> : null}

      <div className="row g-4">
        {places.map((place) => (
          <div className="col-md-6 col-lg-4" key={place.place_Id ?? place.Place_Id}>
            <article className="card h-100 shadow-sm">
              <img
                src={place.place_Url ?? place.Place_Url ?? "https://via.placeholder.com/600x300?text=Destination"}
                className="card-img-top"
                alt={place.place_Name ?? place.Place_Name}
                style={{ height: "220px", objectFit: "cover" }}
              />
              <div className="card-body">
                <h3 className="h5">{place.place_Name ?? place.Place_Name}</h3>
                <p className="text-muted mb-0">{place.place_Description ?? place.Place_Description}</p>
              </div>
            </article>
          </div>
        ))}
      </div>
    </section>
  );
};

export default Destinations;
