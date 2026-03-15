using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Sportify.Data;
using Sportify.Repositories;
using Sportify.Services;

namespace Sportify
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = args,
                ContentRootPath = AppContext.BaseDirectory,
                WebRootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot")
            });

            // Configure Database Connection
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Cookie tabanlı kimlik doğrulama
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                    options.SlidingExpiration = true;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                });

            // Email ve Repository DI kaydı
            builder.Services.Configure<EmailAyarlari>(builder.Configuration.GetSection("EmailAyarlari"));
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();

            // Add services
            builder.Services.AddControllersWithViews();

            // Register HttpClient + SportDataService
            builder.Services.AddHttpClient<ISportDataService, SportDataService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication(); // UseAuthorization'dan ÖNCE olmalı
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
