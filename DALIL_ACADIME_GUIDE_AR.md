# دليل مشروع DalilAcadime بالعربي

> هذا الملف يشرح المشروع الحالي كما هو مكتوب، وبأسلوب تدريجي جدًا. الفكرة أن تتخيل أنك ترسل طلبًا من Swagger، ونمشي معه خطوة خطوة حتى يرجع الرد.

## 1. ما هو المشروع؟

`DalilAcadime` هو Web API لنظام إدارة أكاديمية/منصة تعليمية (LMS). النظام يحتوي حاليًا على:

- دورات تعليمية `Courses`.
- طلاب `Students`.
- مدربين `Instructors`.
- تسجيل الطالب في الدورة `Enrollments`.
- عمليات CRUD للدورات والطلاب.
- Pagination لإرجاع البيانات على صفحات.
- FluentValidation للتحقق من البيانات القادمة من المستخدم.
- Entity Framework Core للتعامل مع PostgreSQL.
- Swagger لتجربة الـ API.
- JWT Authentication مع endpoint لتسجيل الدخول وإنشاء التوكن، مع أدوار `Admin` و`Instructor` و`Student`.
- تسجيل مستخدم جديد من خلال `POST /api/Auth/register` مع إنشاء توكن مباشرة.
- رفع صور الدورات إلى `wwwroot/img/courses` مع التحقق من الامتداد والحجم.
- خدمة ملفات Static Files لإتاحة صورة الدورة من خلال رابط HTTP.
- Exception Handler موحد يحول الأخطاء إلى ردود `ProblemDetails` مفهومة.

## 2. الصورة الكبيرة جدًا

المشروع مقسوم إلى ثلاثة مشاريع، وكل مشروع له وظيفة:

```text
المستخدم / Swagger / أي Frontend
			  |
			  v
DalilAcadime.API
Controllers + Program + Exception Handler
			  |
			  v
DalilAcadime.Core
Entities + DTOs + Interfaces + Validators + Mappings
			  ^
			  |
DalilAcadime.InfraStructures
Services + Repositories + DbContext + PostgreSQL + Migrations
```

بكلام أبسط:

1. **API** يستقبل HTTP Request.
2. **Controller** يعرف أي عملية يريدها المستخدم.
3. **Service** يطبق منطق العمل: هل العنصر موجود؟ هل البيانات صحيحة؟ ماذا يجب أن يحدث؟
4. **Repository** ينفذ استعلام قاعدة البيانات.
5. **DbContext** يترجم استعلامات C# إلى SQL ويرسلها إلى PostgreSQL.
6. النتيجة ترجع بالعكس: Database ثم Repository ثم Service ثم Controller ثم HTTP Response.

## 3. لماذا يوجد أكثر من طبقة؟

لو وضعنا كل شيء داخل Controller سيعمل المشروع، لكنه سيصبح صعب الفهم والصيانة. التقسيم الحالي يفصل المسؤوليات:

- **Controller:** لا يعرف تفاصيل SQL.
- **Service:** لا يعرف تفاصيل HTTP و`Ok()` و`CreatedAtAction()`.
- **Repository:** لا يعرف كيف يتم اختيار status code.
- **Core:** يحتوي على العقود والنماذج الأساسية ولا يعتمد على API.
- **Infrastructure:** يحتوي على الأشياء الخارجية المتغيرة مثل PostgreSQL وEF Core.

مثال: إذا أردت لاحقًا تغيير PostgreSQL إلى SQL Server، غالبًا تعدل Infrastructure بدل أن تعيد كتابة Controllers وDTOs.

---

# 4. ترتيب التشغيل من أول لحظة

## 4.1 أين يبدأ البرنامج؟

البداية من:

```text
DalilAcadime.API/Program.cs
```

فيه السطر:

```csharp
var builder = WebApplication.CreateBuilder(args);
```

هذا ينشئ Builder. الـ Builder هو المكان الذي نجهز فيه الخدمات والإعدادات قبل تشغيل التطبيق.

## 4.2 تسجيل الخدمات

في `Program.cs` يتم تسجيل:

- MVC Controllers بواسطة `AddControllers()`.
- OpenAPI وSwagger.
- `ICourseRepository` مع `CourseRepository`.
- `ICourseServices` مع `CourseServices`.
- `IStudentRepository` مع `StudentRepository`.
- `IStudentService` مع `StudentService`.
- `IEnrollmentRepository` مع `EnrollmentRepository`.
- `IEnrollmentService` مع `EnrollmentService`.
- `IUsersRepository` مع `UsersRepository`.
- `IAuthServices` مع `AuthService`.
- `IPasswordHasher` مع `BcryptPasswordHasher`.
- `ITokenService` مع `JwtTokenService`.
- `IImageService` مع `ImgService`.
- Validators الموجودة في assembly الخاص بـ `CreateCourseDtoValidators`.
- Validators الخاصة بالدورات والطلاب والتسجيلات.
- Global Exception Handler.
- `AppDbContext` مع PostgreSQL.
- JWT Authentication.
- Authorization.

عندما يطلب Controller مثلًا `ICourseServices` في constructor، لا يقوم بنفسه بعمل `new CourseServices(...)`. الـ Dependency Injection Container ينشئ الكلاس ويحقن dependencies المطلوبة.

## 4.3 بناء التطبيق

```csharp
var app = builder.Build();
```

بعد هذا السطر تصبح الخدمات جاهزة ويتم إعداد Pipeline.

## 4.4 ترتيب الـ Middleware الحالي

الترتيب في `Program.cs` هو:

1. في Development: Swagger وSwagger UI.
2. في Development: OpenAPI endpoint.
3. `UseExceptionHandler()` لالتقاط الاستثناءات.
4. `UseHttpsRedirection()` لتحويل HTTP إلى HTTPS.
5. `UseStaticFiles()` لإتاحة الملفات الموجودة داخل `wwwroot` مثل صور الدورات.
6. `UseAuthentication()` لقراءة JWT إن وجد.
7. `UseAuthorization()` لتطبيق الصلاحيات إن وُجدت `[Authorize]`.
8. `MapControllers()` لربط URLs بالـ Controllers.
9. `Run()` لتشغيل السيرفر.

### لماذا الترتيب مهم؟

الطلب يمر عبر الـ Middleware بالترتيب. إذا حدث Exception داخل Controller أو Service، يجب أن يكون `UseExceptionHandler()` موجودًا ليحوله إلى رد منظم. وإذا كان هناك Authorization، يجب أن يأتي بعد Authentication حتى يكون المستخدم معروفًا أولًا.

## 4.5 روابط التشغيل

من `Properties/launchSettings.json`:

- HTTP: `http://localhost:5253`
- HTTPS: `https://localhost:7193`
- البيئة: `Development`

بما أن البيئة Development، يظهر Swagger في الجذر:

```text
https://localhost:7193/
```

وذلك لأن `RoutePrefix = string.Empty`.

---

# 5. المشاريع الثلاثة

## 5.1 مشروع `DalilAcadime.API`

نوعه Web SDK، ويستهدف `.NET 10`.

وظيفته:

- استقبال طلبات HTTP.
- تعريف الـ routes.
- تشغيل التطبيق.
- إعداد Dependency Injection.
- إعداد Swagger وAuthentication وException Handling.

الملفات:

- `Program.cs`
- `Controllers/CoursesController.cs`
- `Controllers/StudentController.cs`
- `Controllers/AuthController.cs`
- `Controllers/EnrollmentController.cs`
- `Middelwear/GlobalExceptionHandler.cs`
- `appsettings.json`
- `appsettings.Development.json`
- `DalilAcadime.API.http`
- `Properties/launchSettings.json`
- `wwwroot/img/courses/`
- `DalilAcadime.API.csproj`

> اسم مجلد `Middelwear` مكتوب بهذا الشكل في المشروع. الاسم الصحيح إملائيًا غالبًا `Middleware`، لكن الاسم الحالي لا يمنع التشغيل.

## 5.2 مشروع `DalilAcadime.Core`

يستهدف `.NET 10`.

وظيفته أن يكون قلب المشروع:

- Entities التي تمثل البيانات.
- DTOs التي تمثل البيانات الداخلة والخارجة من الـ API.
- Interfaces للعقود.
- Validators.
- Mappings.
- Exceptions الخاصة بالتطبيق.

هو لا يحتوي على Controller ولا اتصال مباشر بقاعدة البيانات.

الملفات الجديدة الخاصة بالمصادقة داخل Core:

- `Entities/Users.cs`
- `DTOs/LoginDTO.cs`
- `Repository/IUsersRepository.cs`
- `Security/IPasswordHasher.cs`
- `Security/ITokenService.cs`
- `Services/IAuthServices.cs`
- `Services/IEnrollmentService.cs`
- `Storage/IImageService.cs`
- `DTOs/EnrollmentDTO.cs`

## 5.3 مشروع `DalilAcadime.InfraStructures`

يستهدف `.NET 10`.

وظيفته تنفيذ التفاصيل الخارجية:

- `AppDbContext`.
- إعداد جداول EF Core.
- Repositories.
- Services الفعلية.
- إعدادات JWT.
- Migrations.
- PostgreSQL.

هذا المشروع يعتمد على `Core` لأنه ينفذ الـ interfaces الموجودة فيه.

الملفات الجديدة الخاصة بالمصادقة داخل Infrastructure:

