# UI System

## Direction
- **YouTube-like Workspace**: nền trắng/xám rất nhẹ, chữ chính `#0f0f0f`, text phụ `#606060`, accent đỏ YouTube chỉ dùng cho logo, selection, error/destructive state.
- Không dùng accent xanh SaaS cho nút, link, menu active hoặc selection.
- Hạn chế black fill: chỉ dùng cho CTA chính hoặc trạng thái cần neo thị giác; avatar, badge, filter, border dùng surface xám nhẹ hoặc red-tint theo YouTube.
- Keep a dense operational UI: sidebar icon rail, group panel, table/list, toolbar, settings, and admin user management.
- Avoid marketing/hero filler; the first screen should be the usable channel tracking workspace.
- Rows là list phẳng có divider, hover xám nhẹ, selected có nền xám và vạch đỏ nhỏ bên trái.
- Controls dùng radius nhất quán khoảng `10px`; tránh lẫn phẳng/card bo tròn quá nhiều.
- Sử dụng font chữ **Roboto** làm chủ đạo và **Material Symbols Outlined** cho icon để gần với visual YouTube.

## Design Tokens (SaaS + YouTube Core Theme)
| Token | Value | Usage |
|-------|-------|-------|
| `--surface-0` | `#ffffff` | Body, content panel, rows |
| `--surface-1` | `#f9f9f9` | Hover and subtle sections |
| `--surface-2` | `#f2f2f2` | Search/input bg, active group, selected row |
| `--surface-3` | `#e5e5e5` | Dividers, borders |
| `--text-main` | `#0f0f0f` | Primary text |
| `--text-muted` | `#606060` | Secondary text, labels |
| `--accent-saas` | `#0f0f0f` | Primary action button fill |
| `--accent-yt` | `#ff0000` | YouTube red for brand/selection/error |
| `--red-down` | `#cc0000` | Error/destructive text |

## Product Copy
- Use Vietnamese labels for product actions where helpful.
- Keep API fields, commands, endpoint names, and config keys in English.

## Core Screens
- Channel workspace: group list, channel table, add channel modal, row operations.
- Settings: API key management and profile/password.
- Users: admin/manager user management with role and manager assignment.

## Table Columns
- `STT`
- `Kênh`
- `Owner / Updated`
- `Video`
- `Subscriber`
- `View`
- `Biến động / Ngày`
- `Checked`
