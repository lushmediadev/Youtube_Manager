# Working agreements

- Khi giao tiếp, trả lời, walkthrough, task/checklist, hướng dẫn triển khai: viết tiếng Việt.
- Giữ nguyên tiếng Anh cho tên hàm/biến, log lỗi, lệnh terminal, config key, API field.
- Luôn phân loại nhiệm vụ thành `Quick Task` hoặc `Project Task` trước khi thực hiện.
- Mục tiêu ưu tiên: giảm token đầu vào, chỉ nạp context theo nhu cầu thực tế.

## Quick Task

- Trả lời trực tiếp.
- Không đọc project memory mặc định.
- Chỉ đọc code/file khi câu hỏi phụ thuộc trực tiếp vào chúng.
- Không bắt buộc cập nhật `docs/CHANGELOG.md`, `docs/DECISIONS_INDEX.md`, `docs/modules/*`, `docs/tasks/*`.

## Project Task

- Trước khi sửa code, luôn đọc:
  - `AGENTS.md`
  - `docs/PROJECT_BRIEF.md`
  - `docs/MEMORY_INDEX.md`
- Chỉ đọc thêm khi task thực sự cần:
  - `docs/modules/<module>.md`
  - `docs/DECISIONS_INDEX.md`
  - `docs/tasks/active/<task-id>.md`
  - `docs/UI_SYSTEM.md` nếu task đụng UI
- Không đọc mặc định full changelog, archive, decision history, hoặc task history.

## Project Memory Skill

- Với mọi `Project Task`, ưu tiên dùng skill `project-memory-bootstrap` để tạo mới hoặc refresh project memory chuẩn.
- Root `AGENTS.md` chỉ giữ policy, trigger, và routing; không giữ template chi tiết hay workflow memory dài.

## Rule Bootstrap

- Với `Project Task`, trước khi sửa code phải quét nhanh cấu trúc repo và file config chính.
- Nếu thiếu, tạo tối thiểu:
  - `docs/PROJECT_BRIEF.md`
  - `docs/MEMORY_INDEX.md`
  - `docs/DECISIONS_INDEX.md`
  - `docs/CHANGELOG.md`
- Chỉ tạo `docs/modules/<module>.md` cho module thực sự quan trọng hoặc đang được chỉnh sửa.
- Chỉ tạo `docs/tasks/active/<task-id>.md` khi task có nhiều bước hoặc kéo dài qua nhiều lượt.

## Update Policy

- Chỉ cập nhật memory khi có thay đổi thật sự về tri thức.
- Sau mỗi `Project Task`, append 1 entry ngắn vào `docs/CHANGELOG.md`.
- Khi có quyết định mới còn hiệu lực, cập nhật `docs/DECISIONS_INDEX.md` và `docs/DECISIONS.md`.

## UI Design Discipline

- Task UI luôn dùng skill `uncodixfy`.
- Nếu task đụng product UI, dashboard, settings, detail page, browser/library, result/review flow thì dùng thêm `tailwind-ai-webapp-ui`.
- Nếu có tiếng Việt trong UI thì dùng thêm `utf8-vietnamese-ui-guard`.
- Nếu project có UI, ưu tiên đọc `docs/UI_SYSTEM.md` trước khi sửa UI.

## Safety Rules

- Không đổi API/contract nếu chưa xác nhận impact hoặc chưa có migration plan.
- Không dùng memory file để thay cho việc đọc code liên quan.
- Không duplicate cùng một thông tin ở nhiều file memory.
- Mỗi loại thông tin phải có một canonical source.
