using Microsoft.EntityFrameworkCore;
using ThuyTo.Models;
using Microsoft.Extensions.Options;
using AspNetCoreHero.ToastNotification;
using AspNetCoreHero.ToastNotification.Extensions;
using QuestPDF;
using QuestPDF.Infrastructure;
using System.IO;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using Microsoft.AspNetCore.Authentication.Cookies;
using ThuyTo.Models.Service;
internal class Program
{
    static Program()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        FontManager.RegisterFont(File.OpenRead("wwwroot/fonts/Roboto-Regular.ttf"));
        
    }
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
      
        var connection = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new Exception("ERROR");
        builder.Services.AddDbContext<ThuyToContext>(options => options.UseMySql(connection, ServerVersion.AutoDetect(connection)));

        /*var mailsettings = builder.Configuration.GetSection("MailSettings");
        builder.Services.Configure<MailSettings> (mailsettings);*/

        // Add services to the container.
        builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
        // builder.Services.AddSassCompiler();
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(cfg =>
        {
            cfg.Cookie.Name = "ThuyTo";
            cfg.IdleTimeout = new TimeSpan(0, 30, 0);
            cfg.Cookie.HttpOnly = true;
            cfg.Cookie.IsEssential = true;
            cfg.Cookie.SecurePolicy = CookieSecurePolicy.None; // Chỉ dùng trong môi trường phát triển
        });
        builder.Services.AddHttpContextAccessor();
		
		builder.Services.AddNotyf(config => { config.DurationInSeconds = 10; config.IsDismissable = true; config.Position = NotyfPosition.TopRight; });
		builder.Services.AddScoped<IVnPayService, VnPayService>();
		var app = builder.Build();
		
		if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        app.Use(async (context, next) =>
        {
            await next();

            if (context.Response.StatusCode == 404)
            {
                context.Request.Path = "/NotFound";
                // Trả về NotFound hoặc Middleware khác
                await context.Response.WriteAsync("Not Found");
            }
        });
        app.UseSession();
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseNotyf();
        app.UseRouting();
        app.UseAuthentication();
       
        app.UseAuthorization();
        
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
              name: "areas",
              pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
            );
        });
		app.UseEndpoints(endpoints =>
		{
			endpoints.MapControllerRoute(
				name: "search",
				pattern: "tim-kiem",
				defaults: new { controller = "Product", action = "Search" });
		});
		app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
		app.UseEndpoints(endpoints =>
		{
			endpoints.MapControllerRoute(
				name: "vnpay-callback",
				pattern: "Cart/PaymentCallback",
				defaults: new { controller = "Cart", action = "PaymentCallback" });
		});
		app.Run();
    }
}

//Scaffold-DbContext "Server=.\SQLExpress;Database=ThuyTo;Trusted_Connection=True;TrustServerCertificate=True; Connection Timeout=3600" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Force
