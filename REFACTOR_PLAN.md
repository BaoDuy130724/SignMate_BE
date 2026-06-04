# 📋 KẾ HOẠCH REFACTOR BACKEND SIGNMATE — PURE CQRS

> Mục tiêu: Refactor toàn bộ BE .NET (`SignMate_BE`) sang **Pure CQRS** trên nền Clean Architecture,
> đảm bảo đáp ứng đầy đủ nhu cầu của app mobile SignMate (xem `SIGNMATE_APP_CHUC_NANG_CHI_TIET.md`).

## ✅ Các quyết định đã chốt

| Hạng mục | Quyết định |
|---|---|
| Mức độ CQRS | **Pure CQRS** — logic vào thẳng Command/Query Handler, dùng UoW + Repository, gỡ dần Service layer |
| Response format | **ApiResponse toàn bộ** — mọi endpoint bọc `{ success, data, message, errors }` |
| Phạm vi | **Refactor + bổ sung endpoint còn thiếu** mà app mobile cần |
| Build | **Build (và test nếu có) sau mỗi phase** trước khi sang phase kế |

## 🧱 Hiện trạng nền tảng (đã có sẵn)

- Clean Architecture 4 layer: `API / Application / Domain / Infrastructure`
- `IRepository<T>`, `IUnitOfWork`, `Repository`, `UnitOfWork`
- DI tách layer: `AddApplication()` + `AddInfrastructure()` hội tụ ở `Program.cs`
- `GlobalExceptionMiddleware` (map exception → ApiResponse)
- `ApiResponse` / `ApiResponse<T>`
- MediatR + `ValidationBehavior` đã đăng ký
- CQRS **một phần**: Auth/Admin/Onboarding mới là MediatR bọc mỏng gọi lại Service (logic vẫn ở Service)
- FluentValidation đã đăng ký nhưng **gần như chưa có validator**

---

## 🎯 Convention chuẩn (áp dụng xuyên suốt)

### Cấu trúc thư mục mỗi feature (Application layer)
```
Features/{Module}/
  Commands/{Action}/
    {Action}Command.cs          // record : ICommand<TResult>
    {Action}CommandHandler.cs   // logic nghiệp vụ + IUnitOfWork/IRepository
    {Action}CommandValidator.cs // AbstractValidator (FluentValidation)
  Queries/{Action}/
    {Action}Query.cs            // record : IQuery<TResult>
    {Action}QueryHandler.cs     // EF Core projection (read-side), AsNoTracking
```

### Quy ước cốt lõi
- **Controller siêu mỏng**: chỉ inject `IMediator`, gọi `_mediator.Send(...)`, bọc kết quả vào `ApiResponse.SuccessResult(...)`. Không logic, không inject service/DbContext.
- **Handler trả về DTO/kết quả thuần** → Controller bọc `ApiResponse` cho success; **mọi failure ném exception** → middleware chuẩn hóa thành `ApiResponse` thất bại.
- **Marker interface** `ICommand<T>` / `IQuery<T>` (kế thừa `IRequest<T>`) để tách rõ ghi/đọc.
- **Command** (ghi): dùng `IUnitOfWork` + `IRepository<T>`, gọi `SaveChangesAsync`, transaction khi cần nhiều bước.
- **Query** (đọc): projection EF Core `.Select()` → DTO, `AsNoTracking`, không đi qua repository ghi.
- **Exception ngữ nghĩa** (`Application/Common/Exceptions`): `NotFoundException`, `ConflictException`, `ForbiddenException`, `BadRequestException` → middleware map status code; thay dần exception thô.
- **Comment chuẩn senior**: XML-doc `<summary>` cho mọi Command/Query/Handler/Validator/Repository method — nêu *mục đích nghiệp vụ*, không diễn giải lại code.
- **DI**: Handler tự động register qua MediatR; mỗi phase **xóa Service + dòng DI tương ứng** sau khi chuyển xong.

### Nguyên tắc SOLID (chất lượng codebase)
- **S — Single Responsibility**: mỗi Handler chỉ giải quyết đúng 1 use-case; mỗi Validator chỉ kiểm tra 1 request; Controller chỉ điều phối. Không gộp nhiều nghiệp vụ vào 1 class.
- **O — Open/Closed**: thêm nghiệp vụ = thêm Command/Query/Handler mới, không sửa code cũ. Cross-cutting concern (validation, logging) mở rộng qua `IPipelineBehavior`.
- **L — Liskov Substitution**: lập trình theo abstraction (`IRepository<T>`, `IEmailService`…); mọi implementation thay thế được mà không phá vỡ handler.
- **I — Interface Segregation**: interface nhỏ, đúng mục đích (`IOtpService`, `IBlobService`…). Không tạo "fat interface".
- **D — Dependency Inversion**: Handler phụ thuộc abstraction ở tầng Application; implementation cụ thể nằm ở Infrastructure và được nối qua DI. Domain/Application không tham chiếu Infrastructure.

