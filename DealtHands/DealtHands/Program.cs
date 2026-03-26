using DealtHands.Data;
using DealtHands.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers(); // <-- add this

// Register services
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<GameEngine>();
builder.Services.AddScoped<FinancialCalculator>();
builder.Services.AddScoped<GameChangerService>();
//builder.Services.AddScoped<AIPricingService>();
builder.Services.AddScoped<EducatorService>();

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<GameSessionService>();


/*
// Add database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
*/

// v2
builder.Services.AddDbContext<DealtHandsDbv2Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DealtHandsDBV2")));


//for calculator
builder.Services.AddControllers();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

// Map API controllers
app.MapControllers(); // <-- add this

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
