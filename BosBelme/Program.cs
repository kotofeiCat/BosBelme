
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
builder.Services.AddSignalR();

// Внедрение зависимостей для сервисов
builder.Services.AddScoped<IAuthService, Authentication>();
builder.Services.AddScoped<IRegService, Registration>();
builder.Services.AddScoped<ICookieAuthService, CookieAuthService>();
builder.Services.AddScoped<IRoomService, RoomService>();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
