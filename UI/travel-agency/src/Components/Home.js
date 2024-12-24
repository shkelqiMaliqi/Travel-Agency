import React from 'react';
//import 'bootstrap/dist/css/bootstrap.min.css';

const Home = () => {
  return (
    <div className='mainDiv'>
      
      {/* Hero Section */}
      <header className="bg-primary text-white text-center py-5">
        <div className="container">
          <h1>Explore the World with TravelAgency</h1>
          <p className="lead">
            Your journey starts here. Find the perfect destination for your next adventure.
          </p>
          <a href="/destinations" className="btn btn-warning btn-lg">
            Discover More
          </a>
        </div>
      </header>

      {/* Featured Destinations */}
      <section id="destinations" className="py-5">
        <div className="container">
          <h2 className="text-center mb-4">Featured Destinations</h2>
          <div className="row">
            <div className="col-md-4 mb-4">
              <div className="card">
                <img
                  src="https://via.placeholder.com/300"
                  className="card-img-top"
                  alt="Destination 1"
                />
                <div className="card-body">
                  <h5 className="card-title">Maldives</h5>
                  <p className="card-text">
                    Crystal clear waters and white sandy beaches await you.
                  </p>
                </div>
              </div>
            </div>
            <div className="col-md-4 mb-4">
              <div className="card">
                <img
                  src="https://via.placeholder.com/300"
                  className="card-img-top"
                  alt="Destination 2"
                />
                <div className="card-body">
                  <h5 className="card-title">Paris</h5>
                  <p className="card-text">
                    Experience the romance and charm of the City of Light.
                  </p>
                </div>
              </div>
            </div>
            <div className="col-md-4 mb-4">
              <div className="card">
                <img
                  src="https://via.placeholder.com/300"
                  className="card-img-top"
                  alt="Destination 3"
                />
                <div className="card-body">
                  <h5 className="card-title">Bali</h5>
                  <p className="card-text">
                    Discover the beauty and culture of this tropical paradise.
                  </p>
                </div>
              </div>
            </div>

            <div className="col-md-4 mb-4">
              <div className="card">
                <img
                  src="https://via.placeholder.com/300"
                  className="card-img-top"
                  alt="Destination 4"
                />
                <div className="card-body">
                  <h5 className="card-title">Kosovo</h5>
                  <p className="card-text">
                    Experience the romance and charm of the City of Light.
                  </p>
                </div>
              </div>
            </div>

            <div className="col-md-4 mb-4">
              <div className="card">
                <img
                  src="https://via.placeholder.com/300"
                  className="card-img-top"
                  alt="Destination 2"
                />
                <div className="card-body">
                  <h5 className="card-title">Kosovo 5</h5>
                  <p className="card-text">
                    Experience the romance and charm of the City of Light.
                  </p>
                </div>
              </div>
            </div>

            <div className="col-md-4 mb-4">
              <div className="card">
                <img
                  src="https://via.placeholder.com/300"
                  className="card-img-top"
                  alt="Destination 2"
                />
                <div className="card-body">
                  <h5 className="card-title">Kosovo 6</h5>
                  <p className="card-text">
                    Experience the romance and charm of the City of Light.
                  </p>
                </div>
              </div>
            </div>


          </div>
        </div>
      </section>

      {/* Footer */}
      <footer className="bg-dark text-white text-center py-3">
        <div className="container">
          <p className="mb-0">&copy; 2024 TravelAgency. All Rights Reserved.</p>
        </div>
      </footer>
    </div>
  );
};

export default Home;
