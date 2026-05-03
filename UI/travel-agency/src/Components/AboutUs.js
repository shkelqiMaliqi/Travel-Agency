import React from "react";

const AboutUs = () => {
  const services = [
    {
      title: "Curated destinations",
      text: "We select destinations that offer a strong mix of culture, scenery, comfort, and memorable activities.",
    },
    {
      title: "Ready-made packages",
      text: "Each package combines a destination, hotel, travel dates, available seats, and clear pricing in one place.",
    },
    {
      title: "Simple booking",
      text: "Travelers can browse trips, compare details, create an account, and book directly through the portal.",
    },
  ];

  const values = [
    "Clear information before booking",
    "Comfortable hotels and practical itineraries",
    "Useful destination details for confident choices",
    "Support before and after reservations",
  ];

  return (
    <section>
      <div className="dashboard-header">
        <div>
          <p className="dashboard-kicker">About us</p>
          <h1>Your travel planning partner</h1>
          <p className="text-muted mb-0">
            Travel Agency helps travelers discover destinations, compare packages, and book memorable trips with confidence.
          </p>
        </div>
      </div>

      <div className="row g-4 mb-4">
        <div className="col-lg-8">
          <div className="dashboard-panel h-100">
            <h2>Who we are</h2>
            <p className="text-muted mb-3">
              We are a travel agency built around simple, transparent trip planning. Our goal is to make it easier for people to choose where to go, understand what each package includes, and reserve a trip without confusion.
            </p>
            <p className="text-muted mb-0">
              From beach escapes and cultural city breaks to nature-focused adventures, every destination is presented with practical details, hotel information, dates, prices, and available seats so travelers can make the right choice for their budget and travel style.
            </p>
          </div>
        </div>

        <div className="col-lg-4">
          <div className="dashboard-panel h-100">
            <h2>Our mission</h2>
            <p className="text-muted mb-0">
              To connect travelers with well-organized trips that feel exciting, reliable, and easy to book from the first search to the final reservation.
            </p>
          </div>
        </div>
      </div>

      <div className="row g-4 mb-4">
        {services.map((service) => (
          <div className="col-md-4" key={service.title}>
            <article className="dashboard-panel h-100">
              <h2>{service.title}</h2>
              <p className="text-muted mb-0">{service.text}</p>
            </article>
          </div>
        ))}
      </div>

      <div className="row g-4">
        <div className="col-lg-6">
          <div className="dashboard-panel h-100">
            <h2>How our booking works</h2>
            <ol className="text-muted mb-0">
              <li>Explore destinations and read the travel details.</li>
              <li>Compare packages by hotel, dates, price, and seats.</li>
              <li>Create an account or log in to book your selected trip.</li>
              <li>Review your reservations in My bookings.</li>
            </ol>
          </div>
        </div>

        <div className="col-lg-6">
          <div className="dashboard-panel h-100">
            <h2>What we care about</h2>
            <ul className="text-muted mb-0">
              {values.map((value) => (
                <li key={value}>{value}</li>
              ))}
            </ul>
          </div>
        </div>
      </div>
    </section>
  );
};

export default AboutUs;
