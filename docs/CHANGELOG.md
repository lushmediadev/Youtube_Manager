# Changelog

## 2026-06-12
- Removed visible select-all toolbar buttons and fixed group range selection anchoring for Ctrl/Shift multi-select.
- Reduced perceived UI delays by moving group edits, row moves, refresh feedback, and startup data loads away from blocking foreground awaits.
- Closed the Add Channel modal before the backend crawl request so `Add & Check` responds immediately while the add job continues in the background.
- Made add/delete channel operations update the visible list optimistically, so rows appear or disappear immediately while backend sync continues in the background.
- Added first-page preload and background page warming for sibling/current channel-list scopes so switching groups has cached data ready without blocking the first render.
- Added metric snapshots for YouTube channel refreshes and changed `Biến động / Ngày` to compare against a stored baseline with a minimum one-day denominator instead of showing `/ 00`.
- Changed `Biến động / Ngày` back to desktop-style previous-refresh comparison while keeping metric snapshots as raw refresh history.
- Added instant small-group navigation by deriving per-group first-page caches from fully loaded All Channels data before falling back to skeleton placeholders.
- Reduced perceived loading delays by rendering cached/current rows for search, sort, and owner-filter navigation before falling back to skeleton placeholders.

## 2026-06-11
- Added a warm-start list cache so returning users see cached groups/first-page channels immediately while production API data refreshes in the background.
- Fixed pending list state so the internal page size `160` is not shown as the real `All Channels` count while switching owner filters.
- Made group, owner-filter, search, sort, and context-menu navigation render an immediate pending list state before waiting for production API responses.
- Prepared VPS deployment for `ytm.jazzrelaxation.com` with a dedicated YouTube Manager app/db compose stack that joins the shared Caddy proxy network without sharing SpotiCheck data.
- Shortened the horizontal YouTube logo mark so its visible red player shape matches the native YouTube icon proportions more closely.
- Rebuilt the horizontal YouTube mark as a higher-resolution SVG vector and enlarged the login/sidebar logo rendering for cleaner edges.
- Added a separate horizontal YouTube mark for the sidebar and login screen so the app logo matches the YouTube Studio-style ratio instead of a square app icon.
- Widened the sidebar YouTube logo mark to match the native YouTube icon ratio and renamed the top toolbar refresh button to `Refresh all`.
- Replaced the square YouTube app mark with a cleaner transparent SVG player icon and applied it consistently to the favicon, sidebar logo, and login logo.
- Changed channel Excel export from HTML `.xls` download to a real backend-generated `.xlsx` file and made group sidebar labels consistently bold.
- Added a root local `.env` so local runs from `D:\Youtube_manager` use `backend/youtube_manager.db` instead of creating an empty root SQLite database.
- Added backend paging/search/sort plus `/items/summary`, switched the channel table to cached virtual rendering, and made refresh/export/delete-dead bulk actions fetch the full scope only on demand.

## 2026-06-12
- Added row/group selection action toolbars with `Ctrl+A`, `Delete`/`Backspace`, `Escape`, and `Ctrl`/`Shift` group selection behavior matching the Spotify app interaction model.

