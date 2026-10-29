using ReportSystem.Data;
using ReportSystem.Services;
using QuestPDF.Infrastructure;

namespace ReportSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddSingleton<SqlConnectionFactory>();

            builder.Services.AddScoped<IReportingService, ReportingService>();

            builder.Services.AddScoped<ExcelExportService>();

			builder.Services.AddScoped<HtmlExportService>();

			builder.Services.AddScoped<DocxExportService>();

			builder.Services.AddScoped<CsvExportService>();

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
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
