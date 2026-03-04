using DealtHands.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Register services
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<GameEngine>();
builder.Services.AddScoped<FinancialCalculator>();
builder.Services.AddScoped<GameChangerService>();
//builder.Services.AddScoped<AIPricingService>();



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
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
