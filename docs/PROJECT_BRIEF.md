# Project Brief

## Purpose
- Rebuild desktop `Youtube Manager` thành web app host được trên VPS.
- Giữ cách lấy data như desktop app: dùng `YouTube Data API v3 API key` để lấy public channel stats.
- Tái dùng mô hình workspace/list/group của `D:\Spotify_AnylaticsWeb_App`, nhưng thêm phân quyền `admin/manager/user`.

## System Shape
- `backend/app/`: FastAPI API, auth, RBAC, database models, YouTube crawler.
- `frontend/`: static SPA quản lý group/channel/settings/user.
- `deploy/`, `Dockerfile`: nền deploy VPS lấy từ app Spotify.
- `YoutubeManager-master/` và `app/`: bản desktop WPF cũ, giữ lại để tham chiếu hành vi.

## Core Invariants
- Public channel tracking dùng `YouTube Data API v3`; không dùng OAuth cho luồng mặc định.
- Data fetch giữ đơn giản như desktop app: resolve channel từ URL/id/handle rồi gọi `channels.list` để lấy `snippet/statistics`.
- `admin` xem/quản trị tất cả channel/group/user.
- `manager` xem/quản trị workspace của chính mình và user thuộc manager đó.
- `user` chỉ xem/quản trị channel/group của chính mình.
- API key là secret cấp hệ thống, chỉ `admin` quản lý trong settings.

## Build / Test / Lint
- Backend syntax: `python -m compileall backend/app`
- Frontend syntax: `node --check frontend/app.js`
- Run local: `python -m uvicorn backend.app.main:app --host 127.0.0.1 --port 8010`

## References
- Desktop source: `YoutubeManager-master/YoutubeManager/Works/ChannelLoadWork.cs`
- Spotify scaffold: `D:\Spotify_AnylaticsWeb_App`
- Role reference: `D:\vps mới\Youtube_Upload_Lush`