### Nguyên tắc ACID (bảo toàn dữ liệu) — áp dụng ở write-side
- **Ghi đơn (1 entity / 1 aggregate)**: một lần `SaveChangesAsync` của EF Core đã là **atomic** — không cần transaction tường minh.
- **Ghi nhiều bước phụ thuộc nhau** (ví dụ: trừ XP + cập nhật streak + tạo notification; tạo subscription + cập nhật user; enroll + khởi tạo progress): **bắt buộc** bọc trong transaction:
  `BeginTransactionAsync` → các thao tác → `CommitTransactionAsync`; lỗi thì `RollbackTransactionAsync` (đặt trong try/catch hoặc dựa vào middleware để rollback) → đảm bảo **Atomicity** & **Consistency**.
- **Isolation**: chọn `IsolationLevel` phù hợp (mặc định `ReadCommitted`; nâng lên `Serializable`/`RepeatableRead` cho thao tác nhạy cảm chống race như cộng dồn streak/XP đồng thời).
- **Durability**: chỉ phản hồi thành công cho client **sau khi** commit thành công.
- **Idempotency / chống trùng**: với thao tác có thể bị gọi lặp (thanh toán callback VNPay, cập nhật progress), kiểm tra trạng thái hiện tại trước khi ghi để không nhân đôi dữ liệu.

---

## 🗂️ Các Phase (DỄ → KHÓ)

### Phase 0 — Nền tảng & "Gold Standard" *(bắt buộc làm trước)*
- Thêm `ICommand<T>` / `IQuery<T>`, bộ `Exceptions` ngữ nghĩa, cập nhật middleware map exception mới.
- Thêm `BaseApiController` (helper bọc ApiResponse) + (tùy chọn) `LoggingBehavior`.
- Chuẩn hóa lỗi model-binding / 401 / 404 cũng trả `ApiResponse`.
- **Convert trọn vẹn module nhỏ nhất làm mẫu: `B2BContact`** (`POST /api/b2b/contact`).
- ✅ Build.

### Phase 1 — Module dễ
`Onboarding` → `Notifications` → `Dashboard` (progress) → `Enrollments` (enrollments/me) → `Users` (me, me/streak).

### Phase 2 — Auth (core, nâng skeleton → pure CQRS)
login, register, send-register-otp, refresh, forgot/reset/change-password, logout.

### Phase 3 — Content domain (app-critical)
`Courses` → `Lessons` → `Vocabulary` (upload-reference multipart + background job tách keypoint).

### Phase 4 — Learning & AI
`Progress` (progress/sign, progress/lesson, +XP) → `Streak` → `Practice` → `Games`.

### Phase 5 — Payment *(khó, external)*
`Subscription` (plans, subscription/me, subscribe) + `VNPay` (callback vnpay-return, ResponseCode).

### Phase 6 — Teacher
teacher/dashboard, classes, students, comments, vocabulary upload (tái dùng Phase 3).

### Phase 7 — Center / Class / Tracking
center-home, center classes + class detail, students, reports, student online-tracking.

### Phase 8 — Admin & Analytics *(nặng nhất — aggregation)*
admin/dashboard (retention, conversion, revenue), reports, analytics.

### Phase 9 — Dọn dẹp & nghiệm thu
Xóa Service layer còn sót, rà DI/`Program.cs` sạch, đảm bảo không còn DbContext/Service trong Controller, đối chiếu lại từng endpoint app cần, Swagger + full build cuối.

---

## ⚙️ Cách vận hành theo đợt
- Mỗi phase: convert → `dotnet build` → báo cáo ngắn (file đổi, endpoint, gap đã lấp) → duyệt → sang phase sau.
- Phase 0 là "khuôn vàng" để các phase sau nhân bản, tránh sửa đi sửa lại.

## 📌 Theo dõi tiến độ

- [x] Phase 0 — Nền tảng + B2BContact mẫu
- [x] Phase 1 — Onboarding, Notifications, Dashboard, Enrollments, Users
- [x] Phase 2 — Auth
- [x] Phase 3 — Courses, Lessons, Vocabulary
- [x] Phase 4 — Progress, Streak, Practice, Games
- [x] Phase 5 — Subscription, Plans, VNPay
- [x] Phase 6 — Teacher
- [x] Phase 7 — Center, Class, Tracking
- [ ] Phase 8 — Admin, Analytics
- [ ] Phase 9 — Dọn dẹp & nghiệm thu
