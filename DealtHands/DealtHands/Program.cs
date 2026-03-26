using DealtHands.Data;
using DealtHands.Services;
using Microsoft.EntityFrameworkCore;


// This is the Program.cs file

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddControllers();

// Register services
builder.Services.AddScoped<FinancialCalculator>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<GameSessionService>();
builder.Services.AddSingleton<SessionTracker>(); // Must be singleton

// V2 database context
builder.Services.AddDbContext<DealtHandsDbv2Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DealtHandsDBV2")));

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
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