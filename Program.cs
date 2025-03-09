using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace MyCompany
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            {
                var cultureInfo = new CultureInfo("en-US"); // Используем культуру с точкой как разделителем
                CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
                CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
                builder.Services.AddRazorPages();
                builder.Services.AddDbContext<DB>(options =>
                    options.UseNpgsql(DB.GetConnectionString));
            }

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Factory}/{action=List}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
