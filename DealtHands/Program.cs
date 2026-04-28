using DealtHands.Data;
using DealtHands.Services;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddControllers();

// Register HTTP Context Accessor (required for AuthenticationService)
builder.Services.AddHttpContextAccessor();

// Register services
builder.Services.AddScoped<FinancialCalculator>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<GameSessionService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>(); // Add authentication service
builder.Services.AddSingleton<SessionTracker>(); // Must be singleton

// V2 database context
builder.Services.AddDbContext<DealtHandsDbv2Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DealtHandsDBV2")));

// Session configuration
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // For localhost development
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.Name = ".DealtHands.Session"; // Give session cookie a specific name
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession(); // Must be after UseRouting and before UseAuthorization
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllers();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();