using Hangfire;
using IReckonUpload.Business;
using IReckonUpload.Business.Jobs;
using IReckonUpload.DAL;
using IReckonUpload.Extensions;
using IReckonUpload.Models.Configuration;
using IReckonUpload.Models.Consumers;
using IReckonUpload.Tools;
using IReckonUpload.Uploader;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;
using System.IO;

namespace IReckonUpload
{
   
    public class Startup
    {
        public IHostingEnvironment Environment { get; }

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Environment = env;
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            IConfigurationSection appConfig = Configuration.GetSection("AppConfiguration");
            var connectionString = Configuration.GetConnectionString("DefaultConnection");

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Latest);
            services.Configure<AppConfigurationOptions>(appConfig);
            services.AddJwtAuthentication(appConfig);
            services.ConfigureDatabase(connectionString);
            services.AddSingleton<DbContext, AppDbContext>();
            if (Environment.IsDevelopment())
            {
                services.AddTransient<IRepository<Consumer>, FakeConsumerRepository>();
            }
            else
            {
                services.AddSingleton<IRepository<Consumer>, GenericRepository<Consumer>>();
            }

            services.AddSingleton<IUploader, IReckonUpload.Uploader.Uploader>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = appConfig["ApplicationName"] + " API", Version = "v1" });
                var filePath = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "SwaggerDemo.xml");

                c.IncludeXmlComments(filePath);
            });

            services.AddSingleton<IHangfireWrapper, HangfireWrapper>();

            services.AddHangfire(x => x.UseSqlServerStorage(connectionString));

            services.AddSingleton<IFileToModelConverter, FileToModelConverter>();
            services.AddScoped<IStoreIntoDatabase, StoreIntoDatabase>();
            services.AddScoped<IStoreAsJsonFile, StoreAsJsonFile>();

            services.AddSingleton(new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            IConfigurationSection appConfig = Configuration.GetSection("AppConfiguration");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();
            app.UseAuthentication();
            app.UseHangfireServer();
            app.UseHangfireDashboard("/hangfire", new DashboardOptions { });
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = string.Empty;
                c.SwaggerEndpoint("/swagger/v1/swagger.json", appConfig["ApplicationName"] + " API V1");
            });

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