- `Repository/UsersRepository.cs`
- `Security/BcryptPasswordHasher.cs`
- `Security/JwtTokenService.cs`
- `Data/Configration/UserConfigration.cs`
- `Services/AuthService.cs`
- `Services/EnrollmentService.cs`
- `Storage/ImgService.cs`
- `Migrations/20261105134705_InitialCreate.cs`
- `Migrations/20261105140344_FixUserPasswordHashLength.cs`
- `Migrations/20261105150243_AddUserId.cs`
- `Migrations/20261105160000_LinkExistingStudentsToUsers.cs`
- `Migrations/20261107094851_AddCourseImg.cs`

---

# 6. شرح ملفات API

## 6.1 `Program.cs`

### التسجيل

```csharp
builder.Services.AddControllers();
```

يخبر ASP.NET Core أن المشروع يستخدم Controllers ويقرأ attributes مثل `[HttpGet]` و`[Route]`.

```csharp
builder.Services.AddSwaggerGen(...);
```

ينشئ صفحة Swagger ويعرض اسم API ووصفه وإصداره.

```csharp
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
```

معناه: عندما يطلب أي كلاس `ICourseRepository`، أعطه `CourseRepository`.

`Scoped` يعني نسخة واحدة لكل HTTP Request. هذه مناسبة لـ `DbContext` لأن الـ DbContext نفسه غالبًا يعيش خلال الطلب فقط.

```csharp
builder.Services.AddValidatorsFromAssemblyContaining<CreateCourseDtoValidators>();
```

يبحث عن validators في assembly الذي يحتوي على `CreateCourseDtoValidators` ويسجلها تلقائيًا. لذلك يستطيع `CourseServices` طلب `IValidator<CreateCourseDto>`.

يتم تسجيل Validators الطلاب والتسجيلات صراحة أيضًا:

```csharp
builder.Services.AddValidatorsFromAssemblyContaining<CreateStudentDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateEnrollmentDtoValidator>();
```

التسجيل يتم من assembly الخاص بـ Core، لذلك يستطيع كل Service طلب الـ Validator الخاص به من Dependency Injection.

### قاعدة البيانات

```csharp
builder.Services.AddDbContext<AppDbContext>(opt =>
	opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```

هذا يقرأ `DefaultConnection` من الإعدادات ويجعل EF Core يستخدم PostgreSQL.

### JWT

الكود يقرأ قسم `Jwt` إلى `JwtSetting`، ثم يضع قواعد التحقق:

- `ValidateIssuer = true`: يجب أن يكون مصدر التوكن صحيحًا.
- `ValidateAudience = true`: يجب أن تكون الجهة المستقبلة صحيحة.
- `ValidateLifetime = true`: لا يقبل توكن منتهي الصلاحية.
- `ValidateIssuerSigningKey = true`: يتحقق من التوقيع.
- `ClockSkew = 5 دقائق`: سماح بسيط لاختلاف وقت السيرفرات.

### Exception Handler

```csharp
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
```

الأول يسجل المعالج، والثاني يجهز خدمة كتابة Problem Details.

## 6.2 `Controllers/CoursesController.cs`

الكلاس:

```csharp
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CoursesController(ICourseServices courseService, IImageService imageService) : ControllerBase
```

بسبب `[controller]` يصبح المسار الأساسي:

```text
/api/Courses
```

الـ primary constructor يستقبل `ICourseServices` ويخزنه تلقائيًا ليستعمله الكلاس.

الكلاس محمي افتراضيًا بـ `[Authorize]`، لكن بعض العمليات تضع `[AllowAnonymous]` حتى يمكن استخدامها بدون تسجيل دخول، وبعض العمليات الإدارية تتطلب دور `Admin`.

### Endpoint: جلب الدورات مع Pagination وفلترة السعر

```text
GET /api/Courses?page=1&pageSize=10&minPrice=0&maxPrice=10000
```

هذه العملية متاحة بدون تسجيل دخول بسبب `[AllowAnonymous]`. القيم يتم ضبطها قبل إرسالها للخدمة:

- `page` لا يقل عن 1.
- `pageSize` بين 1 و50.
- `minPrice` الحد الأدنى للسعر.
- `maxPrice` الحد الأعلى للسعر.

ترجع `PageResult<CourseResponseDto>` وليس قائمة عادية.

القيم `page` و`pageSize` يتم ضبطها قبل إرسالها للخدمة:

- `page = Math.Max(page, 1)` يمنع رقم صفحة أقل من 1.
- `pageSize = Math.Clamp(pageSize, 1, 50)` يجعل حجم الصفحة بين 1 و50.

السبب: منع صفحات غير منطقية ومنع طلب عدد ضخم جدًا من السجلات.

### Endpoint: جلب دورة بالـ ID

```text
GET /api/Courses/5
```

هذه العملية متاحة بدون تسجيل دخول بسبب `[AllowAnonymous]`.

إذا وجدت الدورة يرجعها. إذا لم توجد، Service يرمي `NotFoundException`، والمعالج العام يحولها إلى `404`.

### Endpoint: عدد الدورات

```text
GET /api/Courses/Count
```

متاح بدون تسجيل دخول بسبب `[AllowAnonymous]`، ويرجع عدد الدورات كرقم.

### Endpoint: إنشاء دورة

```text
POST /api/Courses
```

تتطلب تسجيل الدخول بدور `Admin`.

الـ body المتوقع مثلًا:

```json
{
  "title": "C# Programming",
  "price": 500,
  "level": "Beginner",
  "instructorId": 1
}
```

الـ Controller يمرر DTO للخدمة. عند النجاح يرجع `201 Created` مع رابط عملية جلب الدورة الجديدة بواسطة `CreatedAtAction`.

### Endpoint: تعديل دورة

```text
PUT /api/Courses/5
```

تتطلب تسجيل الدخول بدور `Admin`.

الـ body يحتوي `Title`, `Price`, `Level`, `InstructorId`.

ملاحظة: Controller ينفذ استدعاءً أولًا:

```csharp
var course = await courseService.GetCourseById(id);
```

لكنه لا يستخدم المتغير `course` بعد ذلك. ثم يستدعي `UpdateCourse`. هذا يعني غالبًا وجود استعلام زائد، لأن Service نفسه يتحقق من وجود الدورة مرة أخرى.

### Endpoint: حذف دورة

```text
DELETE /api/Courses/5
```

تتطلب تسجيل الدخول بدور `Admin`.

Service يجد الدورة ثم Repository يحذفها، والرد الحالي يرجع الكيان المحذوف داخل `200 OK`.

### Endpoint: رفع صورة دورة

```text
POST /api/Courses/upload-image
```

تتطلب تسجيل الدخول بدور `Admin`، ويجب إرسال الملف كـ `multipart/form-data` في الحقل `file`.
تقوم `CoursesController` بتمرير stream واسم الملف وحجمه إلى `IImageService`.

قواعد `ImgService` الحالية:

- الحجم أكبر من صفر ولا يتجاوز 2 MB.
- الامتدادات المسموحة: `.jpg` و`.jpeg` و`.png` و`.gif` و`.webp`.
- يتم إنشاء اسم عشوائي باستخدام `Guid` داخل `wwwroot/img/courses`.
- يرجع الـ API رابطًا مثل `/img/courses/{generated-name}.jpg`.
- `UseStaticFiles()` يجعل الرابط قابلًا للقراءة من العميل.

## 6.3 `Controllers/EnrollmentController.cs`

المسار الأساسي هو `/api/Enrollment` لأن اسم الكلاس مفرد.

| Method | Endpoint | الصلاحية | النتيجة |
|---|---|---|---|
| `GET` | `/api/Enrollment/me` | `Student` | دورات الطالب المرتبط بـ `NameIdentifier` داخل JWT |
| `GET` | `/api/Enrollment` | `Admin` | كل التسجيلات |
| `GET` | `/api/Enrollment/{id}` | `Admin` | تسجيل واحد |
| `POST` | `/api/Enrollment` | `Admin` | إنشاء تسجيل ويرجع `201 Created` |
| `PUT` | `/api/Enrollment/{id}` | `Admin` | تعديل تسجيل |
| `DELETE` | `/api/Enrollment/{id}` | `Admin` | حذف تسجيل |

في `/me` لا يرسل العميل `studentId` في الرابط؛ يقرأ Controller هوية المستخدم من التوكن، ثم يمرر `userId` إلى `EnrollmentService`.

## 6.4 `Controllers/StudentController.cs`

المسار الأساسي:

```text
/api/Student
```

> اسم Controller مفرد، لذلك المسار هو `Student` وليس `Students`.

الكلاس محمي بـ `[Authorize]`، لذلك كل عمليات الطلاب تتطلب توكن JWT صالحًا حاليًا.

### جلب الطلاب بصفحات

```text
GET /api/Student?page=1&pageSize=10
```

القواعد نفسها الموجودة في الدورات: الصفحة على الأقل 1، وحجم الصفحة من 1 إلى 50.

### جلب دورات طالب

```text
GET /api/Student/5/courses
```

Service يتأكد أولًا أن الطالب موجود، ثم Repository يجلب الدورات المرتبطة به عبر `Enrollments`.

### جلب طالب

```text
GET /api/Student/5
```

إذا لم يوجد الطالب يرجع `404` من خلال `NotFoundException`.

### إنشاء طالب

```text
POST /api/Student
```

مثال body:

```json
{
  "fullName": "Ahmed Ali",
  "email": "ahmed@example.com"
}
```

النتيجة الناجحة `201 Created`.

### تعديل طالب

```text
PUT /api/Student/5
```

يتم التحقق من البيانات، إيجاد الطالب، تعديل الاسم والإيميل، ثم الحفظ.

يوجد في Controller استدعاء أول لجلب الطالب وتخزينه في متغير غير مستخدم، كما في تعديل الدورة. التحقق الحقيقي يتم في Service مرة أخرى.

