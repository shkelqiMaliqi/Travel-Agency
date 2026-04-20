import React, { useEffect, useState } from "react";
import { getPlaces } from "../services/api";

const Destinations = () => {
  const [places, setPlaces] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    let active = true;

    getPlaces()
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
  }, []);

  return (
    <section>
      <div className="mb-4">
        <h1 className="mb-2">Destinations</h1>
        <p className="text-muted">Explore destinations loaded directly from the Travel Agency API.</p>
      </div>

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
