# Decisions Index

## Active Decisions

- `DEC-001`: Default tracking uses `YouTube Data API v3 API key`, not OAuth. OAuth/Analytics is out of scope unless the app later needs owned-channel analytics.
- `DEC-002`: Web app uses `admin/manager/user` RBAC. Admin sees all; manager sees self plus assigned users; user sees only self.
- `DEC-003`: Keep existing WPF desktop source in place as behavior reference while the hostable web app lives in `backend/`, `frontend/`, and `deploy/`.
- `DEC-004`: YouTube fetch flow stays desktop-simple: API key plus `channels.list` public stats only. No OAuth, Analytics/Reporting, raw history, or browser fallback in the default path.
- `DEC-005`: YouTube API keys are system-level secrets managed by `admin` only.
- `DEC-008`: API key settings opens as an overlay modal on the Channels workspace; profile/password settings stay in user management, not the API key modal.
- `DEC-009`: Multi-key handling uses backend fallback across stored API keys. Desktop `ThreadCount` maps to backend concurrency config, not a user-facing setting.
- `DEC-010`: Hero cover may use public YouTube channel banner from `brandingSettings.image.bannerExternalUrl`; if absent, keep the existing default hero background.
- `DEC-011`: Channel lists use backend paging, summary counts, and frontend virtual rendering. Bulk actions fetch full scope on demand instead of loading every row during navigation.
