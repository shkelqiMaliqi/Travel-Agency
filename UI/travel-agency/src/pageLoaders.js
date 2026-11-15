import { getPackages, getPlaces } from "./services/api";

// Shared loaders let intent-based prefetching warm the same chunks React.lazy uses.
export const pageLoaders = {
  AboutUs: () => import("./Components/AboutUs"),
  ContactUs: () => import("./Components/ContactUs"),
  RegisterPage: () => import("./Components/RegisterPage"),
  LoginPage: () => import("./Components/LoginPage"),
  ForgotPassword: () => import("./Components/ForgotPassword"),
  LoggedIn: () => import("./Components/LoggedIn"),
  Destinations: () => import("./Components/Destinations"),
  Packages: () => import("./Components/Packages"),
  PackageDetails: () => import("./Components/PackageDetails"),
  MyBookings: () => import("./Components/MyBookings"),
  BookingDetails: () => import("./Components/BookingDetails"),
  Profile: () => import("./Components/Profile"),
  AdminDashboard: () => import("./Admin/Admin_Dashboard"),
};

const routes = {
  "/aboutus": "AboutUs", "/contactus": "ContactUs", "/registerpage": "RegisterPage",
  "/loginpage": "LoginPage", "/forgot-password": "ForgotPassword", "/dashboard": "LoggedIn",
  "/destinations": "Destinations", "/packages": "Packages", "/my-bookings": "MyBookings",
  "/profile": "Profile", "/admin": "AdminDashboard",
};

export function prefetchPage(path) {
  if (navigator.connection?.saveData || /(^|-)2g$/.test(navigator.connection?.effectiveType || "")) return;
  const loader = pageLoaders[routes[path]];
  if (loader) loader().catch(() => {});
  if (path === "/packages") {
    getPackages().catch(() => {});
    getPlaces().catch(() => {});
  } else if (path === "/destinations") getPlaces().catch(() => {});
}

export function prefetchLink(event) {
  const href = event.target.closest("a")?.getAttribute("href");
  if (href) prefetchPage(href);
}
