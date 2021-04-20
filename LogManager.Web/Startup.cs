using FluentValidation;
using LogManager.BLL.Services;
using LogManager.BLL.Utilities;
using LogManager.BLL.Validation;
using LogManager.Core.Abstractions.BLL;
using LogManager.Core.Abstractions.DAL;
using LogManager.Core.Settings;
using LogManager.DAL.Contexts;
using LogManager.DAL.Factories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LogManager.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddHttpContextAccessor();

            services.Configure<RequestSettings>(this.Configuration.GetSection("RequestSettings"));
            services.Configure<ConnectionSettings>(this.Configuration.GetSection("ConnectionSettings"));
            services.Configure<FileSettings>(this.Configuration.GetSection("FileSettings"));
            services.Configure<PageSettings>(this.Configuration.GetSection("PageSettings"));

            services.AddSingleton<IDbContextFactory<LogManagerDbContext>, DbContextFactory>();
            services.AddSingleton<IRepositoryFactory, RepositoryFactory>();

            services.AddSingleton<ILogParser<ParsedLogEntry>, LogParser>();
            services.AddSingleton<IValidator<ParsedLogEntry>, LogEntryValidator>();
            services.AddSingleton<WebHelper>();

            services.AddScoped<ILogService, LogService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=LogEntry}/{id?}");
            });
        }
    }
}