### حذف طالب

```text
DELETE /api/Student/5
```

إذا لم يوجد الطالب يرجع `404`، وإلا يحذف ويرجع الكيان المحذوف.

## 6.5 `Controllers/AuthController.cs`

المسار الأساسي هو:

```text
/api/Auth
```

### Login

```text
POST /api/Auth/login
```

هذه العملية عليها `[AllowAnonymous]`، أي لا تحتاج توكن لأن وظيفتها إنشاء التوكن أصلًا.

Body:

```json
{
  "email": "admin@a.com",
  "password": "admin123"
}
```

عند نجاح تسجيل الدخول يرجع `LoginResponseDto`:

```json
{
  "token": "eyJ...",
  "expiresAt": "..."
}
```

يتم استخدام التوكن في Swagger من زر **Authorize** بهذه الصيغة:

```text
Bearer eyJ...
```

### Register

```text
POST /api/Auth/register
```

متاح بدون تسجيل دخول. يستقبل `FullName` و`Email` و`Password`، ثم يقوم `AuthService` بتشفير كلمة المرور باستخدام BCrypt، وإنشاء Student بدور `Student`، ثم يرجع توكنًا وتاريخ انتهاء التوكن.

## 6.6 `Middelwear/GlobalExceptionHandler.cs`

الكلاس `GlobalExceptionHandler` يطبق `IExceptionHandler`.

وظيفته أن يمنع ظهور Exception خام للمستخدم ويحول الأخطاء إلى JSON موحد.

التحويل الحالي:

| الاستثناء | HTTP Status | العنوان |
|---|---:|---|
| `FluentValidation.ValidationException` | 400 | Validation failed |
| `NotFoundException` | 404 | Resource not found |
| `ConflictException` | 409 | Conflict |
| `UnauthorizedException` | 401 | Unauthorized |
| أي Exception آخر | 500 | An unexpected error occurred |

في حالة Validation، يتم تجميع الأخطاء حسب اسم الخاصية داخل:

```json
{
  "errors": {
	"Email": ["Email is required."]
  }
}
```

في حالة `500` يتم تسجيل الخطأ في Logger، لكن لا يتم إرسال تفاصيله للمستخدم، وهذا جيد أمنيًا حتى لا تظهر تفاصيل داخلية.

---

# 7. شرح Core بالتفصيل

## 7.1 Entities

### `Entities/Course.cs`

يمثل الدورة.

- `Id`: المفتاح الأساسي.
- `Title`: اسم الدورة.
- `Price`: السعر كـ `float`.
- `Level`: مستوى الدورة.
- `InstructorId`: مفتاح المدرب.
- `Instructor`: Navigation Property إلى المدرب.
- `Enrollments`: قائمة تسجيلات الطلاب في هذه الدورة.

الـ enum الداخلي `CourseLevel` يحتوي:

- `Beginner`.
- `Intermediate`.
- `Advanced`.

الـ Navigation Property ليست عمودًا عاديًا في جدول الدورات؛ هي طريقة EF Core لتمثيل العلاقة مع جدول المدربين والتسجيلات.

### `Entities/Student.cs`

يمثل الطالب:

- `Id`.
- `FullName`.
- `Email`.
- `Enrollments`: تسجيلات هذا الطالب.

### `Entities/Instructor.cs`

يمثل المدرب:

- `Id`.
- `FullName`.
- `Courses`: الدورات التي يدرسها.

### `Entities/Enrollment.cs`

يمثل عملية تسجيل طالب في دورة:

- `Id`.
- `StudentId` و`Student`.
- `CourseId` و`Course`.
- `EnrollmentAt`: وقت التسجيل، ويمكن أن يكون nullable، وله default في قاعدة البيانات.

العلاقة المنطقية هي:

```text
Student 1 ---- * Enrollment * ---- 1 Course
```

أي أن الطالب يمكن أن يسجل عدة دورات، والدورة يمكن أن يكون فيها عدة طلاب. جدول `Enrollment` هو الجدول الوسيط.

### `Entities/Users.cs`

يمثل حساب المستخدم الذي يستطيع تسجيل الدخول إلى النظام:

- `Id`: المفتاح الأساسي.
- `FullName`: اسم المستخدم.
- `Email`: البريد المستخدم في تسجيل الدخول.
- `PasswordHash`: كلمة المرور بعد تشفيرها بواسطة BCrypt، وليست كلمة المرور الأصلية.
- `Role`: دور المستخدم.

الكلاس `UserRoles` يحتوي القيم الثابتة للأدوار:

- `Admin`: يستطيع تنفيذ عمليات إدارة الدورات المحمية.
- `Instructor`: دور موجود في التوكن، لكن لا توجد حاليًا endpoints خاصة به.
- `Student`: الدور الافتراضي للمستخدم الجديد، ولا توجد حاليًا عملية إنشاء حساب من API.

## 7.2 DTOs

### `DTOs/CourseDTO.cs`

يوجد record اسمه `CreateCourseDto` للبيانات المطلوبة عند الإنشاء:

- `Title`.
- `Price`.
- `Level`.
- `InstructorId`.

و`UpdateCourseDto` يحتوي نفس الحقول للتعديل.

أما `CourseResponseDto` فهو ما يخرج للمستخدم:

- `Id`.
- `Title`.
- `Price`.
- `Level`.
- `InstructorName`.

لاحظ أنه لا يرجع كامل كائن Instructor، بل يرجع الاسم فقط. هذا يقلل حجم الرد ويمنع تكرار بيانات غير لازمة.

يوجد `CourseDTO` قديم داخل comment، لذلك لا يتم compile ولا يستخدم حاليًا.

### `DTOs/StudentDTO.cs`

- `StudentResponseDto`: البيانات الخارجة، وهي `Id`, `FullName`, `Email`.
- `CreateStudentDto`: `FullName`, `Email`.
- `UpdateStudentDto`: `FullName`, `Email`.

### `DTOs/LoginDTO.cs`

يحتوي DTOs الخاصة بتسجيل الدخول:

- `LoginRequestDto`: يستقبل `Email` و`Password`.
- `LoginResponseDto`: يرجع `Token` و`ExpiresAt`.

### `DTOs/PageResult.cs`

`PageResult<T>` هو container عام للـ Pagination:

- `Data`: النتائج الحالية.
- `Page`: رقم الصفحة.
- `PageSize`: عدد العناصر المطلوب في الصفحة.
- `TotalCount`: عدد العناصر الكامل في قاعدة البيانات.
- `TotalPages`: يحسب عدد الصفحات تلقائيًا.

مثال: إذا كان `TotalCount = 25` و`PageSize = 10` فالنتيجة `TotalPages = 3`.

## 7.3 Exceptions

في `Exceptions/AppExceptions.cs`:

- `NotFoundException`: العنصر المطلوب غير موجود.
- `ConflictException`: يوجد تعارض، مثل Email أو Course Title مكرر.
- `UnauthorizedException`: بيانات الدخول غير صحيحة أو المستخدم غير مصرح له.

هما يرثان من `Exception` حتى يستطيع الـ Global Handler معرفة نوع الخطأ.

## 7.4 Mappings

### `Mappings/CourseMappings.cs`

- `ToCourseResponseDto`: يحول Entity إلى Response DTO.
- `ToCourseEntity`: يحول Create DTO إلى Entity جديدة.

سبب وجود Mapping أن شكل قاعدة البيانات ليس بالضرورة هو الشكل الذي نريد إظهاره للعميل.

### `Mappings/StudentMapping.cs`

- `ToStudentResponseDto`: يحول Student إلى Response DTO.
- `ToStudentEntity`: يحول Create DTO إلى Student جديد.

هذه الدوال Extension Methods، لذلك يمكن كتابتها هكذا:

```csharp
student.ToStudentResponseDto()
```

بدلًا من إنشاء Mapper يدويًا كل مرة.

## 7.5 Interfaces الخاصة بالـ Repository

### `Repository/ICourseRepository.cs`

يحدد العمليات التي يجب أن ينفذها Repository الدورات:

- Pagination.
- Count.
- الدورات ضمن نطاق السعر.
- دورة بالـ ID.
- إنشاء.
- تعديل.
- حذف.

### `Repository/IUsersRepository.cs`

يحتوي حاليًا على عملية واحدة:

- `GetUserByEmail`: البحث عن حساب بواسطة البريد الإلكتروني أثناء Login.

### `Repository/IStudentRepository.cs`

يحدد:

- Pagination للطلاب.
- جلب دورات طالب.
- جلب طالب.
- إنشاء.
- تعديل.
- حذف.

الواجهة لا تقول كيف يتم التنفيذ، بل تقول ما العمليات الموجودة فقط.

## 7.6 Interfaces الخاصة بالـ Service

### `Services/ICourseServices.cs`

يحدد واجهة عمليات الدورات التي يراها Controller.

### `Services/IStudentService.cs`

يحدد واجهة عمليات الطلاب التي يراها Controller.

### `Services/IAuthServices.cs`

يحدد عملية `Login(LoginRequestDto dto)` التي تستخدمها `AuthController`.

### `Security/IPasswordHasher.cs`

يفصل عملية كلمات المرور عن Service:

- `HashPassword`: تحويل كلمة المرور إلى hash.
- `VerifyPassword`: مقارنة كلمة المرور المدخلة بالـ hash المخزن.

### `Security/ITokenService.cs`

يحدد عملية `GenerateToken(Users user)` التي تنشئ JWT وترجع التوكن ووقت انتهائه.

الفائدة أن Controller يعتمد على interface وليس على implementation مباشر.

## 7.7 Validators

