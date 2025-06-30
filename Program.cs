using TechNova.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();

// Đăng ký EF Core với đúng tên chuỗi kết nối
builder.Services.AddDbContext<StoreDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StoreDbConnection")));

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<EmailService>();

builder.Services.AddSession();

var app = builder.Build();

app.UseSession(); // Trước app.UseRouting()

app.UseStaticFiles();
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

    endpoints.MapDefaultControllerRoute();
});

// Gọi hàm seed data trong một scope
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    Seeddata.Initialize(services);
}

app.MapDefaultControllerRoute();
app.Run();
