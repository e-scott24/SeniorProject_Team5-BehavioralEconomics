var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();

// Required for FinancialsController and any other API controllers
builder.Services.AddControllers();

// Session setup is used by FinancialsController
// DistributedMemoryCache is the in-process backing store (replace later)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// UseSession must come before MapControllers so that the session is available in controllers
app.UseSession();

app.UseAuthorization();
app.MapStaticAssets();
app.MapControllers();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
