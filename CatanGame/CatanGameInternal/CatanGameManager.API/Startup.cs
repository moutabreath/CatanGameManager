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
using System.IO;
using System.Linq;

namespace CatanGameManager.API
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
            services.Configure<MongoConfig>(Configuration.GetSection("MongoConfig"));
            services.Configure<GameManagerConfig>(Configuration.GetSection("GameManagerConfig"));

            services.AddScoped<ICatanGameBusinessLogic, CatanGameBusinessLogic>();
            services.AddScoped<ICatanGamePersist, CatanGameMongoPersist>();
            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Internal Game API WSVAP (WebSmartView)", Version = "v1" });
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            string path = Directory.GetCurrentDirectory();
            loggerFactory.AddFile($"{path}\\Logs\\InternalCatanGameLog.log");

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

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
