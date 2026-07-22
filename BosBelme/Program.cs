using One_Shot_Bounce.Engine;

var builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("PostgresConnection")
    ?? throw new InvalidOperationException("Нет ссылки на строку подключения");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthorization();

//Подключение СигналР
builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        // Настраиваем наш собсвенный конвертор для передачи JSON данных
        options.PayloadSerializerOptions.Converters.Add(new Vector2JsonConverter());
    });

// Внедрение зависимостей для сервисов
builder.Services.AddScoped<IAuthService, Authentication>();
builder.Services.AddScoped<IRegService, Registration>();
builder.Services.AddScoped<ICookieAuthService, CookieAuthService>();
builder.Services.AddScoped<IRoomService, RoomService>();

// Интеграция игры
builder.Services.AddSingleton<IBounceGameManager, BounceGameManager>();

// Настройка куки
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

//Настройка сигналР
app.MapHub<GameRoomHub>("/gameRoomHub");
app.MapHub<BounceHub>("/bouncehub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Автоматическая миграция бд
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        context.Database.Migrate();
        Console.WriteLine("Миграции применина");

        BosBelme.Service.Service.DbInitializer.Seed(context);
    }
    catch (Exception ex) { Console.WriteLine($"Ошибка миграции - {ex.Message}"); }
}

// Подключение прометеуса
app.UseHttpMetrics();
app.MapMetrics();

app.Run();
