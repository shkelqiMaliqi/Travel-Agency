import React from "react";
import { render, screen, fireEvent } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import ForgotPassword from "./ForgotPassword";
import { requestPasswordReset } from "../services/api";

jest.mock("../services/api", () => ({ requestPasswordReset: jest.fn(), resetPassword: jest.fn() }));

test("asks the user to check email and never fills a reset code from the API response", async () => {
  requestPasswordReset.mockResolvedValue({ message: "Check your inbox.", resetCode: "must-not-be-used" });
  render(<MemoryRouter><ForgotPassword /></MemoryRouter>);
  fireEvent.change(screen.getByLabelText("Account email"), { target: { value: "traveler@example.com" } });
  fireEvent.click(screen.getByRole("button", { name: "Email reset code" }));
  expect(await screen.findByText("Check your inbox.")).toBeInTheDocument();
  expect(screen.getByLabelText("Reset code")).toHaveValue("");
  expect(screen.queryByText("must-not-be-used")).not.toBeInTheDocument();
});
