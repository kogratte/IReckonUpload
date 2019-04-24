using IReckonUpload.DAL;
using IReckonUpload.DAL.Consumers;
using IReckonUpload.DAL.Products;
using IReckonUpload.Extensions;
using IReckonUpload.Models.Business;
using IReckonUpload.Models.Configuration;
using IReckonUpload.Models.Consumers;
using IReckonUpload.Tools;
using IReckonUpload.Uploader;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;

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

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Latest);
            services.Configure<AppConfigurationOptions>(appConfig);
            services.AddJwtAuthentication(appConfig);
            services.ConfigureDatabase(Configuration.GetConnectionString("DefaultConnection"));

            if (Environment.IsDevelopment())
            {
                services.AddTransient<IRepository<Consumer>, FakeConsumerRepository>();
            }
            else
            {
                services.AddSingleton<IRepository<Consumer>, ConsumerRepository>();
            }

            services.AddSingleton<IRepository<Product>, ProductRepository>();
            services.AddSingleton<IUploader, IReckonUpload.Uploader.Uploader>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = appConfig["ApplicationName"] + " API", Version = "v1" });
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
