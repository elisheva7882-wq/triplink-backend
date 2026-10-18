# תוכנית פיתוח TripLink

## 1. מטרת המסמך וגבולות העבודה

מסמך זה הוא תוכנית בלבד. בשלב הנוכחי נוצר רק PROJECT_PLAN.md לצד TripLink.sln. כל יצירה, שינוי שם, התקנת חבילות, Migration או שינוי קוד המתוארים בהמשך מיועדים לשלבי פיתוח עתידיים.

המערכת תאפשר פרסום נסיעות, חיפוש נסיעות והרשמה של משתמשים לנסיעות, תוך שמירת קיבולת והרשאות גם בעת בקשות מקבילות. דרישות מוצר שטרם אושרו מסומנות כהצעות או כשאלות פתוחות.

## 2. מבנה הפתרון הנוכחי

```text
TripLink.sln
TripLink/
  TripLink.csproj
  Program.cs
  WeatherForecast.cs
  Controllers/WeatherForecastController.cs
  Properties/launchSettings.json
  appsettings.json
  appsettings.Development.json
  TripLink.http
```

TripLink.sln מכיל פרויקט Web API יחיד בשם TripLink. הפרויקט משתמש ב־net8.0, עם Nullable ו־ImplicitUsings פעילים ועם Swashbuckle.AspNetCore בגרסה 6.6.2. Program.cs רושם Controllers ו־Swagger, מפעיל HTTPS redirection ו־Authorization וממפה Controllers. טרם מוגדרים בו Authentication, מסד נתונים או שירותים עסקיים. WeatherForecastController ו־WeatherForecast הם קובצי דוגמה.

בשלב עתידי נשנה את שם הפרויקט TripLink ל־TripLink.API, או ניצור TripLink.API ונעביר אליו את נקודת הכניסה וההגדרות. ההחלטה תתקבל לפני שלב הקמת השכבות. אין לבצע זאת כעת.

## 3. מבנה היעד

```text
TripLink.sln
PROJECT_PLAN.md
TripLink.Core/
  Entities/           User.cs, Ride.cs, Registration.cs
  Enums/              RideStatus.cs, RegistrationStatus.cs
  Contracts/          RideSearchQuery.cs
  Interfaces/         IRideRepository.cs, IRegistrationRepository.cs
                      IUserRepository.cs, IUnitOfWork.cs
  Exceptions/         NotFoundException.cs, ConflictException.cs
                      ForbiddenException.cs, BusinessValidationException.cs
TripLink.Data/
  TripLinkDbContext.cs
  Configurations/     UserConfiguration.cs, RideConfiguration.cs
                      RegistrationConfiguration.cs
  Repositories/       RideRepository.cs, RegistrationRepository.cs
                      UserRepository.cs
  Transactions/       UnitOfWork.cs
  Migrations/
TripLink.Service/
  DTOs/               CreateRideRequest.cs, UpdateRideRequest.cs
                      RideResponse.cs, RegistrationResponse.cs
                      RegisterRequest.cs, LoginRequest.cs, AuthResponse.cs
  Interfaces/         IRideService.cs, IRegistrationService.cs
                      IAuthService.cs, ITokenService.cs
  Services/           RideService.cs, RegistrationService.cs, AuthService.cs
  Security/           JwtTokenService.cs, JwtOptions.cs
TripLink.API/
  Controllers/        RidesController.cs, RegistrationsController.cs
                      AuthController.cs
  Middleware/         ExceptionHandlingMiddleware.cs
  Program.cs
  appsettings.json
  appsettings.Development.json
  nlog.config
  Properties/launchSettings.json
  TripLink.API.http
TripLink.Tests/
  Unit/               RideServiceTests.cs, RegistrationServiceTests.cs
                      AuthServiceTests.cs
  Integration/        TripLinkApiFactory.cs, TestDatabaseFixture.cs
                      RidesApiTests.cs, AuthApiTests.cs
                      ErrorHandlingTests.cs, RegistrationConcurrencyTests.cs
```

בכל תיקיית פרויקט יהיה קובץ csproj בשם הפרויקט. קובצי הדוגמה יוסרו רק בשלב הפיתוח המתאים.

### אחריות ותלויות

| פרויקט | אחריות | Project References |
|---|---|---|
| TripLink.Core | ישויות, חוזי גישה לנתונים, שגיאות עסקיות וערכי מצב | ללא הפניה לפרויקט אחר |
| TripLink.Data | EF Core, מיפוי טבלאות, שאילתות ועסקאות | TripLink.Core |
| TripLink.Service | מקרי שימוש, DTOs, כללים עסקיים ויצירת JWT | TripLink.Core |
| TripLink.API | HTTP, Controllers, middleware והרכבת DI | TripLink.Core, TripLink.Service, TripLink.Data |
| TripLink.Tests | בדיקות שירותים ושילוב עם API ומסד הנתונים | ארבעת הפרויקטים לפי הצורך |

