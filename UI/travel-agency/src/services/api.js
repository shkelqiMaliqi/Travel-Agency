const API_BASE_URL = process.env.REACT_APP_API_URL || "http://localhost:5132/api/v1";
const AUTH_STORAGE_KEY = "travelAgency.auth";
const CACHE_TTL_MS = 20000;
const responseCache = new Map();
const pendingRequests = new Map();

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
  const method = (fetchOptions.method || "GET").toUpperCase();
  const canCache = method === "GET";
  const cacheKey = canCache ? `${token || "public"}:${path}` : "";

  if (canCache) {
    const cached = responseCache.get(cacheKey);
    if (cached && Date.now() - cached.createdAt < CACHE_TTL_MS) {
      return cached.payload;
    }

    if (pendingRequests.has(cacheKey)) {
      return pendingRequests.get(cacheKey);
    }
  }

  const requestPromise = fetch(`${API_BASE_URL}${path}`, {
      headers: {
        "Content-Type": "application/json",
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
        ...(fetchOptions.headers || {}),
      },
      ...fetchOptions,
    })
    .then(async (response) => {
      const isJson = response.headers.get("content-type")?.includes("application/json");
      const payload = isJson ? await response.json() : await response.text();

      if (!response.ok) {
        if (auth && response.status === 401) {
          clearAuth();
        }

        const message =
          (typeof payload === "object" && payload?.message) ||
          (typeof payload === "string" && payload) ||
          "Request failed.";

        throw new Error(message);
      }

      const result = payload && typeof payload === "object" && "success" in payload && "data" in payload ? payload.data : payload;

      if (canCache) {
        responseCache.set(cacheKey, { payload: result, createdAt: Date.now() });
      } else {
        responseCache.clear();
      }

      return result;
    })
    .finally(() => {
      if (canCache) {
        pendingRequests.delete(cacheKey);
      }
    });

  if (canCache) {
    pendingRequests.set(cacheKey, requestPromise);
  }

  return requestPromise;
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

export async function verifyMfa(formData) {
  return request("/auth/verify-mfa", {
    method: "POST",
    body: JSON.stringify(formData),
  });
}

export async function requestPasswordReset(formData) {
  return request("/auth/forgot-password", {
    method: "POST",
    body: JSON.stringify(formData),
  });
}

export async function resetPassword(formData) {
  return request("/auth/reset-password", {
    method: "POST",
    body: JSON.stringify(formData),
  });
}

export async function getPlaces(filters = {}) {
  const params = new URLSearchParams();

  Object.entries(filters).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== "") {
      params.set(key, value);
    }
  });

  const query = params.toString() ? `?${params.toString()}` : "";
  return request(`/places${query}`);
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

export async function getPackages(filters = {}) {
  const params = new URLSearchParams();

  Object.entries(filters).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== "") {
      params.set(key, value);
    }
  });

  const query = params.toString() ? `?${params.toString()}` : "";
  return request(`/packages${query}`);
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

export async function getPackage(id) {
  return request(`/packages/${id}`);
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

export async function getBooking(id) {
  return request(`/bookings/${id}`, {
    auth: true,
  });
}

export async function cancelBooking(id) {
  return request(`/bookings/${id}/cancel`, {
    auth: true,
    method: "PUT",
    body: JSON.stringify({}),
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

export async function changeUserPassword(id, formData) {
  return request(`/users/${id}/password`, {
    auth: true,
    method: "PUT",
    body: JSON.stringify(formData),
  });
}

export async function getUsers() {
  return request("/users", {
    auth: true,
  });
}

export async function updateUserRole(id, role) {
  return request(`/users/${id}/role`, {
    auth: true,
    method: "PUT",
    body: JSON.stringify({ u_Type: role }),
  });
}

export async function deleteUser(id) {
  return request(`/users/${id}`, {
    auth: true,
    method: "DELETE",
  });
}

export async function sendContactMessage(formData) {
  return request("/contact", {
    auth: true,
    method: "POST",
    body: JSON.stringify(formData),
  });
}

export async function getContactMessages() {
  return request("/contact", {
    auth: true,
  });
}

export async function markContactMessageRead(id) {
  return request(`/contact/${id}/read`, {
    auth: true,
    method: "PUT",
    body: JSON.stringify({}),
  });
}

export async function archiveContactMessage(id) {
  return request(`/contact/${id}/archive`, {
    auth: true,
    method: "PUT",
    body: JSON.stringify({}),
  });
}

export async function deleteContactMessage(id) {
  return request(`/contact/${id}`, {
    auth: true,
    method: "DELETE",
  });
}

export async function getAdminStats() {
  return request("/stats/admin", {
    auth: true,
  });
}