### `Validetors/CourseDtoValidators.cs`

للإنشاء والتعديل:

- `Title` لا يجوز أن يكون فارغًا.
- `Level` يجب أن يكون قيمة صحيحة من `CourseLevel`.
- `Price` يجب أن يكون أكبر من أو يساوي صفر.
- `InstructorId` يجب أن يكون أكبر من صفر.

هذه القواعد تمنع مثلًا سعرًا سالبًا أو Instructor ID يساوي صفرًا.

لكنها لا تتأكد وحدها أن المدرب موجود فعليًا؛ وجود المدرب يتم التأكد منه عند محاولة الحفظ في قاعدة البيانات، ثم يتحول خطأ Foreign Key إلى `NotFoundException` في Repository.

### `Validetors/StudentDtoValidators.cs`

للإنشاء والتعديل:

- `FullName` مطلوب.
- `FullName` أقصى طول له 100 حرف.
- `Email` مطلوب.
- `Email` يجب أن يكون بصيغة إيميل.

الـ Validator يفحص الشكل، أما uniqueness للإيميل فتفرضه قاعدة البيانات وتتعامل معه Repository.

---

# 8. Infrastructure بالتفصيل

## 8.1 `Data/AppDbContext.cs`

`AppDbContext` يرث من `DbContext`، وهو الجسر بين C# وقاعدة البيانات.

الـ DbSets:

- `Courses`.
- `Students`.
- `Instructors`.
- `Enrollments`.
- `Users`.

عندما تكتب `db.Courses`, فأنت تتعامل مع جدول Courses بطريقة LINQ.

### `OnModelCreating`

```csharp
modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
```

يبحث تلقائيًا عن كل classes التي تطبق `IEntityTypeConfiguration<T>` ويطبق إعداداتها. لذلك لا تحتاج كتابة إعداد كل جدول داخل DbContext نفسه.

### `OnConfiguring` وSeed

يستخدم `UseSeeding` لإضافة بيانات أولية:

- إذا لم يوجد أي User، يضيف ثلاثة حسابات تجريبية:
  - `admin@a.com` بكلمة مرور `admin123` ودور `Admin`.
  - `instructor@i.com` بكلمة مرور `instructor123` ودور `Instructor`.
  - `student@s.com` بكلمة مرور `student123` ودور `Student`.
- كلمات المرور يتم تحويلها إلى BCrypt hash قبل التخزين.

- إذا لم يوجد أي Student، يضيف Ahmed وSara.
- إذا لم يوجد أي Instructor، ينشئ متغيري `hussein` و`maher`.
- ثم يضيف دورتين بـ `InstructorId = 1` و`InstructorId = 2`.

ملاحظة مهمة: الكود ينشئ `hussein` و`maher` داخل متغيرات، لكنه لا يضيفهما إلى context. لذلك الدورات تشير إلى Instructor IDs قد لا تكون موجودة إذا كانت قاعدة البيانات فارغة. يجب الانتباه لهذا السلوك عند التشغيل.

## 8.2 Configurations

### `CourseConfigration.cs`

- المفتاح الأساسي `Id`.
- `Title` مطلوب وأقصى طول 200.
- `Title` فريد Unique.
- `Price` مطلوب.
- `Level` يخزن كنص بدل رقم enum.
- علاقة Course مع Instructor متعددة إلى واحد.
- حذف Instructor ممنوع إذا كان لديه Course بسبب `Restrict`.

### `StudentConfigration.cs`

- `Id` مفتاح أساسي.
- `FullName` مطلوب وأقصاه 100.
- `Email` مطلوب.
- `Email` فريد.

### `InstructorConfigrationcs.cs`

- `Id` مفتاح أساسي.
- `FullName` مطلوب وأقصاه 100.
- Instructor لديه Courses كثيرة.
- حذف Instructor مع وجود Courses ممنوع.

### `EnrollmentConfigration.cs`

- `Id` مفتاح أساسي.
- Unique Index على `(StudentId, CourseId)`؛ يمنع تسجيل نفس الطالب في نفس الدورة مرتين.
- Enrollment يرتبط بـ Course.
- Enrollment يرتبط بـ Student.
- `EnrollmentAt` يحصل افتراضيًا على `now()` من PostgreSQL.

## 8.3 Repositories

### `Repository/CourseRepository.cs`

هذا هو المكان الذي ينفذ SQL فعليًا عبر EF Core.

#### Pagination

يحسب العدد الكامل، ثم:

```csharp
Skip((page - 1) * pageSize)
Take(pageSize)
```

مثال الصفحة 2 وحجم 10: يتخطى أول 10 ويأخذ العشرة التالية.

يستخدم `Select` لإنشاء `CourseResponseDto` داخل الاستعلام، وهذا مفيد لأنه يطلب الأعمدة المطلوبة بدل تحميل كل الكيان.

#### GetCourses وGetCourseById

يستخدم `Include(c => c.Instructor)` حتى يتم تحميل بيانات المدرب، لأن Mapping يحتاج `Instructor.FullName`.

#### CreateCourse

- يضيف الكورس.
- يحفظ التغييرات.
- يحمل Instructor بعد الحفظ.
- يعالج Unique Violation للعنوان برسالة `ConflictException`.
- يعالج Foreign Key Violation للمدرب برسالة `NotFoundException`.

#### UpdateCourse

الخدمة تعدل الكيان المتتبع، ثم Repository يستدعي `SaveChangesAsync`.

#### DeleteCourse

يحذف الكيان ويحفظ.

### `Repository/StudentRepository.cs`

#### Pagination للطلاب

يرتب حسب `Id` ثم يطبق Skip/Take ويحول النتائج مباشرة إلى DTO.

#### دورات الطالب

يبدأ من Courses ويبحث عن:

```csharp
c.Enrollments.Any(e => e.StudentId == studentId)
```

أي دورة لديها Enrollment لهذا الطالب يتم إرجاعها.

#### CRUD الطلاب

يستخدم `FindAsync` لجلب طالب بالـ ID، ويعالج تكرار الإيميل من PostgreSQL كـ `ConflictException`.

## 8.4 Services

### `Services/CourseServices.cs`

هذه طبقة منطق العمل للدورات:

- تمرير عمليات القراءة البسيطة إلى Repository.
- التأكد من وجود الدورة قبل إرجاعها أو تعديلها أو حذفها.
- تشغيل Validator قبل Create وUpdate.
- تحويل DTO إلى Entity.
- تحويل Entity إلى Response DTO.
- تعديل الحقول المطلوبة فقط.

تسلسل إنشاء دورة:

```text
CreateCourseDto
 -> ValidateAndThrowAsync
 -> ToCourseEntity
 -> CourseRepository.CreateCourse
 -> ToCourseResponseDto
 -> Controller returns 201
```

### `Services/StudentService.cs`

نفس الفكرة للطلاب:

- Pagination.
- التأكد من وجود الطالب قبل جلب دوراته.
- Validator عند الإنشاء والتعديل.
- Mapping بين DTO وEntity.
- التعامل مع NotFound.

تسلسل تعديل طالب:

```text
UpdateStudentDto
 -> ValidateAndThrowAsync
 -> GetStudentById
 -> تغيير FullName وEmail
 -> Repository.UpdateStudent
 -> ToStudentResponseDto
```

## 8.5 `Security/JwtSetting.cs`

يحتوي إعدادات JWT:

- `Key`.
- `Issuer`.
- `Audience`.
- `ExpireInHoures`.

يوجد اختلاف تسمية مهم: الكلاس يستخدم `ExpireInHoures`، بينما `appsettings.json` يحتوي `ExpiresInHours`. لذلك لن يتم ربط مدة الانتهاء تلقائيًا بهذا property بسبب اختلاف الاسم. كذلك `Key` غير موجود في `appsettings.json` الحالي.

---

# 9. قاعدة البيانات والعلاقات

الجداول المنطقية:

```text
Instructors
- Id
- FullName

Courses
- Id
- Title
- Price
- Level
- InstructorId -> Instructors.Id

Students
- Id
- FullName
- Email

Enrollments
- Id
- StudentId -> Students.Id
- CourseId -> Courses.Id
- EnrollmentAt
```

العلاقات:

- Instructor واحد لديه Courses كثيرة.
- Course واحد يتبع Instructor واحد.
- Student واحد لديه Enrollments كثيرة.
- Course واحد لديه Enrollments كثيرة.
- Student وCourse بينهما علاقة many-to-many عبر Enrollment.

القيود المهمة:

- Course Title فريد.
- Student Email فريد.
- لا يمكن تكرار نفس `(StudentId, CourseId)`.
- لا يمكن حذف Instructor لديه Course بسبب Restrict.

---

# 9.6 نظام المصادقة والتفويض في Infrastructure

## `Services/AuthService.cs`

هذه الخدمة تنفذ `IAuthServices` وتعمل بالترتيب التالي عند Login:

1. تبحث عن المستخدم بواسطة البريد من خلال `IUsersRepository`.
2. إذا لم يوجد المستخدم أو كانت كلمة المرور غير صحيحة، ترمي `UnauthorizedException` بنفس الرسالة العامة.
3. تستعمل `IPasswordHasher` للتحقق من كلمة المرور بدون مقارنة كلمة المرور الخام بالـ hash يدويًا.
4. تستعمل `ITokenService` لإنشاء JWT.
5. ترجع `LoginResponseDto` الذي يحتوي التوكن ووقت الانتهاء.

## `Repository/UsersRepository.cs`

يبحث عن المستخدم في جدول `Users` بواسطة البريد، ويستخدم `AsNoTracking()` لأن المستخدم يتم قراءته فقط ولا يتم تعديله أثناء Login.