TripLink.Service יעבוד מול ממשקים שב־TripLink.Core ולא יפנה ישירות ל־TripLink.Data. TripLink.API יחבר ב־Program.cs בין הממשקים למימושים. Controllers יהיו דקים ויעבירו פעולות לשירותים. המבנה מפריד בין HTTP, כללים עסקיים ואחסון, ומתאים לשכבות הנדרשות בקורס.

## 4. החלטות מוצעות לתכנון

- להשאיר net8.0 בשלב הראשון כדי להתאים לפרויקט הקיים; התאמה לדרישות הקורס תיבדק לפני התקנת חבילות.
- להשתמש ב־EF Core עם SQL Server, וב־LocalDB לפיתוח ב־Visual Studio. זו הצעה הטעונה התאמה לדרישות הקורס.
- להשתמש ב־xUnit וב־Microsoft.AspNetCore.Mvc.Testing. לבדיקות מקביליות להשתמש במסד SQL Server ייעודי לבדיקות, ולא ב־EF Core InMemory.
- כל הרשמה מייצגת נוסע אחד ומקום אחד. ביטול הרשמה יאפשר הפעלה מחדש של אותה רשומה.
- משתמש מחובר יוכל לפרסם נסיעה ולהירשם לנסיעה של משתמש אחר. הרשאת ניהול מערכת אינה נדרשת ללא דרישה נוספת.
- לשמור זמנים ב־UTC, ולהחזיר ISO 8601. תצוגת שעה מקומית היא אחריות הלקוח.

אלה הנחות עבודה למסמך, ולא תחליף לאישור דרישות המוצר.

## 5. שלבי הפיתוח לפי סדר

### שלב 1 — הקמת הפרויקטים וההפניות

**מטרה:** להכין פתרון עם חמש השכבות הנדרשות, לפני פיתוח התנהגות עסקית.

**מיקום וקבצים:** TripLink.sln, קובצי csproj של חמשת הפרויקטים, TripLink.API/Program.cs, TripLink.API/Properties/launchSettings.json וקובצי התצורה של API. במסגרת השלב העתידי בלבד יטופלו קובצי TripLink הקיימים ושמותיהם.

**מחלקות ומתודות:** בשלב זה אין צורך במחלקות עסקיות חדשות. Program.cs יישאר נקודת הכניסה. אם נבחר שינוי שם, יש לעדכן את שם הפרויקט, התיקייה, קובץ csproj, namespaces וההפניה בפתרון. אם נבחר יצירה מחדש, יש להעביר את התצורה הנחוצה ולוודא שהפתרון מפנה לפרויקט החדש לפני הסרת הישן.

**נימוק:** הקמת התלויות תחילה מונעת הפניות הפוכות וממקמת כל רכיב בשכבה המתאימה.

**Visual Studio:** לפתוח TripLink.sln; באמצעות Add > New Project להוסיף שלושה פרויקטי Class Library בשם TripLink.Core, TripLink.Data ו־TripLink.Service, ופרויקט xUnit בשם TripLink.Tests. לבצע את המעבר המתוכנן ל־TripLink.API. באמצעות Add > Project Reference להגדיר את ההפניות שבטבלה, ולהגדיר את TripLink.API כ־Startup Project.

**הרצה ואימות:** Build > Build Solution, ואז F5 עם פרופיל HTTPS. לבדוק ש־Swagger עולה ושאין הפניות שבורות. לוודא את שמות כל חמשת הפרויקטים ב־Solution Explorer. הסרת WeatherForecast.cs ו־WeatherForecastController.cs תיעשה בעת הוספת API אמיתי בשלב 7.

### שלב 2 — מודלים וחוזים ב־TripLink.Core

**מטרה:** להגדיר שפה משותפת לנתונים ולעסקאות לפני מימוש האחסון.

**מיקום וקבצים:** כל הקבצים בתיקיות Entities, Enums, Contracts, Interfaces ו־Exceptions של TripLink.Core המפורטים במבנה היעד.

**מחלקות וממשקים:** User, Ride, Registration; הערכים RideStatus ו־RegistrationStatus; RideSearchQuery; ארבע מחלקות השגיאה; IRideRepository עם GetByIdAsync(), SearchAsync(), AddAsync(); IRegistrationRepository עם GetByIdAsync(), GetByRideAndUserAsync(), CountActiveByRideAsync(), AddAsync(); IUserRepository עם GetByIdAsync(), GetByNormalizedEmailAsync(), AddAsync(); IUnitOfWork עם ExecuteSerializableAsync() ו־SaveChangesAsync().

המתודה ExecuteSerializableAsync() תקבל פעולה אסינכרונית ו־CancellationToken. הפעולה תכיל את הקריאות, שינויי הישויות והשמירה באותו DbContext ובאותה עסקה. חוזה ההחזרה יהיה גנרי לפי תוצאת הפעולה. כל מתודת I/O תקבל CancellationToken. הישויות יכילו את השדות בסעיף מסד הנתונים; כללי מקרי השימוש ימומשו בשירותים.

**נימוק:** חוזים עצמאיים מאפשרים לפתח שירותים ללא תלות במימוש EF Core, ולהחליף מאגרי נתונים בבדיקות יחידה.

