import React, { lazy, Suspense } from "react";
import { Route, Routes } from "react-router-dom";
import "./App.css";
import Navbar from "./NavBar";
import Home from "./Components/Home";
import { pageLoaders, prefetchLink } from "./pageLoaders";
import ProtectedRoute from "./Components/ProtectedRoute";

const AboutUs = lazy(pageLoaders.AboutUs);
const ContactUs = lazy(pageLoaders.ContactUs);
const RegisterPage = lazy(pageLoaders.RegisterPage);
const LoginPage = lazy(pageLoaders.LoginPage);
const ForgotPassword = lazy(pageLoaders.ForgotPassword);
const LoggedIn = lazy(pageLoaders.LoggedIn);
const Destinations = lazy(pageLoaders.Destinations);
const Packages = lazy(pageLoaders.Packages);
const PackageDetails = lazy(pageLoaders.PackageDetails);
const MyBookings = lazy(pageLoaders.MyBookings);
const BookingDetails = lazy(pageLoaders.BookingDetails);
const Profile = lazy(pageLoaders.Profile);
const AdminDashboard = lazy(pageLoaders.AdminDashboard);

function App() {
  return (
    <div className="app-shell" onPointerOver={prefetchLink} onFocus={prefetchLink}>
      <Navbar />
      <main className="container py-4">
        <Suspense fallback={<div role="status" className="py-4">Loading page…</div>}>
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/aboutus" element={<AboutUs />} />
          <Route path="/contactus" element={<ContactUs />} />
          <Route path="/registerpage" element={<RegisterPage />} />
          <Route path="/loginpage" element={<LoginPage />} />
          <Route path="/forgot-password" element={<ForgotPassword />} />
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
        </Suspense>
      </main>
    </div>
  );
}

export default App;
