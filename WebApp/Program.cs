using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;
using WebApp.Services;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

#region Add services to the container.

services.AddControllersWithViews();
services.AddRazorPages();
services.AddDbContext<AppDbContext>(options =>
{
    string connectionString = configuration.GetConnectionString("TimNguoiGiupViec_DB");
    options.UseSqlServer(connectionString);
});
var mailsetting = configuration.GetSection("MailSettings");
services.Configure<MailSettings>(mailsetting);
services.AddTransient<IEmailSender, EmailService>();

#endregion

#region Identity
services.AddIdentity<AppUser, IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

// Truy cập IdentityOptions
services.Configure<IdentityOptions>(options =>
{
    // Thiết lập về Password
    options.Password.RequireDigit = false; // Không bắt phải có số
    options.Password.RequireLowercase = false; // Không bắt phải có chữ thường
    options.Password.RequireNonAlphanumeric = false; // Không bắt ký tự đặc biệt
    options.Password.RequireUppercase = false; // Không bắt buộc chữ in
    options.Password.RequiredLength = 3; // Số ký tự tối thiểu của password
    options.Password.RequiredUniqueChars = 1; // Số ký tự riêng biệt

    // Cấu hình Lockout - khóa user
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Khóa 5 phút
    options.Lockout.MaxFailedAccessAttempts = 3; // Thất bại 3 lần thì khóa
    options.Lockout.AllowedForNewUsers = true;

    // Cấu hình về User.
    options.User.AllowedUserNameCharacters = // các ký tự đặt tên user
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;  // Email là duy nhất


    // Cấu hình đăng nhập.
    options.SignIn.RequireConfirmedEmail = true;            // Cấu hình xác thực địa chỉ email (email phải tồn tại)
    options.SignIn.RequireConfirmedPhoneNumber = false;     // Xác thực số điện thoại
    options.SignIn.RequireConfirmedAccount = true;

});

services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login/";
    options.LogoutPath = "/logout/";
    options.AccessDeniedPath = "/khongduoctruycap.html";
});

services.AddAuthentication()
        .AddGoogle(options =>
        {
            var gconfig = configuration.GetSection("Authentication:Google");
            options.ClientId = gconfig["ClientId"];
            options.ClientSecret = gconfig["ClientSecret"];
            options.CallbackPath = "/login-google";
        })
        .AddFacebook(options =>
        {
            var fconfig = configuration.GetSection("Authentication:Facebook");
            options.AppId = fconfig["AppId"];
            options.AppSecret = fconfig["AppSecret"];
            options.CallbackPath = "/login-facebook";
        });
#endregion


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();