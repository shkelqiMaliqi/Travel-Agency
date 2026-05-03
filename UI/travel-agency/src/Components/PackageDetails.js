import React, { useCallback, useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { createBooking, getPackage, getStoredAuth } from "../services/api";

const valueOf = (item, camel, pascal) => item?.[camel] ?? item?.[pascal];
const money = (value) => Number(value || 0).toLocaleString(undefined, { style: "currency", currency: "EUR" });
const date = (value) => (value ? new Date(value).toLocaleDateString() : "");
const nightsBetween = (start, end) => {
  if (!start || !end) {
    return "";
  }

  const startDate = new Date(start);
  const endDate = new Date(end);
  const nights = Math.round((endDate - startDate) / (1000 * 60 * 60 * 24));

  return nights > 0 ? `${nights} nights` : "";
};

const destinationGuides = {
  Bali: {
    highlights: ["Ubud rice terraces and jungle views", "Temple visits with cultural context", "Beach time and relaxed resort mornings", "Local markets, cafes, and optional spa time"],
    goodToKnow: "Best for travelers who want culture, nature, and a slower tropical pace. Light clothing, comfortable sandals, and sun protection are useful.",
  },
  Barcelona: {
    highlights: ["Gaudi architecture and Gothic Quarter walks", "Tapas evening in lively local streets", "Mediterranean beach time", "Colorful neighborhoods with easy cafe stops"],
    goodToKnow: "Great for art, food, and walkable city exploring. Book major landmark visits early during busy seasons.",
  },
  Cairo: {
    highlights: ["Pyramids and ancient monument visits", "Museum time for Egyptian history", "Nile evening atmosphere", "Guided transfers for easier sightseeing"],
    goodToKnow: "Ideal for history-focused travelers. Expect warm days, busy streets, and structured sightseeing around major landmarks.",
  },
  "Cape Town": {
    highlights: ["Table Mountain scenery", "Coastal drive viewpoints", "Markets and local food stops", "Beaches, vineyards nearby, and outdoor activities"],
    goodToKnow: "Good for active travelers who like scenery and fresh air. Pack layers because coastal weather can shift during the day.",
  },
  Dubai: {
    highlights: ["Modern skyline and marina views", "Desert safari experience", "Beach and resort downtime", "Shopping, rooftop views, and warm evenings"],
    goodToKnow: "Best for travelers who want comfort, architecture, and sunny weather. Many indoor attractions are useful during hot afternoons.",
  },
  Istanbul: {
    highlights: ["Historic mosques and old city streets", "Grand bazaar-style market wandering", "Bosphorus ferry views", "Food traditions across two continents"],
    goodToKnow: "A strong choice for layered history and food. Comfortable walking shoes help because many highlights are best explored on foot.",
  },
  Kyoto: {
    highlights: ["Temple and shrine visits", "Bamboo paths and garden walks", "Tea tasting and traditional streets", "Quiet mornings in historic districts"],
    goodToKnow: "Best for culture, photography, and slower travel. Start early for peaceful temples and softer light.",
  },
  Maldives: {
    highlights: ["Lagoon villa relaxation", "Snorkeling in clear water", "Beach access and slow mornings", "Boat transfer and resort comfort"],
    goodToKnow: "Perfect for rest, romance, and ocean time. Most days are intentionally relaxed, with optional water activities.",
  },
  Marrakech: {
    highlights: ["Souk tour and craft market colors", "Garden and courtyard visits", "Riad-style stay", "Desert day trip outside the city"],
    goodToKnow: "Best for travelers who enjoy atmosphere, food, and markets. The medina can be busy, so guided time makes the first day easier.",
  },
  "New York": {
    highlights: ["Skyline viewpoint access", "Museum time and Central Park", "Broadway-area energy", "Neighborhood walks and food stops"],
    goodToKnow: "Good for a fast city break. Plan comfortable shoes and leave room for spontaneous neighborhood exploring.",
  },
  Paris: {
    highlights: ["Eiffel Tower and Seine views", "Museum and cafe recommendations", "Montmartre and market walks", "Romantic evenings in central neighborhoods"],
    goodToKnow: "Ideal for first-time visitors, couples, and food lovers. The itinerary leaves time for wandering, cafes, and photos.",
  },
  Rome: {
    highlights: ["Ancient ruins and Colosseum area", "Piazza walks and church art", "Food district time", "Gelato stops and evening strolls"],
    goodToKnow: "Best for history, food, and classic city wandering. Many streets are stone, so comfortable shoes matter.",
  },
  Santorini: {
    highlights: ["Caldera views and village walks", "Sunset dinner experience", "Volcanic beach visit", "Terrace breakfasts and photo stops"],
    goodToKnow: "A scenic choice for couples and slow travel. Expect stairs and bright sun in the most famous village areas.",
  },
  Singapore: {
    highlights: ["Gardens and waterfront skyline", "Cultural district walks", "Local food tour", "Easy metro-connected city exploring"],
    goodToKnow: "Great for clean, efficient city travel with excellent food. Light clothes and rain-friendly planning are helpful.",
  },
  Sydney: {
    highlights: ["Harbor walks and waterfront views", "Beach transfers", "Wildlife visit", "Coastal paths and relaxed cafes"],
    goodToKnow: "Best for travelers who want city comfort with outdoor days. Bring layers for breezy harbor evenings.",
  },
  Tokyo: {
    highlights: ["Shibuya and Shinjuku city energy", "Temple stops and quiet contrast", "Food streets and shopping districts", "Metro guidance for easier exploring"],
    goodToKnow: "Ideal for modern city lovers and food-focused travelers. The itinerary balances guided structure with free evenings.",
  },
  Vancouver: {
    highlights: ["Harbor paths and mountain views", "Forest trails close to the city", "Food market time", "Low-stress outdoor exploring"],
    goodToKnow: "Best for nature lovers who still want city convenience. Waterproof layers are useful, especially outside summer.",
  },
};

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
  const placeName = valueOf(tripPackage, "place_Name", "Place_Name");
  const hotelName = valueOf(tripPackage, "hotel_Name", "Hotel_Name");
  const guide = destinationGuides[placeName] || {
    highlights: ["Comfortable hotel stay", "Destination sightseeing", "Free time for local exploring", "Simple booking and travel planning"],
    goodToKnow: "This package is designed to balance guided activities with enough free time to enjoy the destination at your own pace.",
  };
  const startDate = valueOf(tripPackage, "start_Date", "Start_Date");
  const endDate = valueOf(tripPackage, "end_Date", "End_Date");
  const tripLength = nightsBetween(startDate, endDate);
  const isSoldOut = seats < 1;

  return (
    <section>
      <div className="dashboard-header">
        <div>
          <p className="dashboard-kicker">{placeName}</p>
          <h1>{valueOf(tripPackage, "package_Name", "Package_Name")}</h1>
          <p className="text-muted mb-0">{hotelName}</p>
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
              <div className="package-detail-copy">
                <h2>Trip overview</h2>
                <p>{valueOf(tripPackage, "package_Description", "Package_Description")}</p>

                <h2>About {placeName}</h2>
                <p>{valueOf(tripPackage, "place_Description", "Place_Description")}</p>

                <h2>What you will experience</h2>
                <ul className="detail-list">
                  {guide.highlights.map((highlight) => (
                    <li key={highlight}>{highlight}</li>
                  ))}
                </ul>

                <h2>Hotel stay</h2>
                <p>
                  {hotelName}
                  {valueOf(tripPackage, "hotel_Stars", "Hotel_Stars") ? ` is a ${valueOf(tripPackage, "hotel_Stars", "Hotel_Stars")}-star stay. ` : " "}
                  {valueOf(tripPackage, "hotel_Description", "Hotel_Description")}
                </p>

                <h2>Good to know</h2>
                <p>{guide.goodToKnow}</p>
              </div>
            </div>
          </article>
        </div>

        <div className="col-lg-5">
          <div className="dashboard-panel">
            <h2>Booking summary</h2>
            <dl className="profile-list">
              <dt>Dates</dt>
              <dd>{date(startDate)} - {date(endDate)}</dd>
              <dt>Trip length</dt>
              <dd>{tripLength}</dd>
              <dt>Destination</dt>
              <dd>{placeName}</dd>
              <dt>Hotel</dt>
              <dd>{hotelName}</dd>
              <dt>Price per person</dt>
              <dd>{money(price)}</dd>
              <dt>Available seats</dt>
              <dd>{isSoldOut ? "Sold out" : seats}</dd>
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
                <button type="button" className="btn btn-primary w-100" disabled={isSoldOut} onClick={handleBook}>
                  {isSoldOut ? "Sold out" : "Book this package"}
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
