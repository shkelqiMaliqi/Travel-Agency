const API_BASE_URL = process.env.REACT_APP_API_URL || "http://localhost:5132/api/v1";
const AUTH_STORAGE_KEY = "travelAgency.auth";

export function getStoredAuth() {
  const raw = localStorage.getItem(AUTH_STORAGE_KEY);

  if (!raw) {
    return null;
  }

  try {
    return JSON.parse(raw);
  } catch (error) {
    localStorage.removeItem(AUTH_STORAGE_KEY);
    return null;
  }
}

export function saveAuth(authResponse) {
  localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(authResponse));
}

export function clearAuth() {
  localStorage.removeItem(AUTH_STORAGE_KEY);
}

export function getAuthToken() {
  return getStoredAuth()?.token || "";
}

async function request(path, options = {}) {
  const { auth = false, ...fetchOptions } = options;
  const token = auth ? getAuthToken() : "";

  const response = await fetch(`${API_BASE_URL}${path}`, {
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(fetchOptions.headers || {}),
    },
    ...fetchOptions,
  });

  const isJson = response.headers.get("content-type")?.includes("application/json");
  const payload = isJson ? await response.json() : await response.text();

  if (!response.ok) {
    const message =
      (typeof payload === "object" && payload?.message) ||
      (typeof payload === "string" && payload) ||
      "Request failed.";

    throw new Error(message);
  }

  return payload;
}

export async function registerUser(formData) {
  return request("/auth/register", {
    method: "POST",
    body: JSON.stringify(formData),
  });
}

export async function loginUser(formData) {
  return request("/auth/login", {
    method: "POST",
    body: JSON.stringify(formData),
  });
}

export async function getPlaces() {
  return request("/places");
}

export async function addPlace(formData) {
  return request("/places", {
    auth: true,
    method: "POST",
    body: JSON.stringify(formData),
  });
}

export async function updatePlace(id, formData) {
  return request(`/places/${id}`, {
    auth: true,
    method: "PUT",
    body: JSON.stringify(formData),
  });
}

export async function deletePlace(id) {
  return request(`/places/${id}`, {
    auth: true,
    method: "DELETE",
  });
}

export async function getHotels(placeId) {
  const query = placeId ? `?placeId=${placeId}` : "";
  return request(`/hotels${query}`);
}

export async function addHotel(formData) {
  return request("/hotels", {
    auth: true,
    method: "POST",
    body: JSON.stringify(formData),
  });
}

export async function updateHotel(id, formData) {
  return request(`/hotels/${id}`, {
    auth: true,
    method: "PUT",
    body: JSON.stringify(formData),
  });
}

export async function deleteHotel(id) {
  return request(`/hotels/${id}`, {
    auth: true,
    method: "DELETE",
  });
}

export async function getPackages() {
  return request("/packages");
}

export async function addPackage(formData) {
  return request("/packages", {
    auth: true,
    method: "POST",
    body: JSON.stringify(formData),
  });
}

export async function updatePackage(id, formData) {
  return request(`/packages/${id}`, {
    auth: true,
    method: "PUT",
    body: JSON.stringify(formData),
  });
}

export async function deletePackage(id) {
  return request(`/packages/${id}`, {
    auth: true,
    method: "DELETE",
  });
}

export async function createBooking(formData) {
  return request("/bookings", {
    auth: true,
    method: "POST",
    body: JSON.stringify(formData),
  });
}

export async function getMyBookings() {
  return request("/bookings/mine", {
    auth: true,
  });
}

export async function getBookings() {
  return request("/bookings", {
    auth: true,
  });
}

export async function updateBookingStatus(id, status) {
  return request(`/bookings/${id}/status`, {
    auth: true,
    method: "PUT",
    body: JSON.stringify(status),
  });
}

export async function getUserProfile(id) {
  return request(`/users/${id}`, {
    auth: true,
  });
}

export async function updateUserProfile(id, formData) {
  return request(`/users/${id}`, {
    auth: true,
    method: "PUT",
    body: JSON.stringify(formData),
  });
}

export async function sendContactMessage(formData) {
  return request("/contact", {
    method: "POST",
    body: JSON.stringify(formData),
  });
}