## `Security/BcryptPasswordHasher.cs`

هو التطبيق الفعلي لـ `IPasswordHasher`:

- `HashPassword` يستخدم BCrypt لتخزين hash آمن بدل كلمة المرور الأصلية.
- `VerifyPassword` يستخدم BCrypt لمقارنة كلمة المرور المدخلة بالـ hash.

## `Security/JwtTokenService.cs`

ينشئ JWT من إعدادات `JwtSetting` ويضع داخله Claims:

- `NameIdentifier`: رقم المستخدم.
- `Email`: بريد المستخدم.
- `Role`: دور المستخدم.

وجود Claim الدور هو ما يجعل `[Authorize(Roles = UserRoles.Admin)]` قادرًا على معرفة أن المستخدم Admin.

## `Data/Configration/UserConfigration.cs`

يضبط جدول Users كالتالي:

- `Id` مفتاح أساسي.
- `Email` مطلوب، طوله الأقصى 250، وفريد.
- `FullName` مطلوب، طوله الأقصى 100.
- `PasswordHash` مطلوب.
- `Role` مطلوب، طوله الأقصى 50.

## إعداد JWT في `Program.cs`

يتم تسجيل:

- `IUsersRepository` كـ `UsersRepository`.
- `IPasswordHasher` كـ `BcryptPasswordHasher` بعمر Singleton.
- `ITokenService` كـ `JwtTokenService` بعمر Singleton.
- `IAuthServices` كـ `AuthService` بعمر Scoped.

كما تمت إضافة تعريف Bearer إلى Swagger، لذلك يمكن وضع التوكن من زر Authorize بدل إضافته يدويًا لكل طلب.

## صلاحيات الـ endpoints الحالية

| Endpoint | الصلاحية |
|---|---|
| `POST /api/Auth/login` | متاح للجميع (`AllowAnonymous`) |
| `GET /api/Courses` | متاح للجميع (`AllowAnonymous`) |
| `GET /api/Courses/Count` | متاح للجميع (`AllowAnonymous`) |
| `GET /api/Courses/{id}` | يتطلب `Admin` |
| `POST /api/Courses` | يتطلب `Admin` |
| `PUT /api/Courses/{id}` | يتطلب `Admin` |
| `DELETE /api/Courses/{id}` | يتطلب `Admin` |
| كل `StudentController` | يتطلب توكن JWT صالحًا |

## ملاحظة على إعداد مدة التوكن

`appsettings.json` يحتوي `ExpiresInHours: 8`، بينما `JwtSetting` و`JwtTokenService` يستخدمان property اسمها `ExpireInHoures` ويحسبان المدة بـ `AddMinutes`. يجب الانتباه إلى أن الاسم والوحدة في الإعداد الحاليين لا يطابقان بعضهما؛ الكود الفعلي يستخدم قيمة `ExpireInHoures` إذا تم ربطها.

## ملاحظة أمنية على الحسابات التجريبية

الحسابات وكلمات المرور المذكورة في Seed للتجربة المحلية فقط. لا يجب إبقاؤها كما هي في الإنتاج، كما يجب نقل مفتاح JWT وكلمة اتصال قاعدة البيانات إلى Secrets أو Environment Variables.

---

# 10. شرح الـ Migrations وتسلسلها

الـ Migration هي سجل تاريخي لتغييرات شكل قاعدة البيانات. لا نحذف Migration قديمة عادة؛ كل Migration جديدة تقول لـ EF Core ما الذي تغير منذ السابقة.

التسلسل الموجود حاليًا حسب ملفات المشروع:

1. `20261105134705_InitialCreate.cs`
   - Migration الأساسية للجداول والعلاقات الحالية.
2. `20261105140344_FixUserPasswordHashLength.cs`
   - تعديل طول تخزين `PasswordHash`.
3. `20261105150243_AddUserId.cs`
   - إضافة ربط المستخدم بالبيانات المناسبة.
4. `20261105160000_LinkExistingStudentsToUsers.cs`
   - ربط الطلاب الموجودين بالمستخدمين.
5. `20261107094851_AddCourseImg.cs`
   - إضافة بيانات صورة الدورة.

يجب تطبيق هذه الهجرات بالترتيب حتى تصل قاعدة البيانات إلى الشكل الذي يتوقعه الكود الحالي.

## ملفات `.Designer.cs`

يوجد Designer لبعض الهجرات، مثل:

- `20261105134705_InitialCreate.Designer.cs`
- `20261105140344_FixUserPasswordHashLength.Designer.cs`
- `20261105150243_AddUserId.Designer.cs`
- `20261107094851_AddCourseImg.Designer.cs`

هذه الملفات ينشئها EF Core تلقائيًا. لا تضع فيها منطق Business يدويًا. وظيفتها حفظ snapshot لنموذج EF وقت إنشاء كل Migration.

`AppDbContextModelSnapshot.cs` هو snapshot الحالي الذي يقارن به EF Core النموذج الحالي عندما تنشئ Migration جديدة.

## كيف تطبق الهجرات؟

عند تشغيل:

```powershell
dotnet ef database update --project DalilAcadime.InfraStructures --startup-project DalilAcadime.API
```

يقرأ EF الهجرات بالترتيب ويطبق غير المطبق منها على PostgreSQL. لا تطبق Migration منفردة قديمة على قاعدة فارغة وتتوقع الشكل النهائي؛ التسلسل هو الذي يبني الشكل الحالي.

---

# 11. مسار الطلب بالتفصيل: مثال إنشاء طالب

لنفترض أنك أرسلت:

```text
POST https://localhost:7193/api/Student
```

Body:

```json
{
  "fullName": "Omar Ahmad",
  "email": "omar@example.com"
}
```

ما الذي يحدث؟

1. السيرفر يستقبل الطلب.
2. `UseHttpsRedirection` يتأكد من HTTPS.
3. `MapControllers` يطابق `/api/Student` مع `StudentController.CreateStudent`.
4. ASP.NET Core يحول JSON إلى `CreateStudentDto`.
5. `StudentService` يستلم DTO.
6. `CreateStudentDtoValidator` يفحص الاسم والإيميل.
7. إذا فشل التحقق، يرمي `ValidationException`.
8. `GlobalExceptionHandler` يحولها إلى `400` مع errors.
9. إذا نجح التحقق، `ToStudentEntity` ينشئ Student.
10. `StudentRepository` يضيفه إلى `db.Students`.
11. `SaveChangesAsync` يرسل INSERT إلى PostgreSQL.
12. إذا كان الإيميل مكررًا، PostgreSQL يرجع Unique Violation.
13. Repository يحولها إلى `ConflictException`.
14. Global Handler يرجع `409`.
15. إذا نجح الحفظ، Service يحول Student إلى StudentResponseDto.
16. Controller يرجع `201 Created` مع رابط الطالب.

---

# 12. مسار طلب تعديل دورة

```text
PUT /api/Courses/3
```

1. Controller يستقبل `id` و`UpdateCourseDto`.
2. يوجد استدعاء أول لجلب الدورة لكنه لا يستخدم الناتج.
3. Service يشغل Validator.
4. Service يجلب الدورة مرة ثانية.
5. إذا غير موجودة يرمي `NotFoundException`.
6. يغير Title وPrice وLevel وInstructorId.
7. EF يكون متتبعًا للكيان، لذلك `SaveChangesAsync` يعرف التغييرات.
8. Repository يحفظ.
9. Unique Title أو Foreign Key قد ينتجان 409 أو 404.
10. Service يحول النتيجة إلى Response DTO.
11. Controller يرجع 200.

---

# 13. لماذا يوجد Validation في أكثر من مكان؟

هناك مستويان مختلفان، وليس تكرارًا بلا سبب:

## Validation في FluentValidation

يفحص ما يمكن فحصه قبل قاعدة البيانات:

- النص غير فارغ.
- الطول صحيح.
- السعر غير سالب.
- الإيميل شكله صحيح.
- ID أكبر من صفر.

فائدته إعطاء رسالة واضحة وسريعة للمستخدم بدل إرسال طلب سيئ إلى قاعدة البيانات.

## Constraints في قاعدة البيانات

تحمي البيانات حتى لو جاء الإدخال من مكان آخر غير API:

- Email unique.
- Course Title unique.
- Foreign Keys.
- العلاقات.

فائدة قاعدة البيانات أنها آخر خط دفاع. لا يجب الاعتماد على Validation وحده، لأن طلبًا آخر أو bug قد يتجاوز Service.

---

# 14. الإعدادات والملفات المساندة

## `appsettings.json`

يحتوي:

- Connection String لـ PostgreSQL.
- Issuer وAudience لـ JWT.
- مدة باسم `ExpiresInHours`.
- Logging levels.
- `AllowedHosts`.

> يحتوي الملف الحالي على كلمة مرور PostgreSQL صريحة. لا تضع هذه القيمة في Git أو بيئة الإنتاج. استخدم User Secrets أو Environment Variables أو Secret Manager، وغيّر كلمة المرور إذا تم نشرها.

## `appsettings.Development.json`

ملف إعدادات خاص ببيئة Development. الملف موجود في المشروع، لكن لا توجد فيه إعدادات إضافية مهمة حاليًا.

## `Properties/launchSettings.json`

يحدد طرق التشغيل المحلية والمنافذ ومتغير `ASPNETCORE_ENVIRONMENT=Development`.

## `DalilAcadime.API.http`

ملف لتجربة طلبات HTTP من Visual Studio أو أدوات HTTP، إذا كان يحتوي على requests. لا يغير منطق التطبيق نفسه.

## ملفات `.csproj`

### API packages

