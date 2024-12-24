import React from 'react';
import { Route, Routes } from 'react-router-dom'; 
import Navbar from './NavBar';
import Home from './Components/Home';
import AboutUs from './Components/AboutUs';
import ContactUs from './Components/ContactUs';
import Profile from './Components/Profile';
import RegisterPage from './Components/RegisterPage';
import LoginPage from './Components/LoginPage';
import LoggedIn from './Components/LoggedIn';
import Destinations from './Components/Destinations';

function App() {
  return (
    <>
    
      <Navbar /> {/* Import and render Navbar */}
      <div className="container">
      
        <Routes>
        <Route path="/" element={<Home />} /> {/* Default route */}
       
          <Route path="/aboutus" element={<AboutUs />} />
          <Route path="/contactus" element={<ContactUs />} />
          <Route path="/profile" element={<Profile />} />
          <Route path="/registerPage" element={<RegisterPage />} />
          <Route path="/loggedIn" element={<LoggedIn />} />
          <Route path="/loginPage" element={<LoginPage />} />
          <Route path="/destinations" element={<Destinations />} />
          
        </Routes>
      </div>
      <footer>
        {/* Optional footer content */}
      </footer>
    </>
  );
}

export default App;
