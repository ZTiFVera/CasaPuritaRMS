using CasaPuritaRMS.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add MVC
builder.Services.AddControllersWithViews();

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
    app.UseExceptionHandler("/Room/Index");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

// Default page is now Room Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Room}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();