**Visual Studio:** ליצור תיקיות באמצעות Add > New Folder, וקובצי Class ו־Interface באמצעות Add > New Item. לבדוק שכל namespace מתחיל ב־TripLink.Core ושאין הפניה ל־EF Core או ASP.NET Core.

**הרצה ואימות:** לבנות את TripLink.Core ואת הפתרון; לבדוק שממשקי המאגרים מתייחסים לישויות ול־RideSearchQuery בלבד, ושאין תלות ב־DTOs של שירות או ב־HTTP.

### שלב 3 — מסד הנתונים ו־TripLink.Data

**מטרה:** להקים סכימה, מיפוי ועסקאות שמסוגלות לאכוף עקביות.

**מיקום וקבצים:** TripLink.Data/TripLinkDbContext.cs, Configurations/*.cs, Repositories/*.cs, Transactions/UnitOfWork.cs, Migrations/*; רישום DbContext ב־TripLink.API/Program.cs. Connection string לפיתוח יישמר באמצעות User Secrets של TripLink.API.

**מחלקות ומתודות:** TripLinkDbContext עם OnModelCreating(); UserConfiguration, RideConfiguration ו־RegistrationConfiguration עם Configure(); מימושי RideRepository, RegistrationRepository ו־UserRepository למתודות של שלב 2; UnitOfWork עם ExecuteSerializableAsync() ו־SaveChangesAsync(). כל המאגרים ו־UnitOfWork יקבלו אותו TripLinkDbContext במתחם Scoped.

להוסיף Microsoft.EntityFrameworkCore.SqlServer ו־Microsoft.EntityFrameworkCore.Design בגרסאות תואמות ל־net8.0 ולדרישות הקורס; להוסיף כלי EF הנדרש ל־Package Manager Console. להגדיר rowversion, אינדקסים ואילוצים כמפורט בהמשך. לטפל בסכסוך rowversion ובכשלי עסקה צפויים באמצעות תרגום לשגיאות עסקיות מוגדרות, ללא זליגת פרטי SQL ללקוח. מדיניות retry תהיה מוגבלת ותפעיל מחדש את כל העסקה עם מצב נקי.

**נימוק:** אילוצי מסד מגינים גם כאשר שתי בקשות עברו בדיקות עסקיות במקביל. עסקת Serializable מאפשרת לספור הרשמות פעילות ולהוסיף הרשמה באופן עקבי.

**Visual Studio:** Manage NuGet Packages להתקנת חבילות; Manage User Secrets בפרויקט API להגדרת ConnectionStrings:TripLinkDb. ב־Package Manager Console להריץ Add-Migration InitialCreate -Project TripLink.Data -StartupProject TripLink.API, לבדוק את קובצי ה־Migration, ואז Update-Database -Project TripLink.Data -StartupProject TripLink.API. לפתוח SQL Server Object Explorer לבדיקת הסכימה.

**הרצה ואימות:** Build Solution; להפעיל Migration על מסד פיתוח ריק; לבדוק מפתחות, אינדקסים, check constraints ו־rowversion. לבדוק שמסד הנתונים דוחה אימייל כפול, זוג RideId/UserId כפול וקיבולת שאינה חיובית. אין להפעיל Migration אוטומטי על מסד שאינו סביבת פיתוח ללא החלטה נפרדת.

### שלב 4 — שירותים וכללים עסקיים

**מטרה:** לממש את פעולות הנסיעה וההרשמה באופן שאינו תלוי ב־HTTP.

**מיקום וקבצים:** TripLink.Service/DTOs/CreateRideRequest.cs, UpdateRideRequest.cs, RideResponse.cs, RegistrationResponse.cs; Interfaces/IRideService.cs ו־IRegistrationService.cs; Services/RideService.cs ו־RegistrationService.cs. בדיקות ראשונות ב־TripLink.Tests/Unit/RideServiceTests.cs וב־RegistrationServiceTests.cs.

**מחלקות ומתודות:** IRideService ו־RideService עם CreateAsync(), GetByIdAsync(), SearchAsync(), UpdateAsync(), CancelAsync(); IRegistrationService ו־RegistrationService עם RegisterAsync(), CancelAsync(), GetMineAsync(). להוסיף GetMineAsync() גם לחוזה המאגר ולמימושו עבור הרשמות המשתמש. להעביר userId מההקשר המאומת בפרמטר מפורש; אין לקבל זהות משתמש אמינה מגוף הבקשה.

לממש בדיקת נסיעה עתידית, קיבולת חיובית, שדות חובה, בעלות, חסימת הרשמה עצמית, מניעת הרשמה כפולה והגבלת קיבולת. הרשמה, ביטול ושינוי קיבולת יבוצעו בתוך ExecuteSerializableAsync(). בהפעלה מחדש של הרשמה מבוטלת, לעדכן אותה במקום להוסיף זוג כפול. לעדכן UpdatedAtUtc של Ride בפעולות המשנות זמינות כדי שכל כותבי הזמינות ישתתפו גם בבקרת rowversion.

**נימוק:** שירות מרכזי מחיל את אותו כלל על כל נקודת כניסה, והעסקה מכסה את הבדיקה ואת הכתיבה יחד.

**Visual Studio:** ליצור את הקבצים בתיקיות המוגדרות, להשתמש ב־Implement Interface ולרשום את השירותים כ־Scoped ב־Program.cs בשלב החיבור. ליצור mocks או fakes לממשקי Core בבדיקות יחידה.

**הרצה ואימות:** דרך Test Explorer לבדוק יצירה תקינה, תאריך עבר, בעלות שגויה, נסיעה מלאה, הרשמה עצמית, הרשמה כפולה, ביטול והפעלה מחדש. בדיקות יחידה אינן הוכחה לנכונות העסקאות במסד; זו תיבדק בשלב 8.

### שלב 5 — משתמשים, JWT והרשאות

**מטרה:** לזהות משתמשים בצורה מאומתת ולהגן על פעולות לפי בעלות.

**מיקום וקבצים:** TripLink.Service/DTOs/RegisterRequest.cs, LoginRequest.cs, AuthResponse.cs; Interfaces/IAuthService.cs ו־ITokenService.cs; Services/AuthService.cs; Security/JwtTokenService.cs ו־JwtOptions.cs; TripLink.API/Controllers/AuthController.cs, Program.cs ו־appsettings.json; TripLink.Tests/Unit/AuthServiceTests.cs.

**מחלקות ומתודות:** IAuthService ו־AuthService עם RegisterAsync() ו־LoginAsync(); ITokenService ו־JwtTokenService עם CreateToken(); AuthController עם Register() ו־Login(). להשתמש ב־PasswordHasher<User> דרך IPasswordHasher<User> עם HashPassword() ו־VerifyHashedPassword(). לא לכתוב אלגוריתם גיבוב עצמאי. להוסיף חבילות JWT ו־password hashing תואמות למסגרת שנבחרה.

לנרמל אימייל לפני בדיקה ושמירה; לאכוף ייחודיות במסד גם בעת הרשמות מקבילות. להפיק token הכולל sub ו־jti וזמן תפוגה. להגדיר issuer, audience ומשך חיים ב־JwtOptions; לשמור signing key ב־User Secrets לפיתוח או במנגנון סודות של סביבת ההרצה. להגדיר אימות חתימה, issuer, audience ותפוגה. אין לתעד סיסמאות או tokens.

**נימוק:** JWT מזהה את הקורא; בדיקות הבעלות בשירות קובעות אם הוא רשאי לשנות את המשאב. הפרדת השניים מונעת הסתמכות על userId שמגיע מהלקוח.

**Visual Studio:** להגדיר User Secrets דרך תפריט פרויקט API; להוסיף AddAuthentication().AddJwtBearer() ו־AddAuthorization() ב־Program.cs; להוסיף UseAuthentication() לפני UseAuthorization(). להגדיר Bearer ב־Swagger. לסמן פעולות מוגנות ב־Authorize ואת פעולות ההתחברות ב־AllowAnonymous.

**הרצה ואימות:** ליצור משתמש, להתחבר ולהפעיל פעולה עם Bearer token. לבדוק 401 ללא token, עם חתימה שגויה ועם token שפג תוקפו; לבדוק 403 כאשר משתמש אחר מנסה לשנות נסיעה. לוודא שהתגובה אינה מכילה PasswordHash. תפקידי משתמשים ו־refresh tokens נשארים שאלות פתוחות.

### שלב 6 — טיפול בשגיאות ו־NLog

**מטרה:** להחזיר שגיאות עקביות ולתעד מידע שימושי לפיתוח ותפעול.

**מיקום וקבצים:** TripLink.API/Middleware/ExceptionHandlingMiddleware.cs, TripLink.API/nlog.config, TripLink.API/Program.cs ו־appsettings.json; מחלקות השגיאה ב־TripLink.Core/Exceptions; TripLink.Tests/Integration/ErrorHandlingTests.cs.

**מחלקות ומתודות:** ExceptionHandlingMiddleware עם InvokeAsync(HttpContext). להשתמש ב־ProblemDetails וב־ValidationProblemDetails; למפות NotFoundException ל־404, ForbiddenException ל־403, BusinessValidationException ל־400 ו־ConflictException ל־409. חריגה לא צפויה תחזיר 500 עם הודעה כללית ו־traceId. לא להחזיר stack trace או פרטי SQL. ביטול בקשה מצד הלקוח אינו תקלה עסקית ואין לרשום אותו אוטומטית כ־500.

להוסיף NLog.Web.AspNetCore, לחבר UseNLog() ולתעד באמצעות ILogger<T>. להגדיר console ו־rolling file ב־nlog.config עם timestamp, level, logger, message, exception ו־traceId; להגדיר שמירה מוגבלת של קובצי log ולוודא שהתצורה מועתקת לפלט. נתיב הכתיבה חייב להיות ניתן לכתיבה בסביבת ההרצה. לא לתעד Authorization header, סיסמאות, signing key או גוף בקשת התחברות.

**נימוק:** middleware מרכז את טיפול החריגות; שימוש ב־ILogger משאיר את השירותים בלתי תלויים בספק התיעוד.

**Visual Studio:** להתקין NLog.Web.AspNetCore באמצעות NuGet ולהוסיף nlog.config. למקם את middleware לפני רכיבי הבקשה שעלולים לזרוק חריגות. סדר עקרוני: ExceptionHandlingMiddleware, HTTPS redirection, Authentication, Authorization, MapControllers. לבדוק גם את תשובות 401/403 שמפיק מנגנון האימות, שאינן בהכרח חריגות; אם נדרש פורמט אחיד עבורן, להגדיר handlers או StatusCodePages מתאימים.

**הרצה ואימות:** להפעיל בקשות הגורמות ל־400, 404 ו־409; בבדיקת שילוב לדמות חריגה לא צפויה ולבדוק 500. לוודא התאמה בין traceId בתגובה לרישום, ושאין מידע רגיש. לבדוק כתיבה וגלגול של logs בסביבת פיתוח.

### שלב 7 — API וחיבור המערכת

**מטרה:** לחשוף את מקרי השימוש דרך HTTP ולחבר את כל השכבות.

**מיקום וקבצים:** TripLink.API/Controllers/RidesController.cs, RegistrationsController.cs ו־AuthController.cs; Program.cs; TripLink.API.http; הגדרות Swagger. בשלב זה בלבד להסיר את WeatherForecast.cs ואת Controllers/WeatherForecastController.cs שהועברו מהפרויקט הקיים, ולעדכן קובץ HTTP ששמו הקודם TripLink.http.

**מחלקות ומתודות:** RidesController עם Search(), GetById(), Create(), Update(), Cancel(); RegistrationsController עם Register(), Cancel(), GetMine(); AuthController עם Register() ו־Login(). Controllers יקראו לשירותים, יעבירו CancellationToken ויחלצו userId מאומת מ־sub. ליצור מיפוי מפורש של claim כדי לא להיות תלויים בשינוי אוטומטי של שמות claims.

לרשום DbContext, repositories, UnitOfWork, services, token service ו־password hasher ב־DI. להוסיף ולידציה ל־DTOs וגבולות ל־pageSize. להחזיר DTOs בלבד, ללא EF entities. Create יחזיר 201 עם Location למשאב; ביטול ועדכון יחזירו 204 לפי טבלת API.

**נימוק:** חוזה HTTP ברור מצמצם תלות של הלקוח במבנה הנתונים ומאפשר בדיקה עצמאית של שירותים.

**Visual Studio:** Add > Controller > API Controller - Empty; להוסיף attribute routing ו־Authorize לפי הטבלה. להגדיר API כ־Startup Project, להריץ F5 ולבדוק ב־Swagger ובקובץ HTTP עם tokens של שני משתמשים.

**הרצה ואימות:** תרחיש מלא של יצירת שני משתמשים, התחברות, יצירת נסיעה, חיפוש, הרשמה וביטול. לבדוק status codes, Location, pagination, כללי הרשאה והיעדר קובצי הדוגמה בחוזה Swagger.

### שלב 8 — בדיקות שילוב ומקביליות

**מטרה:** להוכיח שה־API, ההרשאות ומסד הנתונים פועלים יחד, ובפרט שאין חריגה מקיבולת.

**מיקום וקבצים:** TripLink.Tests/Integration/TripLinkApiFactory.cs, TestDatabaseFixture.cs, RidesApiTests.cs, AuthApiTests.cs, ErrorHandlingTests.cs ו־RegistrationConcurrencyTests.cs; התאמה ב־TripLink.API/Program.cs לחשיפת Program לבדיקות באמצעות partial class לפי הצורך.

**מחלקות ומתודות:** TripLinkApiFactory על בסיס WebApplicationFactory<Program>; TestDatabaseFixture עם InitializeAsync() ו־DisposeAsync(); RegistrationConcurrencyTests עם RegisterAsync_WhenLastSeatRequestedConcurrently_AllowsExactlyOne(), RegisterAsync_WhenSameUserRequestsConcurrently_CreatesOneActiveRegistration() ו־CancelAndRegisterAsync_WhenConcurrent_PreservesCapacity(). שמות מתודות הבדיקה האחרות יתארו תרחיש ותוצאה.

כל בדיקת מקביליות תשתמש ב־SQL Server ייעודי לבדיקות, ב־HTTP requests נפרדות וב־DbContext נפרד לכל בקשה. לנסיעה עם מקום אחד לשלוח במקביל בקשות של שני משתמשים שונים, עם מחסום התחלה וללא Thread.Sleep כתיאום. לצפות להצלחה אחת בלבד ול־409 בשנייה; לבדוק במסד עצמו הרשמה פעילה אחת וקיבולת תקינה. אין לשתף DbContext בין threads. לבדוק גם שני ניסיונות של אותו משתמש, ביטול מול הרשמה ושינוי קיבולת מול הרשמה.

**נימוק:** fakes ו־InMemory אינם מפעילים את הנעילות והאילוצים של SQL Server, ולכן אינם מאמתים את התנהגות המקביליות המתוכננת.

**Visual Studio:** להתקין Microsoft.AspNetCore.Mvc.Testing וחבילות xUnit תואמות. להגדיר connection string ייעודי לבדיקות ולהחיל Migrations ב־fixture. להריץ דרך Test Explorer. ליצור מסד בדיקה מזוהה ומבודד ולהגביל ניקוי לנתוני הבדיקה שלו בלבד.

**הרצה ואימות:** להריץ את כל הבדיקות, ולהריץ את בדיקות המקביליות במספר איטרציות מוגבל לגילוי תזמונים שונים. להפעיל timeout כדי שכשל נעילה לא יתקע את ההרצה. לוודא שאין 500 עקב deadlock צפוי: retry מוגבל צריך לקרוא מחדש את המצב ולהחזיר 409 כשהמקום נתפס. לתעד במימוש את מדיניות retry ואת גבולותיה.

### שלב 9 — אימות סופי ותיעוד הפעלה

**מטרה:** לוודא שניתן להפעיל את הפתרון בסביבת פיתוח חדשה לפי דרישות הקורס.

**מיקום וקבצים:** חמשת הפרויקטים הקיימים בשלב זה, קובצי התצורה, TripLink.API.http ו־PROJECT_PLAN.md לעדכון סטטוס עתידי. לא להוסיף מסמך נוסף ללא צורך או אישור במסגרת העבודה העתידית.

**מחלקות ומתודות:** אין רכיבים חדשים מתוכננים; תיקונים רק לפי תוצאות האימות.

**נימוק:** אימות התקנה והפעלה חושף תלות בסודות מקומיים או במסד שכבר הוכן ידנית.

**Visual Studio:** Restore NuGet Packages, Build Solution, הגדרת User Secrets, Update-Database והרצת API. להריץ Test Explorer ולבצע את תרחיש HTTP המלא.

**הרצה ואימות:** פתרון נבנה, מסד ריק מוקם, כל הבדיקות עוברות, Swagger זמין בפיתוח, logs נכתבים, וסודות אינם נמצאים בקבצי המקור. לוודא שהפתרון מכיל את שמות הפרויקטים המדויקים הנדרשים בקורס.

## 6. תכנון מסד הנתונים

### Users

| שדה | טיפוס מוצע | משמעות |
|---|---|---|
| Id | uniqueidentifier | מפתח ראשי |
| DisplayName | nvarchar(100) | שם להצגה |
| Email | nvarchar(256) | אימייל מקורי |
| NormalizedEmail | nvarchar(256) | אימייל מנורמל עם unique index |
| PasswordHash | nvarchar(max) | גיבוב סיסמה בלבד |
| CreatedAtUtc | datetimeoffset | זמן יצירה |

### Rides

| שדה | טיפוס מוצע | משמעות |
|---|---|---|
| Id | uniqueidentifier | מפתח ראשי |
| DriverId | uniqueidentifier | מפתח זר אל Users |
| Origin | nvarchar(200) | מוצא |
| Destination | nvarchar(200) | יעד |
| DepartureTimeUtc | datetimeoffset | מועד יציאה |
| Capacity | int | מספר מקומות לנוסעים, ללא הנהג |
| Status | int | Active או Cancelled |
| CreatedAtUtc | datetimeoffset | זמן יצירה |
| UpdatedAtUtc | datetimeoffset | זמן שינוי |
| RowVersion | rowversion | זיהוי עדכון מתחרה |

### Registrations

| שדה | טיפוס מוצע | משמעות |
|---|---|---|
| Id | uniqueidentifier | מפתח ראשי |
| RideId | uniqueidentifier | מפתח זר אל Rides |
| UserId | uniqueidentifier | מפתח זר אל Users |
| Status | int | Active או Cancelled |
| CreatedAtUtc | datetimeoffset | זמן יצירת הרשומה |
| UpdatedAtUtc | datetimeoffset | זמן שינוי או הפעלה מחדש |
| CancelledAtUtc | datetimeoffset, nullable | זמן ביטול אחרון |
| RowVersion | rowversion | זיהוי שינוי מתחרה |

### קשרים ואילוצים

- User אחד יכול לפרסם Rides רבים ולהחזיק Registrations רבות; Ride אחד מכיל Registrations רבות.
- unique index על Registrations(RideId, UserId), כולל הרשמות מבוטלות. הפעלה מחדש משתמשת באותה רשומה. התכנון אינו שומר היסטוריית אירועים מלאה; צורך כזה מחייב הרחבה עתידית.
- check constraint עבור Capacity > 0 ועבור ערכי Status מוכרים.
- אינדקס על Registrations(RideId, Status) לתמיכה בספירת מקומות ובטווח נעילה של Serializable; אינדקס על UserId לשליפת הרשמות משתמש.
- אינדקס חיפוש מוצע על Rides(Status, DepartureTimeUtc), ואינדקס על DriverId. התאמה לסינון מוצא ויעד תתבצע לפי אופן החיפוש והיקף הנתונים.
- מחיקה פיזית אינה חלק מה־API הראשוני; קשרים ישתמשו ב־Restrict כדי למנוע מחיקה מקרית של נתונים קשורים.
- AvailableSeats יחושב כ־Capacity פחות מספר Registrations במצב Active; לא יישמר מונה נוסף בשלב הראשון.

אילוץ רגיל אינו יכול לאכוף ספירה בין מספר טבלאות. לכן כל פעולה שמשפיעה על זמינות תבוצע בעסקת Serializable: לקרוא Ride, לוודא מצב וזמן, לקרוא הרשמה קיימת, לספור Active, לבצע שינוי ולשמור לפני Commit. גם הפחתת קיבולת וביטול נסיעה חייבים להשתמש באותו מסלול עסקה. rowversion לבדו אינו מונע שתי הוספות לרשומות שונות.

במקרה של deadlock או סכסוך עדכון, יש rollback, retry מוגבל עם DbContext נקי וקריאה מחדש. אם המצב החדש אינו מאפשר את הפעולה, להחזיר 409. אין לבצע retry בלתי מוגבל או להשתמש בתוצאה ישנה של ספירת המקומות.

## 7. תכנון API

הנתיבים הבאים הם חוזה מוצע. מידע אישי של נוסעים לא ייכלל בתשובת נסיעה ציבורית.

| HTTP | נתיב | מתודת Controller | הרשאה | תוצאה תקינה |
|---|---|---|---|---|
| POST | /api/auth/register | AuthController.Register() | ציבורי | 201 עם מזהה משתמש ופרטים בטוחים |
| POST | /api/auth/login | AuthController.Login() | ציבורי | 200 עם AuthResponse |
| GET | /api/rides | RidesController.Search() | ציבורי, בכפוף לאישור | 200 עם רשימה מדופדפת |
| GET | /api/rides/{rideId} | RidesController.GetById() | ציבורי, בכפוף לאישור | 200 עם RideResponse |
| POST | /api/rides | RidesController.Create() | משתמש מחובר | 201 עם RideResponse ו־Location |
| PUT | /api/rides/{rideId} | RidesController.Update() | בעל הנסיעה | 204 |
| DELETE | /api/rides/{rideId} | RidesController.Cancel() | בעל הנסיעה | 204; ביטול לוגי |
| POST | /api/rides/{rideId}/registrations | RegistrationsController.Register() | משתמש מחובר | 201 עם RegistrationResponse ו־Location |
| GET | /api/registrations/mine | RegistrationsController.GetMine() | משתמש מחובר | 200 עם הרשמותיו |
| GET | /api/registrations/{registrationId} | RegistrationsController.GetById() | בעל ההרשמה | 200 עם RegistrationResponse |
| DELETE | /api/registrations/{registrationId} | RegistrationsController.Cancel() | בעל ההרשמה | 204; ביטול לוגי |

לצורך נתיב Location של הרשמה, להוסיף GetByIdAsync() ל־IRegistrationService ול־RegistrationService ו־GetById() ל־RegistrationsController בשלב 7. בהרשמה מחדש תחזור הרשומה הקיימת; יש לתעד ב־Swagger אם חוזה התגובה יהיה 200 להפעלה מחדש במקום 201, ולהתאים את בדיקות החוזה.

CreateRideRequest ו־UpdateRideRequest יכללו Origin, Destination, DepartureTimeUtc ו־Capacity. זהות הנהג נגזרת מה־token. בקשת הרשמה לנסיעה אינה צריכה UserId או מספר מקומות. RideResponse יכלול Id, DriverId, Origin, Destination, DepartureTimeUtc, Capacity, AvailableSeats ו־Status. RegistrationResponse יכלול Id, RideId, Status ותאריכים, ללא נתונים אישיים מיותרים.

חיפוש יקבל origin, destination, fromUtc, toUtc, page ו־pageSize, עם מיון יציב לפי DepartureTimeUtc ואז Id. ברירת מחדל מוצעת: page=1, pageSize=20 ותקרה של 100. יש להחליט אם התאמת מוצא ויעד היא מדויקת או חלקית. להצגת pagination אפשר להוסיף TripLink.Service/DTOs/PagedResponse.cs בשלב 4, ולהגדיר תוצאת חיפוש מקבילה ב־TripLink.Core/Contracts כדי שהמאגר לא יחזיר DTO של שכבת השירות.

קודי שגיאה: 400 עבור בקשה לא תקינה; 401 עבור זיהוי חסר או לא תקין; 403 עבור בעלות חסרה; 404 עבור משאב שאינו קיים; 409 עבור אימייל כפול, הרשמה פעילה כפולה, נסיעה מלאה או שינוי שמתנגש במצב הנוכחי; 500 עבור תקלה לא צפויה. תגובות השגיאה יכללו type, title, status ו־traceId; הודעות ולידציה יכולות לכלול errors.

## 8. כללים עסקיים

1. יצירת נסיעה דורשת משתמש מחובר, מוצא ויעד לא ריקים, קיבולת חיובית ומועד עתידי.
2. רק הנהג יכול לעדכן או לבטל את נסיעתו. לא ניתן להפחית Capacity מתחת למספר ההרשמות הפעילות.
3. לא ניתן להירשם לנסיעה מבוטלת או לנסיעה שכבר יצאה, ולא ניתן לנהג להירשם לנסיעתו.
4. לכל משתמש לכל היותר הרשמה פעילה אחת לכל נסיעה. הרשמה מבוטלת ניתנת להפעלה מחדש אם נותר מקום.
5. הרשמה למקום האחרון חייבת להיות אטומית. בדיקת קיבולת מחוץ לעסקה אינה מספיקה.
6. ביטול הרשמה מפנה מקום. ביטול חוזר יהיה idempotent ויחזיר 204, כל עוד הקורא מורשה.
7. ביטול נסיעה ישנה את RideStatus ל־Cancelled ויבטל את הרשמותיה הפעילות באותה עסקה. לצורך כך להוסיף GetActiveByRideAsync() לממשק ולמימוש מאגר ההרשמות בשלב 4.
8. שינוי מסלול או מועד כאשר יש נוסעים, ביטול לאחר יציאה וגבולות זמן להרשמה דורשים החלטת מוצר. עד להכרעה מומלץ לדחות שינויים מהותיים לאחר הרשמה ולהימנע מהמצאת מדיניות שקטה.
9. אין לשלוח הודעות או לבצע פעולות חיצוניות מתוך עסקה במסד הנתונים. התראות אינן חלק מההיקף הראשוני.

## 9. אסטרטגיית בדיקות

| תחום | תרחישים עיקריים |
|---|---|
| RideServiceTests | יצירה, ולידציה, בעלות, עדכון קיבולת וביטול |
| RegistrationServiceTests | קיבולת, כפילות, הרשמה עצמית, ביטול והפעלה מחדש |
| AuthServiceTests | גיבוב ואימות סיסמה, אימייל מנורמל כפול וכישלון התחברות |
| RidesApiTests | HTTP contracts, DTOs, pagination ו־Location |
| AuthApiTests | חתימה, תפוגה, issuer, audience, 401 ו־403 |
| ErrorHandlingTests | ProblemDetails, מיפוי חריגות ואי־חשיפת מידע פנימי |
| RegistrationConcurrencyTests | המקום האחרון, אותו משתמש, ביטול ושינוי קיבולת במקביל |

בדיקות יחידה יתמקדו בכללים עסקיים ולא ישכפלו פרטי מימוש. בדיקות שילוב יאמתו אינדקסים, עסקאות, HTTP ואימות אמיתי. כל בדיקה תבודד את נתוניה; אין להשתמש במסד פיתוח או במסד משתמשים אמיתי לצורך ניקוי בדיקות.

## 10. שאלות פתוחות לפני המימוש

1. האם הקורס מחייב SQL Server, גרסת .NET מסוימת, כלי בדיקות מסוים או תבנית Repository מסוימת?
2. האם לשנות את TripLink הקיים ל־TripLink.API או ליצור פרויקט חדש ולהעביר את הדרוש? בשלב התכנון לא מבוצעת אף אפשרות.
3. האם חיפוש נסיעות ציבורי או מחייב התחברות? אילו פרטי נהג מותר להציג?
4. האם משתמשים מתחלקים לתפקידים כגון Driver, Passenger ו־Admin, או שכל משתמש רשאי לפרסם ולהירשם?
5. האם הרשמה תופסת מקום אחד, או שנדרשת הזמנת מספר מקומות? אם מספר מקומות, צריך לעדכן סכימה, חישובי קיבולת ובדיקות.
6. האם נדרשים מחיר, רכב, נקודות איסוף, מיקום גאוגרפי, חיפוש חלקי או סינון נוסף?
7. האם לאפשר שינוי מסלול ומועד לאחר הרשמה? מהו מועד הביטול האחרון ומדיניות ביטול נסיעה?
8. האם נדרשים refresh tokens, אימות אימייל, איפוס סיסמה או חסימת ניסיונות התחברות? אלה אינם כלולים אוטומטית בתכנון הראשוני.
9. האם יש לשמור היסטוריה מלאה של ביטולים והרשמות חוזרות, מעבר למצב הנוכחי ולתאריכי השינוי?
10. האם נדרשים הצגת רשימת נוסעים לנהג, התראות או אישור נהג להרשמה? אם כן, יש להרחיב את חוזי השירות וההרשאות.
11. מהן סביבת הפריסה, מדיניות שמירת logs, מנגנון הסודות וסביבת SQL Server לבדיקות אוטומטיות?
12. האם הקורס מכתיב מדיניות מסוימת למקביליות? אם כן, להתאים את מנגנון העסקאות תוך שמירת בדיקת המקום האחרון כתנאי קבלה.