- `FluentValidation.DependencyInjectionExtensions`: تسجيل validators.
- `Microsoft.AspNetCore.Authentication.JwtBearer`: JWT.
- `Microsoft.AspNetCore.OpenApi`: OpenAPI.
- EF Core Design وTools: إنشاء وتطبيق migrations.
- `Swashbuckle.AspNetCore`: Swagger UI.

### Core package

- `FluentValidation`: كتابة قواعد التحقق.

### Infrastructure packages

- `BCrypt.Net-Next`: موجود للتشفير/الـ hashing، لكن لا يوجد استخدام ظاهر له في الملفات الحالية.
- `Microsoft.IdentityModel.JsonWebTokens`: أدوات JWT، لكن إنشاء التوكن غير موجود حاليًا.
- `Npgsql.EntityFrameworkCore.PostgreSQL`: مزود EF Core لـ PostgreSQL.

---

# 15. ما الذي يحدث عند الأخطاء؟

## بيانات خاطئة

مثال سعر `-10`:

- Validator يرفضه.
- Exception من FluentValidation.
- الرد `400`.

## ID غير موجود

مثال `GET /api/Courses/99999`:

- Service لا يجد الدورة.
- يرمي `NotFoundException`.
- الرد `404`.

## عنوان دورة مكرر

- قاعدة البيانات تمنع التكرار.
- Npgsql يرمي Unique Violation.
- Repository يحولها إلى `ConflictException`.
- الرد `409`.

## إيميل طالب مكرر

نفس الفكرة، لكن الرسالة `Email must be unique.`.

## خطأ غير متوقع

- Global Handler يسجل الخطأ.
- الرد `500`.
- لا يرسل تفاصيل الاستثناء الداخلية للعميل.

---

# 16. ملاحظات مهمة على الكود الحالي

هذه ليست تغييرات نفذتها؛ هي ملاحظات لفهم الحالة الحالية:

1. **إعداد JWT يحتاج ضبطًا:** أصبح هناك Login فعلي وإنشاء Token وصلاحيات، لكن `Jwt:Key` غير موجود في `appsettings.json`، واسم مدة الانتهاء مختلف بين الإعداد والكلاس، كما أن الكود يستخدم `AddMinutes` رغم أن اسم الإعداد يشير إلى Hours.
2. **Seed المدربين يحتاج مراجعة:** يتم إنشاء `hussein` و`maher` في متغيرين، لكن لا يتم إضافتهما إلى context قبل إنشاء الدورات التي تشير إلى ID 1 و2.
3. **Seed الحسابات للتطوير فقط:** كلمات مرور الحسابات التجريبية مكتوبة داخل `AppDbContext`، وهذا غير مناسب للإنتاج.
4. **استدعاء زائد في التعديل:** Controllers يجلبون العنصر ويضعونه في متغير غير مستخدم قبل استدعاء Service الذي يجلبه مرة ثانية.
5. **فلترة Pagination الدورات:** الاستعلام يفلتر العناصر حسب السعر، لكنه يحسب `TotalCount` من كل الدورات قبل الفلترة؛ لذلك `TotalPages` قد لا يعكس عدد النتائج داخل نطاق السعر.
6. **الـ API يستخدم بعض الكيانات في ActionResult للحذف:** مثل `ActionResult<Course>`، بينما الأفضل عادة إرجاع DTO أو `NoContent` حسب قرار تصميم API.
7. **`float` للأسعار:** الكود الحالي يستخدم `float` وMigration الأخيرة تستخدم PostgreSQL `real`. في الأنظمة المالية عادة `decimal` أدق، لكن هذا ليس ما يفعله المشروع حاليًا.
8. **تاريخ الهجرات يبدو مستقبليًا:** أسماء الملفات تحتوي 2026، وهذا ليس خطأ بحد ذاته إذا كان تاريخ الجهاز/المشروع كذلك.
9. **EnrollmentController يحتوي CRUD:** توجد endpoints للإدارة (`GET`, `GET by id`, `POST`, `PUT`, `DELETE`) محمية بدور Admin، بالإضافة إلى `GET /api/enrollments/me` المحمي بدور Student.
10. **تسجيل الطلاب في FluentValidation:** يوجد سطر تسجيل معلق. بما أن التسجيل الحالي يبدأ من assembly فيه validators، قد يتم اكتشافها، لكن ينبغي التأكد من ذلك باختبار فعلي.
11. **أسماء إملائية:** `Validetors`, `Configration`, `Pagenation`, `Middelwear` أسماء موجودة فعليًا. لا تكسر الكود، لكنها قد تربك المطور الجديد.
12. **الأدوار ليست كلها مستخدمة بنفس الشكل:** `Instructor` و`Student` موجودان في UserRoles ويدخلان في التوكن، لكن عمليات إدارة Enrollment وStudents تستخدم Admin، و`/api/enrollments/me` يستخدم Student.

---

# 17. كيف تختبر المشروع كمبتدئ؟

1. شغل PostgreSQL وأنشئ/تأكد من قاعدة `DalilAcademyDB`.
2. تأكد أن Connection String صحيح.
3. طبق migrations.
4. شغل API من Visual Studio أو `dotnet run`.
5. افتح Swagger على رابط HTTPS.
6. نفذ `POST /api/Auth/login` باستخدام حساب تجريبي.
7. انسخ `token` واضغط **Authorize** في Swagger ثم اكتب `Bearer` وبعدها التوكن.
8. جرّب `GET /api/Courses` بدون تسجيل دخول.
9. جرّب `GET /api/Student` بعد وضع التوكن.
10. أنشئ طالبًا صحيحًا بعد تسجيل الدخول.
11. جرّب إيميلًا مكررًا لترى `409`.
12. جرّب طالبًا باسم فارغ لترى `400`.
13. جرّب ID غير موجود لترى `404`.
14. سجّل الدخول بحساب Admin ثم جرّب إنشاء دورة.
15. جرّب إنشاء دورة بحساب Student لترى رفض الصلاحية.
16. جرّب إنشاء دورة بـ Instructor ID غير موجود لترى خطأ العلاقة.
17. سجّل الدخول بحساب Admin ثم جرّب `POST /api/enrollments` بهذا body:

```json
{
  "studentId": 1,
  "courseId": 1
}
```

18. جرّب `GET /api/enrollments` و`GET /api/enrollments/{id}`.
19. جرّب `PUT /api/enrollments/{id}` ثم `DELETE /api/enrollments/{id}`.
20. سجّل الدخول بحساب Student ثم جرّب `GET /api/enrollments/me` لرؤية دوراته فقط.

أوامر مفيدة:

```powershell
dotnet restore
dotnet build
dotnet run --project DalilAcadime.API
dotnet ef database update --project DalilAcadime.InfraStructures --startup-project DalilAcadime.API
```

---

# 18. ملخص سريع جدًا

إذا أردت حفظ الفكرة في رأسك:

- `API` يستقبل الطلب.
- `Controller` يوجه الطلب.
- `Service` يفكر ويقرر.
- `Validator` يفحص البيانات.
- `Mapping` يغير شكل البيانات.
- `Repository` يتكلم مع قاعدة البيانات.
- `DbContext` هو الجسر إلى PostgreSQL.
- `Entity` تمثل جداول قاعدة البيانات.
- `DTO` يمثل ما يدخل ويخرج من API.
- `Migration` تحفظ تاريخ تغييرات الجداول.
- `ExceptionHandler` يحول الأخطاء إلى ردود HTTP مفهومة.
- `Program.cs` يركب كل القطع ويحدد ترتيب التشغيل.

هذا هو مسار الطلب كاملًا:

```text
HTTP Request
 -> Middleware
 -> Controller
 -> Service
 -> Validator / Mapping
 -> Repository
 -> DbContext
 -> PostgreSQL
 -> Repository
 -> Service
 -> Controller
 -> HTTP Response
```

## 18.1 شرح مسار الطلب بالتفصيل

يوضح المسار السابق كيف ينتقل الطلب داخل التطبيق من لحظة وصوله من العميل حتى رجوع النتيجة إليه. المثال التالي ينطبق على معظم العمليات، مثل تسجيل الدخول، إنشاء Course، أو إنشاء Enrollment.

### 1. HTTP Request

يبدأ المسار عندما يرسل العميل طلب HTTP إلى أحد المسارات المسجلة في الـ API. يمكن أن يكون العميل تطبيق Frontend أو Postman أو Swagger أو أي تطبيق آخر.

يتكون الطلب عادةً من:

- **HTTP Method** مثل `GET` أو `POST` أو `PUT` أو `DELETE`.
- **URL** مثل `/api/enrollments`.
- **Headers** مثل `Authorization` و`Content-Type`.
- **Route Parameters** مثل `id` في `/api/enrollments/{id}`.
- **Query Parameters** مثل `page` و`pageSize`.
- **Request Body** في عمليات الإنشاء أو التعديل، وغالبًا يكون بصيغة JSON.

مثال على طلب إنشاء تسجيل:

```http
POST /api/enrollments HTTP/1.1
Content-Type: application/json
Authorization: Bearer <JWT_TOKEN>
```

```json
{
  "studentId": 1,
  "courseId": 1
}
```

### 2. Middleware

بعد وصول الطلب إلى ASP.NET Core، يمر أولًا عبر سلسلة الـ Middleware التي تم ترتيبها في `Program.cs`. كل Middleware تنفذ مهمة معينة، وقد تسمح للطلب بالاستمرار أو توقفه وتعيد Response مباشرة.

من أهم ما يحدث في هذه المرحلة:

