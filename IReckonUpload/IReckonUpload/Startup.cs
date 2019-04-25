using Hangfire;
using IReckonUpload.Business;
using IReckonUpload.Business.Hangfire;
using IReckonUpload.Business.Jobs;
using IReckonUpload.Business.JobStatusManagement;
using IReckonUpload.Business.ModelConverter;
using IReckonUpload.Business.ModelConverter.Core;
using IReckonUpload.Business.ModelConverter.Middlewares;
using IReckonUpload.DAL;
using IReckonUpload.Extensions;
using IReckonUpload.Jobs;
using IReckonUpload.Models.Configuration;
using IReckonUpload.Models.Consumers;
using IReckonUpload.Models.Internal;
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
            // Always reuse the same.
            services.AddScoped<DbContext, AppDbContext>();
            services.AddSingleton<IHangfireWrapper, HangfireWrapper>();
            services.AddSingleton(new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            // Use on demande.
            services.AddScoped<IAppDbContext, AppDbContext>();
            services.AddScoped<IJobStatusManagementService, JobStatusManagementService>();
            services.AddScoped<IStoreIntoDatabase, StoreIntoDatabase>();
            services.AddScoped<IStoreAsJsonFile, StoreAsJsonFile>();
            services.AddScoped<IDeleteTemporaryFile, DeleteTemporaryFile>();


            services.AddScoped<ITransactionService, TransactionService.TransactionService>();
            services.AddScoped<IRepository<UploadedFile>, GenericRepository<UploadedFile>>();
            services.AddSingleton<IUploader, IReckonUpload.Uploader.Uploader>();
            services.AddSingleton<IFileToModelConverter, FileToModelConverter>();
            services.AddScoped<IImportContentFromFile, ImportContentFromFile>();

            // Register middlewares-
            services.AddTransient<ICheckSourceFileMiddleware, CheckSourceFileMiddleware>();
            services.AddTransient<IStoreIntoDatabase, StoreIntoDatabase>();
            services.AddTransient<IStoreAsJsonFile, StoreAsJsonFile>();

            if (Environment.IsDevelopment())
            {
                services.AddScoped<IRepository<Consumer>, FakeConsumerRepository>();
            }
            else
            {
                services.AddScoped<IRepository<Consumer>, GenericRepository<Consumer>>();
            }

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = appConfig["ApplicationName"] + " API", Version = "v1" });

                if (!Environment.IsEnvironment("IntegrationTest"))
                {

                    var filePath = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "SwaggerDemo.xml");

                    c.IncludeXmlComments(filePath);
                }
            });


            services.AddHangfire(x => x.UseSqlServerStorage(connectionString));
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


            RecurringJob.AddOrUpdate<IDeleteTemporaryFile>(x => x.Execute(), Cron.Minutely);

        }
    }
}
