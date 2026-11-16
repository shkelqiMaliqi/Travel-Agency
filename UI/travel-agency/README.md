# Travel Agency website

React 18, React Router 7, Vite, Vitest, and Playwright. Requires Node.js >=22.12.0 (npm 11 recommended).

From this folder: `npm ci`, `npm start`, `npm test`, `npm run lint`, `npm run build`. The production build is written to `build/`; `npm run preview` serves it on port 3100. `REACT_APP_API_URL` remains supported for compatibility; `VITE_API_URL` can also override the default `http://localhost:5132/api/v1`. Both are public API URLs, never secrets.

See the root README and `Tools/verify-windows.ps1` for database-backed authenticated browser tests and the local email inbox. `npm run e2e` alone runs public/form smoke checks; the full authentication scenario requires `E2E_FULL_STACK=true` and an isolated database.
