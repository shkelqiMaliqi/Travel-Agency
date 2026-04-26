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

export async function sendContactMessage(formData) {
  return request("/contact", {
    method: "POST",
    body: JSON.stringify(formData),
  });
}
