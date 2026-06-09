const { test, expect } = require("@playwright/test");

test("home page renders public travel actions", async ({ page }) => {
  await page.goto("/");

  await expect(
    page.getByRole("heading", { name: /find your next trip/i })
  ).toBeVisible();
  await expect(
    page.getByRole("link", { name: /view packages/i })
  ).toBeVisible();
  await expect(
    page.getByRole("link", { name: /explore destinations/i })
  ).toBeVisible();
});

test("login page exposes the authentication form", async ({ page }) => {
  await page.goto("/loginpage");

  await expect(page.getByLabel(/email or username/i)).toBeVisible();
  await expect(page.getByLabel(/password/i)).toBeVisible();
  await expect(page.getByRole("button", { name: /login/i })).toBeVisible();
});

test("forgot password page is reachable from login", async ({ page }) => {
  await page.goto("/loginpage");
  await page.getByRole("link", { name: /reset it here/i }).click();

  await expect(page).toHaveURL(/forgot-password/);
  await expect(
    page.getByRole("button", { name: /generate reset code/i })
  ).toBeVisible();
});
