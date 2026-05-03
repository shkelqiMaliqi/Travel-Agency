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
import Packages from "./Components/Packages";
import PackageDetails from "./Components/PackageDetails";
import MyBookings from "./Components/MyBookings";
import BookingDetails from "./Components/BookingDetails";
import Profile from "./Components/Profile";
import AdminDashboard from "./Admin/Admin_Dashboard";
import ProtectedRoute from "./Components/ProtectedRoute";

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
          <Route
            path="/dashboard"
            element={
              <ProtectedRoute>
                <LoggedIn />
              </ProtectedRoute>
            }
          />
          <Route
            path="/admin"
            element={
              <ProtectedRoute requiredRole="admin">
                <AdminDashboard />
              </ProtectedRoute>
            }
          />
          <Route
            path="/profile"
            element={
              <ProtectedRoute>
                <Profile />
              </ProtectedRoute>
            }
          />
          <Route path="/destinations" element={<Destinations />} />
          <Route path="/packages" element={<Packages />} />
          <Route path="/packages/:id" element={<PackageDetails />} />
          <Route
            path="/my-bookings"
            element={
              <ProtectedRoute>
                <MyBookings />
              </ProtectedRoute>
            }
          />
          <Route
            path="/bookings/:id"
            element={
              <ProtectedRoute>
                <BookingDetails />
              </ProtectedRoute>
            }
          />
        </Routes>
      </main>
    </div>
  );
}

export default App;
