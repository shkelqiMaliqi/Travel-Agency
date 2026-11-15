import { clearAuth, getPackages, getUserProfile, createBooking, saveAuth } from "./api";

const response = (data) => ({
  ok: true,
  headers: { get: () => "application/json" },
  json: async () => ({ success: true, data }),
});

beforeEach(() => {
  clearAuth();
  global.fetch = jest.fn();
});

test("public reads deduplicate requests, reuse results, and avoid a JSON preflight header", async () => {
  fetch.mockResolvedValue(response([{ package_Id: 1 }]));
  await Promise.all([getPackages(), getPackages()]);
  await getPackages();
  expect(fetch).toHaveBeenCalledTimes(1);
  expect(fetch.mock.calls[0][1].headers["Content-Type"]).toBeUndefined();
});

test("a catalog read started before a booking cannot repopulate the cache after that booking", async () => {
  let resolveRead;
  fetch.mockImplementationOnce(() => new Promise((resolve) => { resolveRead = resolve; }));
  const pending = getPackages();
  fetch.mockResolvedValueOnce(response({ message: "Booked" }));
  await createBooking({ package_Id: 1, travelers: 1 });
  resolveRead(response([{ available_Seats: 10 }]));
  await pending;
  fetch.mockResolvedValueOnce(response([{ available_Seats: 9 }]));
  const fresh = await getPackages();
  expect(fresh[0].available_Seats).toBe(9);
  expect(fetch).toHaveBeenCalledTimes(3);
});

test("private data is fetched fresh and is not retained across account changes", async () => {
  saveAuth({ token: "first-account" });
  fetch.mockResolvedValue(response({ name: "First" }));
  await getUserProfile(1);
  await getUserProfile(1);
  expect(fetch).toHaveBeenCalledTimes(2);
  expect(fetch.mock.calls[0][1].headers.Authorization).toBe("Bearer first-account");
  clearAuth();
  saveAuth({ token: "second-account" });
  await getUserProfile(1);
  expect(fetch.mock.calls[2][1].headers.Authorization).toBe("Bearer second-account");
});
