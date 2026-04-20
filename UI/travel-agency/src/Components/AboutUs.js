import React from "react";

const AboutUs = () => {
  return (
    <section className="row justify-content-center">
      <div className="col-lg-8">
        <h1 className="mb-3">About the project</h1>
        <p className="lead text-muted">
          Travel Agency is being shaped into a standards-based web services project with a React frontend and an ASP.NET Web API backend.
        </p>
        <div className="card shadow-sm">
          <div className="card-body">
            <p className="mb-2">
              The current implementation focuses on the core requirements first: versioned REST endpoints, Swagger documentation, JWT authentication, entity alignment with the
              database, and user-facing flows for registration, login, destinations, and contact submissions.
            </p>
            <p className="mb-0">
              The next phase can build on this base with authorization hardening, logging, tests, Docker, CI/CD, and the more advanced operational requirements from the PDF.
            </p>
          </div>
        </div>
      </div>
    </section>
  );
};

export default AboutUs;
