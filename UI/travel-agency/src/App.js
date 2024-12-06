import React from 'react';
import { Route, Routes } from 'react-router-dom'; 
import Navbar from "./NavBar";
import Home from './Components/Home';
import AboutUs from './Components/AboutUs';
import ContactUs from './Components/ContactUs';
import Profile from './Components/Profile';
import Register from './Components/RegisterPage';
import LoggedIn from './Components/LoginPage';
import LoginPage from './Components/LoginPage';

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

