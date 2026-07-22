using CasaPuritaRMS.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add MVC
builder.Services.AddControllersWithViews();

//builder.Services.AddScoped<CasaPuritaRMS.Services.NotificationService>();

// Add EF Core DbContext
builder.Services.AddDbContext<AppDbContext>(options =>   // ← changed here
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Unit/Index");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

// Default page is now Unit Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Unit}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();