## 2026-06-10
- Optimized slow channel-list operations: row drag/drop now updates optimistically and saves using lightweight item-id scopes, while Refresh all/group uses a backend scope refresh endpoint instead of downloading full row data first.
- Switched hero banner URLs to a JPEG `w1707` YouTube image variant and reduced hero image filtering so channel covers render sharper without loading oversized images.
- Fixed the local backend `.env` to use the YouTube SQLite database instead of the old Spotify PostgreSQL database, then backfilled `banner_image` for the existing tracked channels.
- Added YouTube channel banner capture via `brandingSettings.image.bannerExternalUrl` and made the channel hero use the first visible channel banner with the existing default background as fallback.
- Rounded the login username/password inputs, removed the first-account admin hint, and added up/down icons to channel daily-change badges.
- Removed `display_name` from user management code paths and redesigned the create/edit user modal with the existing light palette, one-row username/role/manager layout, and visible dropdown chevrons.
- Narrowed the create/edit user modal and moved `Role`/`Manager` to the top row while placing `Username` on its own line.
- Upgraded the user management list with letter avatars, role/status badges, icon actions, a manager-scope filter, and fixed sidebar active state so `Channels` is not highlighted together with other tabs.
- Refined the user manager filter into a multi-select picker without an `All managers` option, with removable manager chips, matching dropdown width, and more compact user cards.
- Polished the manager filter dropdown search into a rounded full-width input with inline search icon and replaced selected red-rail rows with checkbox selection.
- Adjusted the manager picker search to a standard rounded input, highlighted selected manager avatars in pink/red, and kept the picker open while selecting multiple managers.
- Simplified selected manager rows so only the checkbox turns red while the row and avatar keep the default neutral styling.
- Replaced the group-panel owner native select with a custom light dropdown including a chevron and connected menu styling.
- Added an inline search bar inside the group-panel owner dropdown for filtering large owner lists.
- Added drag-and-drop ordering for groups and channel rows, channel drop-to-group movement, and custom right-click context menus for group/channel actions.
- Restyled custom group rows to match the `All Channels` card width and hide edit/delete actions until hover.
- Added a workspace-level right-click menu for blank channel-list areas with add, refresh, copy, and clear actions.
- Expanded right-click actions with TXT/Excel export, refresh all/group scopes, delete dead channels by group/all scopes, row-level Add Channel, and blank group-panel Add Group actions.
- Trimmed group context menus to group-only actions and clarified refresh/delete-dead labels so current-group scope is distinct from all-channel scope.
- Replaced browser prompt dialogs for creating/renaming groups with inline group-row editing using save/cancel controls and Enter/Escape shortcuts.
- Improved channel row drag/drop so blank table areas accept top/bottom drops with auto-scroll, and added sortable table headers for channel metrics and checked columns.
- Softened remaining dark control borders and focus states on search inputs, inline group editing, ghost buttons, checkboxes, and legacy panels.
- Removed black focus/inset borders from group search, owner filter, manager filter, and user modal inputs.
- Reduced black fill usage in user management by switching role avatars and admin badges to light gray/red-tint surfaces, keeping black only for primary CTA-style actions.
- Added Shift-click range selection for channel rows and click-outside/blank-area clearing for selected rows.
- Improved group drag/drop so blank areas, top/bottom edges, and scroll zones can be used to reorder groups like channel rows.
- Replaced remaining visible native selects in add-channel and user modals with searchable custom dropdowns matching the owner filter style.
- Squared off the login username/password inputs and removed the blue focus glow for a cleaner auth form.
- Replaced the leftover Spotify favicon with a YouTube-style SVG favicon and updated page icon links to bust browser cache.

## 2026-06-09
- Rebuilt the copied Spotify web scaffold into a hostable YouTube Manager app with FastAPI backend, static frontend, YouTube Data API v3 public channel tracking, API key settings, and `admin/manager/user` RBAC.
- Simplified the YouTube fetch path to match the desktop app: channel URL/id/handle resolution plus `channels.list` public `snippet/statistics`; removed unused Spotify/browser crawler modules from the active backend.
- Refined the YouTube-like UI: removed blue SaaS accents, changed KPI badges to a light liquid-glass style, and simplified the sidebar bottom control so only the centered logout icon remains.
- Aligned the group panel filter/search controls with the group row width by normalizing horizontal padding.
- Replaced the Settings page with an API key modal opened from the sidebar, removed profile/password from that flow, and added backend fallback across multiple YouTube API keys when a key fails or hits quota.
- Polished the API key modal footer with a divider, tighter spacing, clearer status badge, and updated explanatory copy.
- **Light Theme Migration**: Chuyển toàn bộ UI từ dark theme (#0b0f14 + #ff0033) sang light theme YouTube-style (#ffffff + #0f0f0f + #cc0000). Thêm shadow depth cho list rows (hover elevation). Cập nhật 3 file: `style.css`, `index.html`, `login.html`.
- **YouTube Studio & SaaS Style UI**: Thay đổi thiết kế từ Spotify-style sang hướng YouTube Studio kết hợp SaaS hiện đại: nút bấm màu xanh dương (#1a73e8) bo góc 8px, thẻ KPI phẳng viền mỏng kèm vạch màu chỉ báo, icon Outlined thanh mảnh, font Roboto, sửa lỗi hiển thị chữ trắng chìm trên nền sáng và nâng cấp giao diện quản trị user lên nền sáng.
