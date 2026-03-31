using Microsoft.EntityFrameworkCore;
using TVTrack.Data;
using TVTrack.Models;
using TVTrack.Models.Repos;
using TVTrack.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC views + JSON endpoints both get used in this project,
// so keep the JSON naming consistent for the API responses.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<AppUser>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddHttpClient<ITmdbService, TmdbService>();
builder.Services.AddScoped<ShowRepository>();

var app = builder.Build();

// Apply migrations on startup so a fresh clone can create/update the local DB
// without manually opening SQL Server first.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
