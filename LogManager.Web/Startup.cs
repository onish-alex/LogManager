using LogManager.BLL.Services;
using LogManager.Core.Settings;
using LogManager.BLL.Utilities;
using LogManager.Core.Abstractions.BLL;
using LogManager.Core.Abstractions.DAL;
using LogManager.DAL.Contexts;
using LogManager.DAL.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LogManager.BLL.Validation;
using FluentValidation;
using LogManager.DAL.Factories;

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

            services.AddDbContext<LogManagerDbContext>(options =>
                options.UseSqlServer(
                   Configuration.GetConnectionString("LogManagerDB")));

            services.Configure<RequestSettings>(this.Configuration.GetSection("RequestSettings"));
            services.Configure<ConnectionSettings>(this.Configuration.GetSection("ConnectionSettings"));

            services.AddScoped<IDbContextFactory<LogManagerDbContext>, DbContextFactory>();
            services.AddScoped<IRepositoryFactory, RepositoryFactory>();

            services.AddScoped<ILogParser<ParsedLogEntry>, LogParser>();
            services.AddScoped<IValidator<ParsedLogEntry>, LogEntryValidator>();
            services.AddScoped<WebHelper>();
            
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
                app.UseExceptionHandler("/Error");
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
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