- **Exception Handling:** التقاط الأخطاء غير المعالجة وتحويلها إلى Response منظم بدل ظهور خطأ داخلي غير مفهوم.
- **HTTPS Redirection:** إعادة توجيه الطلب من HTTP إلى HTTPS عندما يكون ذلك مفعّلًا.
- **Authentication:** قراءة JWT من `Authorization` والتأكد من صحة التوقيع والـ Issuer والـ Audience ومدة الصلاحية.
- **Authorization:** التأكد من أن Role المستخدم يسمح له بتنفيذ الـ endpoint، مثل اشتراط Role `Admin`.
- **Routing:** تحديد الـ Controller والـ Action المناسبين بناءً على الـ URL والـ HTTP Method.
- **Swagger:** توفير واجهة ووصف للمسارات في بيئة التطوير، ولا يغير منطق معالجة الطلب نفسه.

إذا لم يرسل المستخدم Token، أو كان Token غير صحيح أو منتهيًا، يمكن أن ينتهي الطلب هنا بنتيجة `401 Unauthorized`. وإذا كان المستخدم مسجلًا لكن Role الخاص به غير مسموح، تكون النتيجة `403 Forbidden`.

### 3. Controller

بعد نجاح الـ Middleware، يصل الطلب إلى الـ Controller المناسب. وظيفة الـ Controller هي التعامل مع طبقة HTTP فقط، وليس تنفيذ منطق قاعدة البيانات مباشرة.

يقوم الـ Controller عادةً بالمهام التالية:

- قراءة Route Parameters وQuery Parameters وRequest Body.
- تحويل البيانات القادمة إلى DTO مناسب.
- استدعاء Service عن طريق Interface مسجل في Dependency Injection.
- تحويل النتيجة إلى HTTP Status Code مناسب مثل `200`, `201`, `204`, `400`, `404` أو `409`.
- إعادة Response إلى العميل.

مثلًا، عند استدعاء `POST /api/enrollments`، يستقبل `EnrollmentController` كائن `CreateEnrollmentDto` ثم يستدعي `IEnrollmentService.Create(...)`. لا يجب أن يحتوي الـ Controller على استعلامات EF Core أو تفاصيل PostgreSQL.

### 4. Service

الـ Service هو طبقة منطق الأعمال. يستقبل الطلب من الـ Controller ويطبق القواعد التي تحدد هل العملية مسموحة وصحيحة أم لا.

في حالة Enrollment مثلًا، يمكن أن يقوم `EnrollmentService` بالآتي:

- التأكد من أن `StudentId` أكبر من صفر.
- التأكد من أن `CourseId` أكبر من صفر.
- استدعاء Repository لتنفيذ العملية.
- تحويل أخطاء عدم وجود التسجيل إلى `NotFoundException`.
- تمرير البيانات الصحيحة إلى Repository.
- إعادة النتيجة إلى Controller بصيغة DTO.

الـ Service لا يتعامل مع HTTP مباشرة، لذلك لا يعيد `IActionResult` ولا يعرف تفاصيل Response. هذه المسؤولية تبقى في Controller.

### 5. Validator / Mapping

في هذه المرحلة يتم فحص شكل البيانات وتحويلها بين الطبقات عند الحاجة.

#### Validator

الـ Validator يفحص صحة البيانات قبل تنفيذ العملية، مثل:

- هل الحقول المطلوبة موجودة؟
- هل النص ضمن الطول المسموح؟
- هل القيمة رقم موجب؟
- هل البريد الإلكتروني بصيغة صحيحة؟

إذا فشل التحقق، يتوقف الطلب وتكون النتيجة غالبًا `400 Bad Request`، ولا يتم إرسال استعلام إلى قاعدة البيانات.

#### Mapping

الـ Mapping يحول البيانات من شكل إلى آخر:

- من Request DTO إلى Entity أو إلى نموذج داخلي.
- من Entity إلى Response DTO.
- من بيانات قاعدة البيانات إلى الشكل الذي يجب إرساله للعميل.

استخدام DTOs يمنع كشف Entity الداخلية مباشرة، ويجعل شكل الـ API مستقلًا عن شكل جداول قاعدة البيانات.

### 6. Repository

الـ Repository هو الطبقة المسؤولة عن الوصول إلى البيانات. يستقبل أمرًا واضحًا من Service وينفذه باستخدام `AppDbContext`.

في `EnrollmentRepository` مثلًا توجد عمليات:

- جلب كل التسجيلات.
- جلب تسجيل حسب `id`.
- إنشاء تسجيل جديد.
- تعديل تسجيل موجود.
- حذف تسجيل.
- جلب دورات الطالب الحالي من خلال `UserId`.
- التأكد من وجود Student وCourse.
- منع التسجيل المكرر.

الـ Repository لا يقرر صلاحيات المستخدم، ولا يقرأ HTTP Request، ولا ينشئ JWT. هذه المسؤوليات موجودة في Middleware أو Controller أو Service حسب الحالة.

### 7. DbContext

`AppDbContext` هو الجسر بين كود C# وقاعدة البيانات. يحتوي على `DbSet` للجداول، مثل:

- `Users`
- `Students`
- `Courses`
- `Instructors`
- `Enrollments`

عندما ينفذ Repository استعلام LINQ مثل `Where` أو `FirstOrDefaultAsync`، يقوم EF Core بتحويله إلى SQL مناسب. وعند الإضافة أو التعديل، يتم تجهيز التغيير داخل الـ DbContext إلى أن يتم استدعاء `SaveChangesAsync()`.

الـ DbContext عادةً يكون مسجلًا بعمر `Scoped`، أي يتم إنشاء نسخة لكل HTTP Request ثم التخلص منها عند انتهاء الطلب. هذا يمنع مشاركة نفس جلسة EF Core بين طلبات متعددة.

### 8. PostgreSQL

بعد أن يترجم EF Core الاستعلام إلى SQL، يتم إرساله إلى PostgreSQL عن طريق مزود `Npgsql`.

في هذه المرحلة تنفذ قاعدة البيانات عمليات مثل:

- قراءة الصفوف المطلوبة.
- تطبيق شروط `WHERE`.
- تنفيذ العلاقات بين الجداول باستخدام `JOIN`.
- التحقق من Primary Keys وForeign Keys.
- تطبيق Unique Constraints.
- إدخال أو تعديل أو حذف البيانات.

إذا خالفت العملية قيدًا في قاعدة البيانات، مثل محاولة إنشاء Enrollment مكرر، يتم التقاط الخطأ في Repository وتحويله إلى Exception مناسبة مثل `ConflictException`، ثم يرجع للعميل عادةً كـ `409 Conflict`.

### 9. الرجوع من PostgreSQL إلى Repository

بعد تنفيذ الاستعلام، تعيد PostgreSQL البيانات أو نتيجة العملية إلى EF Core. يقوم `AppDbContext` بتحويل الصفوف إلى Entities أو نتائج Projection.

بعد ذلك يقوم Repository بالمهام التالية عند الحاجة:

- تحويل Entity إلى Response DTO.
- إخفاء تفاصيل قاعدة البيانات عن الطبقات الأعلى.
- معالجة عدم وجود البيانات.
- ترجمة أخطاء قاعدة البيانات المعروفة إلى Exceptions مفهومة.
- إعادة النتيجة إلى Service.

في عمليات القراءة، قد يستخدم Repository Projection مباشرة إلى DTO لتقليل البيانات المقروءة وتحسين قابلية ترجمة الاستعلام إلى SQL.

### 10. الرجوع من Repository إلى Service

يستقبل Service نتيجة Repository ويتعامل معها وفق منطق الأعمال. قد يقوم مثلًا بـ:

- التحقق من أن النتيجة ليست `null`.
- تحويل عدم وجود السجل إلى `NotFoundException`.
- تمرير DTO كما هو إلى Controller.
- تطبيق قواعد إضافية لا علاقة لها بتفاصيل قاعدة البيانات.

بهذا يبقى Service مستقلًا عن تفاصيل EF Core، ويبقى Repository مستقلًا عن تفاصيل HTTP.

### 11. الرجوع من Service إلى Controller

يستقبل Controller النتيجة النهائية من Service، ثم يحدد نوع HTTP Response المناسب.

أمثلة:

- `200 OK`: تم جلب البيانات أو تعديلها بنجاح.
- `201 Created`: تم إنشاء مورد جديد بنجاح.
- `204 No Content`: تم الحذف بنجاح ولا توجد بيانات لإرجاعها.
- `400 Bad Request`: البيانات غير صحيحة.
- `401 Unauthorized`: لا يوجد Token صحيح.
- `403 Forbidden`: المستخدم لا يملك الصلاحية.
- `404 Not Found`: العنصر المطلوب غير موجود.
- `409 Conflict`: تعارض مثل التسجيل المكرر.

### 12. HTTP Response

في النهاية يعيد Controller Response إلى العميل. يتكون الرد عادةً من:

- Status Code.
- Headers.
- Response Body بصيغة JSON عند وجود بيانات.

مثال Response ناجح لإنشاء Enrollment:

```http
HTTP/1.1 201 Created
Content-Type: application/json
```

```json
{
  "id": 10,
  "studentId": 1,
  "courseId": 1,
  "enrollmentAt": "2026-01-01T10:00:00Z"
}
```

## 18.2 مثال عملي كامل: إنشاء Enrollment

عند تنفيذ `POST /api/enrollments`، يكون التسلسل التفصيلي كالتالي:

