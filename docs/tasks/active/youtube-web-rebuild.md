# Youtube Web Rebuild

## Goal
Rebuild the desktop YouTube Manager as a hostable FastAPI + static frontend app using the Spotify app scaffold and `admin/manager/user` RBAC.

## Scope
- Copy and adapt the Spotify web scaffold.
- Replace Spotify crawl logic with YouTube channel public stats.
- Add backend API key settings.
- Keep YouTube data fetch simple like the desktop app: public `channels.list` stats only.
- Add manager role visibility and user assignment.
- Update frontend list columns and settings UI for YouTube.

## Verification
- `python -m compileall backend/app`
- `node --check frontend/app.js`
- Backend smoke with local app startup when dependencies are available.

## Current Notes
- `admin` manages global YouTube API keys.
- `manager` manages assigned users and their channel/group rows, but not API keys.
- `user` manages only their own channel/group rows.
