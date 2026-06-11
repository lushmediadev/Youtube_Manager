# VPS Deployment

This folder contains the YouTube Manager runtime config for VPS deployment.

- `docker-compose.vps.yml`: YouTube Manager app + dedicated PostgreSQL database.
- `.env.example`: environment template.
- `Caddyfile`: route snippet for the shared Caddy proxy.

The app is deployed separately from SpotiCheck. It does not reuse the Spotify database, volume, or app container.

## Target

- App path: `/opt/youtube-manager/app`
- Domain: `ytm.jazzrelaxation.com`
- App container: `ytm-app`
- DB container: `ytm-db`
- DB volume: `youtube-manager_ytm_postgres_data`
- Shared proxy network: `shared_proxy`

## Rollout

1. Copy `.env.example` to `.env` and fill real secrets.
2. Ensure the shared Docker network exists:
   - `docker network inspect shared_proxy || docker network create shared_proxy`
3. Run:
   - `docker compose -f docker-compose.vps.yml --env-file .env up -d --build`
4. Add this route to the shared Caddyfile managed by SpotiCheck:
   - `ytm.jazzrelaxation.com { reverse_proxy ytm-app:8000 }`
5. Validate and reload Caddy from `/opt/spoticheck/app/deploy`.
6. Verify:
   - `https://ytm.jazzrelaxation.com/api/health`

## Notes

- Persistent business data lives in the dedicated PostgreSQL volume for this app.
- The app serves the static frontend from FastAPI; there is no separate frontend container.
- Keep `.env`, SQLite files, and Docker volumes out of Git.
