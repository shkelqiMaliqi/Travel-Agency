import React from "react";
import { Route, Routes } from "react-router-dom";
import "./App.css";
import Navbar from "./NavBar";
import Home from "./Components/Home";
import AboutUs from "./Components/AboutUs";
import ContactUs from "./Components/ContactUs";
import RegisterPage from "./Components/RegisterPage";
import LoginPage from "./Components/LoginPage";
import LoggedIn from "./Components/LoggedIn";
import Destinations from "./Components/Destinations";

function App() {
  return (
    <div className="app-shell">
      <Navbar />
      <main className="container py-4">
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/aboutus" element={<AboutUs />} />
          <Route path="/contactus" element={<ContactUs />} />
          <Route path="/registerpage" element={<RegisterPage />} />
          <Route path="/loginpage" element={<LoginPage />} />
          <Route path="/dashboard" element={<LoggedIn />} />
          <Route path="/profile" element={<LoggedIn />} />
          <Route path="/destinations" element={<Destinations />} />
        </Routes>
      </main>
    </div>
  );
}

export default App;
