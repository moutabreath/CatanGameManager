using CatanGameManager.CommonObjects.Config;
using CatanGameManager.Core;
using CatanGameManager.Interfaces;
using CatanGameManager.Interfaces.PersistanceInterfaces;
using CatanGamePersistence.MongoDB;
using CommonLib.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.IO;
using System.Linq;

namespace CatanGameManager.API
{
    public class Startup(IConfiguration configuration)
    {
        public IConfiguration Configuration { get; } = configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<MongoConfig>(Configuration.GetSection("MongoConfig"));
            services.Configure<GameManagerConfig>(Configuration.GetSection("GameManagerConfig"));

            services.AddScoped<ICatanGameBusinessLogic, CatanGameBusinessLogic>();
            services.AddScoped<ICatanGamePersist, CatanGameMongoPersist>();
            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Internal Game Manager API", Version = "v1" });
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            string path = Directory.GetCurrentDirectory();
            Log.Logger = new LoggerConfiguration()
                      .MinimumLevel.Debug()
                      .Enrich.FromLogContext()
                      .WriteTo.File($"{path}\\Logs\\InternalCatanGameLog.log",
                      outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
                     .CreateLogger();

            // Connect the built-in logger factory to Serilog
            loggerFactory.AddSerilog();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Internal Game API V1");
                    c.RoutePrefix = string.Empty;
                });
            }

            //app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
