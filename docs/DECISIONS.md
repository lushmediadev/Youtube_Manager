# Decisions

| ID | Decision | Rationale | Status |
| --- | --- | --- | --- |
| DEC-001 | Use `YouTube Data API v3 API key` for default channel tracking. | The desktop app tracks public channel stats only; OAuth would add consent/verification friction and still would not expose analytics for arbitrary public channels. | Active |
| DEC-002 | Use `admin/manager/user` RBAC. | The app needs workspace ownership: admin all data, manager assigned users, user own data. | Active |
| DEC-003 | Keep WPF source as reference and build the hostable app in web directories. | Avoid destructive replacement and preserve the working desktop app. | Active |
| DEC-004 | Keep YouTube fetch logic desktop-simple: resolve channel then call `channels.list` for public `snippet/statistics`. | The requested app only needs the same basic public stats shown in the desktop app. OAuth, Analytics/Reporting, raw response history, and browser fallback would add unnecessary complexity. | Active |
| DEC-005 | Store YouTube API keys as system-level secrets managed by `admin` only. | One API key pool serves public data fetches for all workspaces; managers/users should not read or overwrite shared secrets. | Active |
| DEC-008 | Open API key settings as a modal overlay from the sidebar Settings icon instead of navigating away from Channels. | API key entry is a quick system setting; users should stay in the channel workspace and finish with Check/Save actions in the modal. | Active |
| DEC-009 | Use backend API key fallback instead of exposing desktop `ThreadCount` in the UI. | Desktop `ThreadCount` controls concurrent channel checks. The web app handles concurrency via backend config and rotates through stored API keys when one fails or hits quota. | Active |
| DEC-010 | Use public YouTube channel banner as the hero cover when available, otherwise keep the existing default hero background. | `channels.list` can return `brandingSettings.image.bannerExternalUrl`, but some channels or API responses may omit it; the UI should fail softly without replacing the current fallback image. | Active |
| DEC-006 | Switch from dark theme to **light theme YouTube-style** (#ffffff bg, #0f0f0f text, #cc0000 accent). | Dark theme với tone đỏ gây nặng nề khó quan sát; light theme thân thiện hơn và tương đồng với YouTube chính thức. Shadow depth cho list rows tạo chiều sâu hiện đại. | Active |
| DEC-007 | Switch buttons to SaaS Blue (#1a73e8), simplify KPI cards, and use Roboto + Outlined icons. | Loại bỏ sự tương đồng quá mức với Spotify (tông bầu dục/icon tròn/màu xanh lá), tăng tính hiện đại của SaaS, làm nổi bật thông tin và sửa lỗi văn bản trắng chìm trên nền sáng. | Active |
| DEC-011 | Use backend paging, summary counts, and frontend virtual rendering for channel lists. | Large owner/group scopes can contain hundreds or thousands of channels. Navigation should load only summary plus the visible page; refresh/export/delete-dead actions fetch the full scope only when explicitly triggered. | Active |