1. يرسل العميل `studentId` و`courseId` مع JWT.
2. يمر الطلب عبر Middleware.
3. يتم التحقق من JWT ومن أن Role المستخدم هو `Admin`.
4. يطابق Routing الطلب مع `EnrollmentController.Create`.
5. يستقبل Controller كائن `CreateEnrollmentDto`.
6. يستدعي Controller الدالة الموجودة في `IEnrollmentService`.
7. يتحقق `EnrollmentService` من أن أرقام Student وCourse صحيحة.
8. يستدعي Service الدالة المناسبة في `IEnrollmentRepository`.
9. يتحقق Repository من وجود Student وCourse.
10. يتحقق Repository من عدم وجود Enrollment مكرر.
11. يضيف Repository التسجيل إلى `AppDbContext` ويستدعي `SaveChangesAsync`.
12. يرسل EF Core أمر `INSERT` إلى PostgreSQL.
13. تعيد PostgreSQL التسجيل الجديد ورقم `Id`.
14. يحول Repository النتيجة إلى `EnrollmentResponseDto`.
15. يعيد Service الـ DTO إلى Controller.
16. يعيد Controller `201 Created` مع بيانات التسجيل.
17. يستلم العميل HTTP Response بصيغة JSON.

إذا فشلت أي خطوة، يتم إيقاف المسار وإرجاع النتيجة المناسبة. مثلًا، عدم وجود Student يؤدي إلى `404 Not Found`، والتسجيل المكرر يؤدي إلى `409 Conflict`، وعدم وجود JWT يؤدي إلى `401 Unauthorized`.

---

# 19. آخر تحديث: CRUD الخاص بـ Enrollment

تم فصل منطق التسجيلات عن Student وإضافة طبقات مستقلة:

- `Core/DTOs/EnrollmentDTO.cs`
- `Core/Repository/IEnrollmentRepository.cs`
- `Core/Services/IEnrollmentService.cs`
- `InfraStructures/Repository/EnrollmentRepository.cs`
- `InfraStructures/Services/EnrollmentService.cs`
- `API/Controllers/EnrollmentController.cs`

## Endpoints الإدارة

كل العمليات التالية تحتاج Role `Admin`:

| العملية | المسار | Body |
|---|---|---|
| عرض كل التسجيلات | `GET /api/enrollments` | لا يوجد |
| عرض تسجيل واحد | `GET /api/enrollments/{id}` | لا يوجد |
| إنشاء تسجيل | `POST /api/enrollments` | `studentId`, `courseId` |
| تعديل تسجيل | `PUT /api/enrollments/{id}` | `studentId`, `courseId` |
| حذف تسجيل | `DELETE /api/enrollments/{id}` | لا يوجد |

مثال إنشاء أو تعديل:

```json
{
  "studentId": 1,
  "courseId": 1
}
```

الخدمة تتحقق من أن الطالب والدورة موجودان، وتمنع تكرار التسجيل لنفس الطالب والدورة. الأخطاء المتوقعة:

- `400` إذا كانت IDs غير صحيحة.
- `404` إذا لم يوجد الطالب أو الدورة أو التسجيل.
- `409` إذا كان التسجيل موجودًا مسبقًا.

## Endpoint الطالب الحالي

```text
GET /api/enrollments/me
```

يحتاج Role `Student`. لا يستقبل `StudentId` من العميل؛ يأخذ User ID من Claim داخل JWT، ثم يبحث عن الطالب المرتبط بهذا المستخدم ويرجع دوراته المسجل بها.

---

# 20. Flow لعدة Requests عبر أكثر من Controller

كل Request مستقل عن الآخر، لكن العميل يستخدم نتيجة Request سابق في الطلب التالي، وأهم نتيجة تنتقل بين الطلبات هي JWT.

## 20.1 تسجيل مستخدم ثم جلب الدورات ثم قراءة دوراته

```text
Client
  |
  | 1) POST /api/Auth/register
  v
AuthController.Register
  -> AuthService
  -> PasswordHasher: تشفير كلمة المرور
  -> StudentRepository
  -> PostgreSQL: INSERT Student/User
  -> TokenService: إنشاء JWT بدور Student
  |
  | 2) يستلم Token ويحفظه العميل
  |
  | 3) GET /api/Courses?page=1&pageSize=10
  |    Authorization: Bearer <StudentToken>
  v
CoursesController.GetCoursesPagenation
  -> CourseService
  -> CourseRepository
  -> PostgreSQL: SELECT Courses
  -> HTTP 200: PageResult<CourseResponseDto>
  |
  | 4) GET /api/Enrollment/me
  |    Authorization: Bearer <StudentToken>
  v
EnrollmentController.GetMyCourses
  -> قراءة UserId من ClaimTypes.NameIdentifier
  -> EnrollmentService
  -> EnrollmentRepository
  -> PostgreSQL: SELECT Courses عبر Enrollments
  -> HTTP 200: دورات الطالب
```

في هذا السيناريو لا ينتقل الطلب من Controller إلى Controller مباشرة. كل Request يمر من Middleware ثم يصل إلى Controller مستقل، والربط بين الطلبات يحدث في العميل من خلال JWT.

## 20.2 Admin: تسجيل الدخول ثم إدارة دورة ورفع صورتها

```text
Client
  |
  | 1) POST /api/Auth/login
  v
AuthController.Login
  -> AuthService
  -> UsersRepository: البحث بالبريد
  -> PasswordHasher: التحقق من كلمة المرور
  -> TokenService: JWT بدور Admin
  -> HTTP 200: Token
  |
  | 2) POST /api/Courses
  |    Authorization: Bearer <AdminToken>
  v
CoursesController.CreateCourse
  -> CourseService
  -> Validator
  -> CourseRepository
  -> PostgreSQL: INSERT Course
  -> HTTP 201: CourseResponseDto
  |
  | 3) POST /api/Courses/upload-image
  |    multipart/form-data: file
  |    Authorization: Bearer <AdminToken>
  v
CoursesController.UploadImage
  -> ImgService
  -> التحقق من الحجم والامتداد
  -> wwwroot/img/courses: حفظ الملف باسم Guid
  -> HTTP 200: { image: "/img/courses/..." }
  |
  | 4) PUT /api/Courses/{id}
  |    Authorization: Bearer <AdminToken>
  v
CoursesController.UpdateCourse
  -> CourseService
  -> Validator
  -> CourseRepository
  -> PostgreSQL: UPDATE Course
  -> HTTP 200: CourseResponseDto
```

رفع الصورة Request منفصل عن إنشاء الدورة؛ لذلك لا يقوم `CreateCourse` بحفظ الملف، ولا يقوم `UploadImage` بتعديل بيانات الدورة أو قاعدة البيانات. الرابط الناتج من رفع الصورة يمكن أن يستخدمه العميل عند الحاجة حسب عقد DTO والـ entity الحاليين.

## 20.3 Admin: إنشاء طالب ثم إنشاء تسجيل له في دورة

```text
Client يحمل AdminToken
  |
  | 1) POST /api/Student
  v
StudentController.CreateStudent
  -> StudentService
  -> CreateStudentDtoValidator
  -> PasswordHasher
  -> StudentRepository
  -> PostgreSQL: INSERT Student
  -> HTTP 201: TokenizedStudentDto
  |
  | 2) POST /api/Enrollment
  |    { "studentId": 1, "courseId": 1 }
  v
EnrollmentController.Create
  -> EnrollmentService
  -> CreateEnrollmentDtoValidator
  -> EnrollmentRepository
  -> التحقق من Student وCourse وعدم التكرار
  -> PostgreSQL: INSERT Enrollment
  -> HTTP 201: EnrollmentResponseDto
  |
  | 3) GET /api/Student/1/courses
  v
StudentController.GetStudentCourses
  -> StudentService
  -> التحقق من وجود Student
  -> StudentRepository
  -> PostgreSQL: SELECT Courses عبر Enrollments
  -> HTTP 200: قائمة دورات الطالب
```

الطلب الثاني يعتمد على `studentId` الناتج من الطلب الأول، لكنه لا يستدعي `StudentController` داخليًا. Controller الثاني يبدأ دورة معالجة جديدة ويستخدم Repository الخاص به.

## 20.4 تدفق قراءة تفاصيل متعددة من Controllers مختلفة

```text
1) GET /api/Courses/{id}
   -> CoursesController.GetCourseById
   -> CourseService -> CourseRepository -> PostgreSQL
   -> CourseResponseDto

2) GET /api/Student/{id}
   -> StudentController.GetStudentById
   -> StudentService -> StudentRepository -> PostgreSQL
   -> StudentResponseDto

3) GET /api/Enrollment
   -> EnrollmentController.GetAll
   -> EnrollmentService -> EnrollmentRepository -> PostgreSQL
   -> EnrollmentResponseDto[]
```

يمكن للواجهة الأمامية تنفيذ هذه الطلبات بالتتابع أو بالتوازي حسب اعتماد البيانات بينها. كل طلب يحصل على `DbContext` بعمر `Scoped` خاص به، ولا تتم مشاركة نفس instance بين Requests مختلفة.

## 20.5 أين يتوقف أي Request؟

```text
Request
  -> ExceptionHandler / HTTPS / StaticFiles / Authentication / Authorization
	  |
	  +-- JWT مفقود أو غير صالح ------> 401
	  +-- Role غير مسموح --------------> 403
	  +-- ملف صورة غير صالح -----------> InvalidFileFormatException -> 500 حاليًا
	  +-- Route صحيح ------------------> Controller
									  -> Service / Validator
									  -> Repository / PostgreSQL
									  -> Response
```

أخطاء `ValidationException` و`NotFoundException` و`ConflictException` و`UnauthorizedException` يتم تحويلها بواسطة `GlobalExceptionHandler` إلى `400` أو `404` أو `409` أو `401` حسب نوع الخطأ.
