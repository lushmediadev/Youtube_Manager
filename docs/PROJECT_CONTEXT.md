# Project Context: YouTube Manager

## Mục tiêu dự án
- Rebuild bản desktop `Youtube Manager` (WPF) thành một web application host được trên VPS.
- Giữ nguyên cơ chế lấy dữ liệu kênh YouTube từ API công khai (YouTube Data API v3 công cộng) mà không cần OAuth cho luồng mặc định.
- Tái sử dụng giao diện cấu trúc workspace/group/channel của ứng dụng Spotify (`D:\Spotify_AnylaticsWeb_App`).
- Bổ sung phân quyền `admin/manager/user`.

## Cấu trúc hệ thống
- **Backend**: FastAPI, SQLite database, lưu API keys dưới dạng bí mật cấp hệ thống.
- **Frontend**: Single Page Application (SPA), dùng HTML/JS thuần + Tailwind CSS + Vanilla CSS (`style.css`).
- **Tài liệu dự án**: Lưu trữ tại thư mục `docs/`.

## Lịch sử điều chỉnh thiết kế
- Giai đoạn đầu: Sử dụng dark theme của Spotify.
- Giai đoạn 2 (2026-06-09): Chuyển sang Light Theme theo tông YouTube (nền trắng, chữ đen, accent đỏ).
- Giai đoạn 3 (2026-06-09): Nâng cấp lên **SaaS & YouTube Studio Style Theme**:
  - Accent chính chuyển sang màu xanh dương SaaS `#1a73e8` để tạo sự hiện đại, dễ quan sát.
  - Accent YouTube đỏ `#cc0000` làm điểm nhấn thương hiệu (logo, live badge).
  - Sử dụng font chữ Roboto và các icon Outlined thanh mảnh.
  - Các KPI card dạng phẳng bo góc nhẹ kèm vạch màu nhấn.
  - Khắc phục các lỗi chữ trắng bị ẩn trên nền sáng.
