import React from 'react';
import { Route, Routes } from 'react-router-dom'; 
import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap/dist/js/bootstrap.bundle.min.js';
import Navbar from "/Navbar";
import Home from './components/Home';
import AboutUs from './components/AboutUs';
import ContactUs from './components/ContactUs';
import Profile from './components/Profile';
import Register from './components/RegisterPage';
import LoggedIn from './components/LoggedIn';
import LoginPage from './components/LoginPage';

/*
import { getToken, getUserFromToken } from './Auth';
import TokenManagement from './TokenManagement';
*/
function App() {
  return (
    <>
      <Navbar />
      <div className="container">
        <Routes>
          <Route path="/home" element={<Home />} />
          <Route path="/aboutus" element={<AboutUs />} />
          <Route path="/contactus" element={<ContactUs />} />
          <Route path="/profile" element={<Profile />} />
          <Route path="/register" element={<Register />} />
          <Route path="/loggedIn" element={<LoggedIn />} />
          <Route path="/loginPage" element={<LoginPage />} />


        </Routes>
      </div>
      <footer>
        {}
      </footer>
    </>
  );
}

export default App;

